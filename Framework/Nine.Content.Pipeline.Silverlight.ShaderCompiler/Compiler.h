// SilverlightShaderBuildHelper.h
#pragma once

#include "d3dx9.h"
#include "stdafx.h"
#include <atlstr.h>
#include <msclr/marshal.h>
#include "CompilerResult.h"

using namespace System;
using namespace System::Text;
using namespace System::IO;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;
using namespace System::Xml;
using namespace System::Runtime::InteropServices;
using namespace System::Text::RegularExpressions;
using namespace msclr::interop; 


BEGIN_NAMESPACE
{
    public ref class Compiler
    {
        String^ _dxLibraryToUse;
        bool _calculatedDxLibraryToUse;

        typedef HRESULT (WINAPI *ShaderCompilerType)
        (
            LPCSTR                          pSrcData,
            UINT                            SrcDataLen,
            CONST D3DXMACRO*                pDefines,
            LPD3DXINCLUDE                   pInclude,
            LPCSTR                          pFunctionName,
            LPCSTR                          pProfile,
            DWORD                           Flags,
            LPD3DXBUFFER*                   ppShader,
            LPD3DXBUFFER*                   ppErrorMsgs,
            LPD3DXCONSTANTTABLE*            ppConstantTable);
        
        String^ GetDxLibraryToUse()
        {
            if (!_calculatedDxLibraryToUse)
            {		
                const int maxIndex = 60;
                const int minIndex = 36;
                bool gotOne = false;
                int index = maxIndex;
                
                // DirectX SDK's install new D3DX libraries by appending a version number.  Here, we start from a high
                // number and count down until we find a library that contains the function we're looking for.  This
                // thus gets us the most recently installed SDK.  If there are no SDKs installed, we _dxLibraryToUse remains
                // NULL and we'll use the statically linked one.
                for (int index = maxIndex; !gotOne && index >= minIndex; index--)
                {
                    String^ libName = "d3dx9_" + index.ToString() + ".dll";
                    CString libNameAsCString(libName);

                    HMODULE dxLibrary = ::LoadLibrary((LPCWSTR)libNameAsCString);
                    if (dxLibrary != NULL)
                    {
                        FARPROC sc = ::GetProcAddress(dxLibrary, "D3DXCompileShader");
                        if (sc != NULL)
                        {
                            gotOne = true;
                            _dxLibraryToUse = libName;
                        }
                        ::FreeLibrary(dxLibrary);
                    }
                }

                _calculatedDxLibraryToUse = true;
            }

            return _dxLibraryToUse;
        }

        array<unsigned char>^ GetConstants(LPD3DXCONSTANTTABLE constantTable, List<String^>^ errors)
        {	
            // Get constant table description
            D3DXCONSTANTTABLE_DESC desc;
            HRESULT hr = constantTable->GetDesc(&desc);

            // Create binary writer
            MemoryStream^ output = gcnew MemoryStream();
            BinaryWriter^ writer = gcnew BinaryWriter(output);

            // Write root element
            writer->Write((unsigned int)desc.Version);
            writer->Write(gcnew String(desc.Creator));
            writer->Write(desc.Constants);

            for(unsigned int c = 0; c < desc.Constants; c++)
            {
                // get constant
                D3DXHANDLE constant = constantTable->GetConstant(NULL, c);
                if(constant == NULL)
                {
                    errors->Add(String::Format("Unable to get constant: {0}", c));
                    continue;
                }

                // get constant desc count		
                UINT descCount = 0;
                hr = constantTable->GetConstantDesc(constant, NULL, &descCount);
                if (!SUCCEEDED(hr) || descCount == 0)
                {
                    errors->Add(String::Format("Unable to get description count for constant: {0}", c));
                    continue;
                }

                // get constant desc
                D3DXCONSTANT_DESC *constantDesc = new D3DXCONSTANT_DESC[descCount];		
                hr = constantTable->GetConstantDesc(constant, &constantDesc[0], &descCount);
                if (!SUCCEEDED(hr))
                {
                    delete [] constantDesc;
                    errors->Add(String::Format("Unable to get description for constant: {0}", c));
                    continue;
                }

                writer->Write(descCount);

                for(unsigned int d = 0; d < descCount; d++)
                {
                    writer->Write(gcnew String(constantDesc[d].Name));
                    writer->Write((unsigned int)constantDesc[d].RegisterSet);                    
                    writer->Write(constantTable->GetSamplerIndex(constant));

                    writer->Write(constantDesc[d].RegisterIndex);
                    writer->Write(constantDesc[d].RegisterCount);
                    
                    writer->Write((unsigned int)constantDesc[d].Class);
                    writer->Write((unsigned int)constantDesc[d].Type);
                }
            }
            writer->Flush();
            writer->Close();

            return output->ToArray();
        }
    public:
        Compiler()
        {
            _calculatedDxLibraryToUse = false;
        }

        CompilerResult^ Process(String^ shaderSourceCode, List<String^>^ errors, String^ shaderProfile, String^ entryPoint)
        {
            CompilerResult^ result;
            marshal_context^ context = gcnew marshal_context();
        
            LPCSTR lpShaderSourceCode = context->marshal_as<LPCSTR>(shaderSourceCode);
            LPD3DXBUFFER compiledShader;
            LPD3DXBUFFER errorMessages;
            LPD3DXCONSTANTTABLE constantTable;

            ShaderCompilerType shaderCompiler = ::D3DXCompileShader;

            // Try to get the latest if the DX SDK is installed.  Otherwise, back up to the statically linked version.
            String^ libraryToLoad = GetDxLibraryToUse();
            CString libraryToLoadAsCString(libraryToLoad);

            HMODULE dxLibrary = ::LoadLibrary((LPCWSTR)libraryToLoadAsCString); 
            bool gotDynamicOne = false;
            if (dxLibrary != NULL)
            {
                FARPROC sc = ::GetProcAddress(dxLibrary, "D3DXCompileShader");
                shaderCompiler = (ShaderCompilerType)sc;
                gotDynamicOne = true;
            }

            LPCSTR lpEntryPoint = context->marshal_as<LPCSTR>(entryPoint);
            LPCSTR lpShaderProfile = context->marshal_as<LPCSTR>(shaderProfile);

            // initialize flags
            DWORD compilerFlags = D3DXSHADER_PACKMATRIX_COLUMNMAJOR;

            D3DXMACRO defines[] = 
            {
                { "SIVERLIGHT", "" },
                { "Silverlight", "" },
                { NULL, NULL },
            };

            HRESULT compileResult = shaderCompiler(
                    lpShaderSourceCode,
                    shaderSourceCode->Length,
                    defines, // pDefines
                    NULL, // pIncludes
                    lpEntryPoint, // entrypoint
                    lpShaderProfile, // "ps_2_0", "vs_2_0", etc.
                    compilerFlags, // compiler flags
                    &compiledShader,
                    &errorMessages,
                    &constantTable   // constant table output
                    );

            if (!SUCCEEDED(compileResult))
            {
                errors->Add(String::Format("Compile error {0}", compileResult));
                
                char *nativeErrorString = NULL;
                if(errorMessages != NULL)
                    nativeErrorString = (char *)(errorMessages->GetBufferPointer());

                String^ managedErrorString = context->marshal_as<String^>(nativeErrorString == NULL ? "Unknown compile error (check flags against DX version)" : nativeErrorString);

                // Need to build up our own error information, since error string from the compiler
                // doesn't identify the source file.

                // Pull the error string from the shader compiler apart.
                // Note that the backslashes are escaped, since C++ needs an escaping of them.  
                String^ subcategory = "Shader";
                String^ dir;
                String^ line;
                String^ col;
                String^ descrip;
                String^ errorCode = "";
                String^ helpKeyword = "";
                int     lineNum = 0;
                int     colNum = 0;
                bool    parsedLineNum = false;

                if (gotDynamicOne)
                {
                    String^ regexString = "(?<directory>[^@]+)memory\\((?<line>[^@]+),(?<col>[^@]+)\\): (?<descrip>[^@]+)";
                    Regex^ errorRegex = gcnew Regex(regexString);
                    Match^ m = errorRegex->Match(managedErrorString);

                    dir     = m->Groups["directory"]->Value;
                    line    = m->Groups["line"]->Value;
                    col     = m->Groups["col"]->Value;
                    descrip = m->Groups["descrip"]->Value;

                    parsedLineNum = Int32::TryParse(line, lineNum);
                    Int32::TryParse(col, colNum);
                }
                else
                {
                    // Statically linked d3dx9.lib's error string is a different format, need to parse that.

                    // Example string: (16): error X3018: invalid subscript 'U'
                    String^ regexString = "\\((?<line>[^@]+)\\): (?<descrip>[^@]+)";
                    Regex^ errorRegex = gcnew Regex(regexString);
                    Match^ m = errorRegex->Match(managedErrorString);

                    line    = m->Groups["line"]->Value;
                    descrip = m->Groups["descrip"]->Value;

                    parsedLineNum = Int32::TryParse(line, lineNum);

                    int colNum = 0;  // no column information supplied
                }

                if (!parsedLineNum)
                {
                    // Just use the whole string as the description.
                    descrip = managedErrorString;
                }
                errors->Add(String::Format("({0}, {1}): error {2} : {3}", lineNum, colNum, errorCode, descrip));

                result = nullptr;
            }
            else
            {
                char *nativeBytestream = (char *)(compiledShader->GetBufferPointer());
                result = gcnew CompilerResult();
                result->ShaderCode = gcnew array<unsigned char>(compiledShader->GetBufferSize());

                // TODO: Really ugly way to copy from a uchar* to a managed array, but I can't easily figure out the
                // "right" way to do it.
                for (unsigned int i = 0; i < compiledShader->GetBufferSize(); i++)
                {
                    result->ShaderCode[i] = nativeBytestream[i];
                }
                
                // Constants
                result->ConstantsDefinition = GetConstants(constantTable, errors);
            }

            if (dxLibrary != NULL)
            {
                ::FreeLibrary(dxLibrary);
            }
            return result;			
        }
    };
}
END_NAMESPACE

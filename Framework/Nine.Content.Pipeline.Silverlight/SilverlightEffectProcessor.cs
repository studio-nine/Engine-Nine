// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SilverlightShaderCompiler;

namespace Nine.Content.Pipeline.Silverlight
{
    [ContentProcessor(DisplayName = "Effect - Silverlight")]
    public class SilverlightEffectProcessor : ContentProcessor<EffectSourceCode, EffectBinary>
    {
        public string Defines { get; set; }
        public EffectProcessorDebugMode DebugMode { get; set; }

        public override EffectBinary Process(EffectSourceCode input, ContentProcessorContext context)
        {
            // Remove comments
            Regex commentRegex = new Regex("//.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string effectCode = commentRegex.Replace(input.EffectCode, "");

            // Remove carriage returns and line feeds
            commentRegex = new Regex(@"(\r\n)|\n", RegexOptions.IgnoreCase);
            effectCode = commentRegex.Replace(effectCode, "");

            // Check effect validity
            // EffectProcessor should check almost all potential errors in the effect code
            EffectContent content = new EffectContent { EffectCode = effectCode };
            EffectProcessor compiler = new EffectProcessor { DebugMode = DebugMode, Defines = Defines };
            compiler.Process(content, context);

            // If we are here, the effect is assumed to be ok!

            // Extract techniques
            IEnumerable<EffectTechnique> techniques = ExtractTechniques(effectCode);

            // Now we have to find entry points for each pass and compile them
            Compiler helper = new Compiler();
            EffectBinary result = new EffectBinary();

            foreach (var technique in techniques)
            {
                EffectTechniqueBinary effectTechniqueBinary = new EffectTechniqueBinary { Name = technique.Name };

                foreach (var pass in technique.Passes)
                {
                    List<string> errors = new List<string>();

                    // Generating a pass binary
                    EffectPassBinary passBinary = new EffectPassBinary
                    {
                        Name = pass.Name,
                        RenderStates = pass.RenderStates,
                    };
                 
                    // Compiling vertex shader
                    if (pass.VertexShaderEntryPoint != null)
                    {
                        CompilerResult vsResult = helper.Process(effectCode, errors, "vs_2_0", pass.VertexShaderEntryPoint);

                        // This should not happen but...
                        if (errors.Count > 0)
                        {
                            ExceptionHelper.RaiseException(String.Join("\n", errors));
                        }

                        passBinary.VertexShaderByteCode = vsResult.ShaderCode;
                        passBinary.VertexShaderParameters = vsResult.ConstantsDefinition;
                    }

                    // Compiling pixel shader
                    if (pass.PixelShaderEntryPoint != null)
                    {
                        CompilerResult psResult = helper.Process(effectCode, errors, "ps_2_0", pass.PixelShaderEntryPoint);

                        // This should not happen but...
                        if (errors.Count > 0)
                        {
                            ExceptionHelper.RaiseException(String.Join("\n", errors));
                        }

                        passBinary.PixelShaderByteCode = psResult.ShaderCode;
                        passBinary.PixelShaderParameters = psResult.ConstantsDefinition;
                    }

                    effectTechniqueBinary.PassBinaries.Add(passBinary);
                }

                result.TechniquesBinaries.Add(effectTechniqueBinary);
            }

            return result;
        }

        /// <summary>
        /// Find all techniques inside an effect
        /// </summary>
        static IEnumerable<EffectTechnique> ExtractTechniques(string sourceCode)
        {
            StringBuilder currentWord = new StringBuilder();
            StringBuilder currentCode = new StringBuilder();
            int depth = 0;
            List<EffectTechnique> result = new List<EffectTechnique>();
            bool techniqueKeywordFound = false;
            string techniqueName = null;

            foreach (char c in sourceCode)
            {
                // Special cases
                if (c == '{')
                {
                    depth++; 
                }
                if (c == '}')
                {
                    depth--;

                    if (depth == 0) // We have found a complete block
                    {
                        // Did we find a technique?
                        if (!string.IsNullOrEmpty(techniqueName))
                        {
                            result.Add(new EffectTechnique {Name = techniqueName, SourceCode = currentCode.ToString()});
                        }

                        techniqueKeywordFound = false;
                        techniqueName = null;
                        currentCode.Clear();
                        currentWord.Clear();
                        continue;
                    }
                }

                // Separators
                if (c == ' ' || c == '\n' || c == '\r' || c == ';') 
                {
                    string word = currentWord.ToString().Trim();
                    currentWord.Clear();

                    if (!string.IsNullOrEmpty(word))
                    {
                        if (string.Equals(word, "technique", StringComparison.OrdinalIgnoreCase))
                        {
                            techniqueKeywordFound = true;
                        }
                        else if (techniqueKeywordFound && string.IsNullOrEmpty(techniqueName))
                        {
                            techniqueName = word;
                            techniqueKeywordFound = false;
                        }
                    }
                }

                // Appending character
                currentWord.Append(c);

                if (depth > 0)
                {
                    currentCode.Append(c);
                }
            }

            return result.ToArray();
        }
    }
}
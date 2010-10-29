using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace CustomToolTemplate
{
    public class MyCustomTool : CustomToolBase
    {
        public MyCustomTool()
        {
            // #error Add the code for your custom tool's code generator
            this.OnGenerateCode += (s, e) =>
                {
                    e.OutputCode.AppendLine("// TODO: add code generation");
                    
                    #region examples of getting information and common operations
                    // Report an error:
                    // e.GenerateError("There was an error creating this file");

                    // Report an error with source file
                    // line and column information:
                    // e.GenerateError("There was an error creating this file", 12, 1);

                    // Get text from the file the tool was
                    // applied to:
                    // string sourceFileText = e.InputText;

                    // set the output file's extension:
                    // e.OutputFileExtension = "xml";

                    // Get the namespace the user typed into 
                    // the Custom Tool Namespace text box:
                    // string userProvidedNamespace = e.Namespace;

                    // Get some of the design-time properties of 
                    // the source project item:
                    // string sourceFileName = e.ProjectItem
                    //                    .Properties
                    //                    .Item("FileName")
                    //                    .Value
                    //                    .ToString();

                    // source file size:
                    // int sourceFileSize = (int)(uint)e.ProjectItem
                    //                   .Properties
                    //                   .Item("Filesize")
                    //                   .Value;                                       

                    // Get all properties' names and values:
                    //for (int p = 1; p < e.ProjectItem.Properties.Count; p++)
                    //{
                    //    System.Diagnostics.Debug.WriteLine(
                    //        string.Format("{0} : {1}",
                    //            e.ProjectItem.Properties.Item(p).Name,
                    //            e.ProjectItem.Properties.Item(p).Value));
                    //}
                    #endregion
                };
        }
    }
}

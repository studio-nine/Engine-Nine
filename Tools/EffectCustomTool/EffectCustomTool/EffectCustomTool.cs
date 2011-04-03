//=============================================================================
//
//  Copyright 2009 - 2010 (c) Yufei Huang.
//
//=============================================================================

#region Using Directives
using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace Nine.Tools.EffectCustomTool
{
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("72C1F067-C0D6-4263-8279-65A8786C628F")]
    [Microsoft.VisualStudio.Shell.ProvideObject(typeof(EffectCustomTool), RegisterUsing = Microsoft.VisualStudio.Shell.RegistrationMethod.CodeBase)]
    public class EffectCustomTool : CustomToolBase
    {
        string name;

        public EffectCustomTool()
        {
            OnGenerateCode += new GenerationHandler(GenerateCode);
        }

        public void GenerateCode(object sender, GenerationEventArgs e)
        {
            e.FailOnError = true;
            name = Path.GetFileNameWithoutExtension(e.InputFilePath);

            EffectCompiler compiler = null;

            try
            {
                compiler = new EffectCompiler(name, e.Namespace, e.InputFilePath);
            }
            catch (Exception ex)
            {
                e.GenerateError(ex.Message);
                return;
            }


            // .Designer.cs
            e.OutputFileExtension = ".Designer.cs";

            string content = compiler.Designer;

            e.OutputCode.Append(content);



            // .cs
            string strFile = Path.Combine(
                e.InputFilePath.Substring(0, 
                e.InputFilePath.LastIndexOf(Path.DirectorySeparatorChar)), name + ".cs");


            if (!File.Exists(strFile))
            {
                string existingContent = "";
                string existingName = "";

                // perform some clean-up, making sure we delete any old (stale) target-files
                foreach (EnvDTE.ProjectItem childItem in e.ProjectItem.ProjectItems)
                {
                    if (!(childItem.Name.ToLower() == name.ToLower() + ".cs" ||
                          childItem.Name.ToLower() == name.ToLower() + "designer.cs") &&
                         !childItem.Name.ToLower().EndsWith(".designer.cs"))
                    {
                        EnvDTE.TextDocument text = (EnvDTE.TextDocument)childItem.Document.Object();

                        text.Selection.SelectAll();

                        existingContent = text.Selection.Text;
                        existingName = Path.GetFileNameWithoutExtension(childItem.Name);

                        childItem.Delete();
                    }
                }

                using (FileStream fs = new FileStream(strFile, FileMode.Create))
                {
                    try
                    {
                        // Create new
                        if (string.IsNullOrEmpty(existingContent))
                        {
                            content = Strings.Default;

                            content = content.Replace(@"{$NAMESPACE}", e.Namespace);
                            content = content.Replace(@"{$CLASS}", Path.GetFileNameWithoutExtension(e.InputFilePath));
                        }

                        // Rename
                        else
                        {
                            content = existingContent.Replace(" class " + existingName, " class " + name);
                            content = content.Replace(" " + existingName + "(", " " + name + "(");
                            content = content.Replace("(" + existingName + " ", "(" + name + " ");
                        }


                        StreamWriter writer = new StreamWriter(fs);

                        writer.Write(content);
                        writer.Flush();
                        writer.Close();

                        e.ProjectItem.ProjectItems.AddFromFile(strFile);
                    }
                    catch (Exception)
                    {
                        fs.Close();
                        if (File.Exists(strFile))
                            File.Delete(strFile);
                    }
                }
            }
        }
    }
}

#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms;
#endregion


namespace Isles.Samples
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class SampleClassAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SampleMethodAttribute : DisplayNameAttribute 
    {
        public SampleMethodAttribute() { }

        public SampleMethodAttribute(string displayName)
            : base(displayName)
        {
            IsStartup = false;
        }

        public SampleMethodAttribute(string displayName, bool isStartup)
            : base(displayName)
        {
            IsStartup = isStartup;
        }
        public bool IsStartup { get; set; }
    }


    public class Sample
    {
        public string Name { get; internal set; }

        public MethodInfo EntryPoint { get; internal set; }


        public void Run()
        {
            if (EntryPoint != null)
            {
                try
                {
                    EntryPoint.Invoke(null, null);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        public override string ToString()
        {
            return Name;
        }


        public static List<Sample> FromType(Type type)
        {
            List<Sample> samples = new List<Sample>();


            foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    if (info.GetParameters().Length == 0)
                    {
                        foreach (Attribute attribute in info.GetCustomAttributes(false))
                        {
                            if (attribute is SampleMethodAttribute)
                            {
                                Sample sample = new Sample();

                                sample.EntryPoint = info;
                                sample.Name = (attribute as SampleMethodAttribute).DisplayName;

                                if (string.IsNullOrEmpty(sample.Name))
                                    sample.Name = info.ReflectedType.Name + "." + info.Name;

                                samples.Add(sample);

                                if ((attribute as SampleMethodAttribute).IsStartup)
                                {
                                    info.Invoke(null, null);
                                }
                            }
                        }
                    }
                }
                catch /*(Exception ex)*/ { }
            }

            return samples;
        }


        public static List<Sample> FromAssembly(Assembly assembly)
        {
            List<Sample> samples = new List<Sample>();


            foreach (Type type in assembly.GetTypes())
            {
                try
                {
                    foreach (Attribute attribute in type.GetCustomAttributes(false))
                    {
                        if (attribute is SampleClassAttribute)
                        {
                            samples.AddRange(FromType(type));
                            break;
                        }
                    }
                }
                catch /*(Exception ex)*/ { }
            }

            return samples;
        }


        public static List<Sample> FromPath(string path)
        {
            List<Sample> samples = new List<Sample>();
            List<string> sampleFiles = new List<string>();
            

            sampleFiles.AddRange(Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly));
            sampleFiles.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly));


            foreach (string file in sampleFiles)
            {
                try
                {
                    // Ignore *.vshost file
                    if (!file.EndsWith(".vshost.exe"))
                        samples.AddRange(FromAssembly(Assembly.LoadFile(file)));
                }
                catch /*(Exception ex)*/ { }
            }

            return samples;
        }
    }
}

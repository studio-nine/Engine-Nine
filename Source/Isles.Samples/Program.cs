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
using System.Threading;
#endregion


namespace Isles.Samples
{
    internal static class Program
    {
        static SamplesForm form = null;
        static Thread sampleThread = null;

            
        public static void Main(string[] args)
        {
            // Load samples from current folder
            List<Sample> testCases = Sample.FromPath(Directory.GetCurrentDirectory());


            // Sort samples by name
            testCases.Sort(delegate(Sample x, Sample y) { return string.Compare(x.Name, y.Name); });


            // Creates a form to show the test cases
            using (form = new SamplesForm())
            {
                form.Samples.Items.AddRange(testCases.ToArray());
                form.Samples.DoubleClick += OnDoubleClick;

                Application.EnableVisualStyles();
                Application.Run(form);
            }
        }

        private static void OnDoubleClick(object sender, EventArgs arg)
        {
            Sample sample = form.Samples.SelectedItem as Sample;

            if (sample != null && form.Samples.SelectedIndex >= 0)
            {
                if (sampleThread != null)
                    sampleThread.Abort();

                sampleThread = new Thread(Start);

                sampleThread.Start(sample);
            }
        }

        private static void Start(object start)
        {
#if DEBUG
            (start as Sample).Run();
#else
            try
            {
                (start as Sample).Run();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.Flush();

                MessageBox.Show(ex.Message, "Error");
            }
#endif
        }
    }
}

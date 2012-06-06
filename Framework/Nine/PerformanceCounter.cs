#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Diagnostics;
using System.Collections.Generic;

#endregion

namespace Nine
{
    class PerformanceCounter : IDisposable
    {
        Stopwatch stopwatch = new Stopwatch();

        public string Name { get; private set; }

        public TimeSpan ElapsedTime
        {
            get { return stopwatch.Elapsed; }
        }
        

        private static List<PerformanceCounter> counters = new List<PerformanceCounter>();

        public static readonly PerformanceCounter ViewFrustumCulling = new PerformanceCounter("ViewFrustumCulling");
        public static readonly PerformanceCounter SceneUpdate = new PerformanceCounter("SceneUpdate");
        public static readonly PerformanceCounter PrepareLights = new PerformanceCounter("PrepareLights");
        public static readonly PerformanceCounter PrepareShadows = new PerformanceCounter("PrepareShadows");
        public static readonly PerformanceCounter Drawing = new PerformanceCounter("Drawing");

        public PerformanceCounter(string name)
        {
            Name = name;
            counters.Add(this);
        }
                
        public IDisposable Start()
        {
#if DEBUG
            stopwatch.Start();
#endif
            return this;
        }

        public void Dispose()
        {
#if DEBUG
            stopwatch.Stop();
#endif     
        }

        public static void Trace()
        {
            System.Diagnostics.Trace.WriteLine("Performance Summary:");
            foreach (var counter in counters)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("{0}\t{1}", counter.Name, counter.ElapsedTime));
                counter.stopwatch.Reset();
            }
        }
    }
}
#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Nine.Tools.ScreenshotCapturer;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Tools.SampleBrowser
{
    enum TargetPlatform
    {
        Windows,
        Xbox,
        WindowsPhone,
    }

    class SampleInfo
    {
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public string[] Category { get; set; }
        public TargetPlatform TargetPlatform { get; set; }
    }
}
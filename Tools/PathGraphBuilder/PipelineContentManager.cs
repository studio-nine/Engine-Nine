#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics;
#endregion

namespace Nine.Tools.PathGraphBuilder
{
    class PipelineContentManager : ContentManager
    {
        string BaseDirectory;

        public PipelineContentManager(string baseDirectory, IServiceProvider services)
            : base(services)
        {
            BaseDirectory = baseDirectory;
        }

        protected override Stream OpenStream(string assetName)
        {
            // In case Xna content pipeline does not support absolute path.
            if (Path.IsPathRooted(assetName))
            {
                if (File.Exists(assetName))
                    return new FileStream(assetName, FileMode.Open);
            }
            else
            {
                var assetFile = Path.Combine(BaseDirectory, assetName) + ".xnb";
                if (File.Exists(assetFile))
                    return new FileStream(assetFile, FileMode.Open);
            }
            return base.OpenStream(assetName);
        }
    }
}

#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Nine.Graphics;
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Nine.Studio.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Studio.Content
{
    public class PipelineBuilder<T>
    {
        public void Build(Stream output, T content)
        {
            var constructor = typeof(ContentCompiler).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            ContentCompiler compiler = (ContentCompiler)constructor.Invoke(null);
            var method = compiler.GetType().GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(compiler, new object[] 
            {
                output, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, ".", ".",
            });
        }

        static int NextAssetId = 0;
        public TRunTime BuildAndLoad<TRunTime>(GraphicsDevice graphics, T content)
        {
            var contentManager = GraphicsResources<BuilderContentManager>.GetInstance(graphics);
            contentManager.SpecifiedStream = new MemoryStream();
            Build(contentManager.SpecifiedStream, content);
            contentManager.SpecifiedStream.Seek(0, SeekOrigin.Begin);
            return contentManager.Load<TRunTime>(NextAssetId++.ToString());
        }

        class BuilderContentManager : ContentManager
        {
            public Stream SpecifiedStream;

            public BuilderContentManager(GraphicsDevice graphics)
                : base(new GraphicsDeviceService() { GraphicsDevice = graphics }) { }

            protected override Stream OpenStream(string assetName)
            {
                return SpecifiedStream;
            }
        }

        class GraphicsDeviceService : IGraphicsDeviceService, IServiceProvider
        {
            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;

            public GraphicsDevice GraphicsDevice { get; set; }

            public object GetService(Type serviceType)
            {
                return serviceType == typeof(IGraphicsDeviceService) ? this : null;
            }
        }
    }
}

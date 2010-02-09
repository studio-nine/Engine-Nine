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
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles
{
    internal static class InternalContents
    {
        #region Resource Content Manager
        /// <summary>
        /// Resource Content manager for Isles.Graphics.Contents
        /// </summary>
        private static ResourceContentManager Content;

        public static ResourceContentManager GetContentManager(GraphicsDevice graphics)
        {
            if (Content == null || 

               // In case of we switch to a different graphics device.
               (Content.ServiceProvider.GetService(
                    typeof(IGraphicsDeviceService)) as IGraphicsDeviceService).GraphicsDevice != graphics)
            {
                Content = new ResourceContentManager(
                    new GraphicsDeviceService(graphics), Contents.ResourceManager);
            }

            return Content;
        }


        internal class GraphicsDeviceService : IGraphicsDeviceService, IServiceProvider
        {
            public GraphicsDevice GraphicsDevice { get; set; }

            public GraphicsDeviceService(GraphicsDevice graphics)
            {
                GraphicsDevice = graphics;
            }

            public object GetService(Type serviceType)
            {
                return (serviceType == typeof(IGraphicsDeviceService)) ? this : null;
            }

            #region IGraphicsDeviceService Members
                        
            public event EventHandler DeviceCreated;

            public event EventHandler DeviceDisposing;

            public event EventHandler DeviceReset;

            public event EventHandler DeviceResetting;

            #endregion
        }
        #endregion

        #region Effects
        public static Effect DebugShader(GraphicsDevice graphics)
        {
            CompiledEffect compiled = Effect.CompileEffectFromFile(
                new System.IO.MemoryStream(Contents.DebugShader), null, null, CompilerOptions.None, TargetPlatform.Windows);

            return new Effect(graphics, compiled.GetEffectCode(), CompilerOptions.None, null);
        }

        public static Effect DebugShader1(GraphicsDevice graphics)
        {
            CompiledEffect compiled = Effect.CompileEffectFromFile(
                new System.IO.MemoryStream(Contents.DebugShader1), null, null, CompilerOptions.None, TargetPlatform.Windows);

            return new Effect(graphics, compiled.GetEffectCode(), CompilerOptions.None, null);
        }

        public static Effect BasicTextureEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("BasicTexture");
        }

        public static Effect SplatTextureEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("SplatTexture");
        }

        public static Effect FloatTextureEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("FloatTexture");
        }

        public static Effect GaussianBlurEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("GaussianBlur");
        }

        public static Effect BloomExtractEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("BloomExtract");
        }

        public static Effect BloomCombineEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("BloomCombine");
        }

        public static Effect BasicSkinnedEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("BasicSkinned");
        }

        public static Effect ShadowCasterEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("ShadowCaster");
        }

        public static Effect ShadowReceiverEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("ShadowReceiver");
        }

        public static Effect BasicParticleEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("ParticleEffect");
        }

        public static Effect SaturationEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("Saturation");
        }

        public static Effect ColorMatrixEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("ColorMatrix");
        }

        public static Effect SkyBoxEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("SkyBox");
        }

        public static Effect HeightmapEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("Heightmap");
        }

        public static Effect WaterEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("Water");
        }

        public static Effect TerrainLayerEffect(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Effect>("TerrainLayer");
        }
        #endregion

        #region Textures
        public static Texture2D LightCircleTexture(GraphicsDevice graphics)
        {
            return GetContentManager(graphics).Load<Texture2D>("LightCircle");
        }
        #endregion
    }
}

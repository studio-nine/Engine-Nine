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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Models;
using Isles.Graphics.Landscape;
using Isles.Graphics.Effects;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class TerrainGeometrySample : BasicModelViewerGame
    {
        Terrain terrain;
        TerrainGeometry geometry;
        GeometryVisualizer visualizer;

        protected override void LoadContent()
        {
            geometry = Content.Load<TerrainGeometry>("Terrain/RF1");
            visualizer = new GeometryVisualizer(GraphicsDevice, geometry);



            TerrainSplatEffect splat = new TerrainSplatEffect();

            splat.TessellationU = splat.TessellationV = 32;
            splat.SplatTexture = Content.Load<Texture2D>("Terrain/splat");
            splat.Texture = Content.Load<Texture2D>("Terrain/Tile");
            splat.TextureR = Content.Load<Texture2D>("Terrain/Layer1");
            splat.TextureG = Content.Load<Texture2D>("Terrain/Layer2");


            terrain = new Terrain(GraphicsDevice, geometry);
            //terrain.Layers.Add(splat);

            TerrainLayerEffect layer = new TerrainLayerEffect();
            layer.TessellationU = layer.TessellationV = 32;
            layer.Texture = Content.Load<Texture2D>("Terrain/Layer1");
            layer.AlphaTexture = Content.Load<Texture2D>("Terrain/2");
            terrain.Layers.Add(layer);

            terrain.Layers.Add(new FloatTextureEffect(Content.Load<Texture2D>("Terrain/clouds")));

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            terrain.Draw(gameTime, Camera.View, Camera.Projection);
        }

        [SampleMethod(IsStartup = false)]
        public static void Test()
        {
            using (TerrainGeometrySample game = new TerrainGeometrySample())
                game.Run();
        }
    }
}

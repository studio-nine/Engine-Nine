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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Models;
using Isles.Graphics.Landscape;
using Isles.Graphics.Effects;
using Isles.Graphics.Illumination;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class LandscapeSample : BasicModelViewerGame
    {
        public Terrain Terrain { get; set; }
        TerrainGeometry terrainGeometry;

        Dome skyDome;
        public FloatTextureEffect SkySurface { get; set; }

        Isles.Graphics.Primitives.Plane water;
        WaterSurfaceEffect waterSurface;

        Matrix view = new Matrix(
            0.148388192f,
            0.04257007f,
            -0.988012552f,
            0.0f,
            -0.988138258f,
            0.04633101f,
            -0.1464107f,
            0.0f,
            0.0395428464f,
            0.998018742f,
            0.0489400253f,
            0.0f,
            0.0f,
            0.0f,
            -149.400024f,
            1.0f);

        public float SkyHeightScale { get; set; }
        public float SkySizeScale { get; set; }        

        Matrix primitiveTransform = Matrix.CreateRotationX(MathHelper.PiOver2);

        RenderToTextureEffect rtt;
        SpriteBatch sprite;


        protected override void LoadContent()
        {
            ShowAxis = false;
            ShowGrid = false;

            SkyHeightScale = 1;
            SkySizeScale = 1;

            terrainGeometry = Content.Load<TerrainGeometry>("Terrain/RF1");

            TerrainSplatEffect splat = new TerrainSplatEffect();

            splat.Texture = Content.Load<Texture2D>("Terrain/Layer1");
            splat.TessellationU = splat.TessellationV = 20;


            Terrain = new Terrain(GraphicsDevice, terrainGeometry);            
            Terrain.Geometry.Position = Vector3.UnitZ * -5.0f;
            Terrain.Layers.Add(splat);
            Terrain.Layers.Add(new FloatTextureEffect(Content.Load<Texture2D>("Terrain/clouds")));


            skyDome = new Dome(GraphicsDevice, 400, 50, 16);
            water = new Isles.Graphics.Primitives.Plane(GraphicsDevice, 400, 400, 128, 128);

            SkySurface = new FloatTextureEffect(Content.Load<Texture2D>("Terrain/Sky"));
            SkySurface.TessellationU = SkySurface.TessellationV = 2;



            waterSurface = new WaterSurfaceEffect();

            rtt = new RenderToTextureEffect(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            sprite = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;


            // Camera recalculate projection based on viewport.
            // rtt.Begin() will change viewport, so save projection matrix here.
            Matrix projection = Camera.Projection;
            //view = Camera.View;
            Matrix skyTransform = Matrix.CreateScale(SkySizeScale, SkySizeScale, SkyHeightScale);


            // Water reflection
            rtt.Begin();
            {
                GraphicsDevice.Clear(Color.White);

                Matrix v = Matrix.CreateReflection(new Microsoft.Xna.Framework.Plane(Vector3.UnitZ, 0)) * view;

                waterSurface.ReflectionViewProjection = v * projection;

                Terrain.Draw(gameTime, v, projection);

                GraphicsDevice.RenderState.AlphaBlendEnable = false;
                GraphicsDevice.RenderState.CullMode = CullMode.None;
                skyDome.Draw(SkySurface, gameTime, primitiveTransform * skyTransform, v, projection);
                GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                GraphicsDevice.RenderState.AlphaBlendEnable = true;
            }
            waterSurface.ReflectionTexture = rtt.End();



            //Terrain.Draw(gameTime, view, Camera.Projection);


            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            //skyDome.Draw(SkySurface, gameTime, primitiveTransform * skyTransform, view, Camera.Projection);
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;


            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            water.Draw(waterSurface, gameTime, primitiveTransform, Camera.View, Camera.Projection);
            GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            GraphicsDevice.RenderState.AlphaBlendEnable = true;

            // Reflection texture
            if (Input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R))
            {
                sprite.Begin(SpriteBlendMode.None);
                sprite.Draw(waterSurface.ReflectionTexture, Vector2.Zero, Color.White);
                sprite.End();
            }
        }

        [SampleMethod]
        public static void Test()
        {
            using (LandscapeSample game = new LandscapeSample())
                game.Run();
        }
    }
}

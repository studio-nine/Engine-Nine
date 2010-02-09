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
using Isles.Graphics.Effects;
using Isles.Graphics.Landscape;
using Isles.Graphics.Filters;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class WaterSample : Microsoft.Xna.Framework.Game
    {
        Axis grid;
        Cube skybox;
        SkyBoxEffect skyboxEffect;
        FreeCamera camera;
        ModelBatch batch;
        Model model;
        Matrix modelTransform;

        Isles.Graphics.Primitives.Plane water;
        RenderToTextureEffect rtt;
        WaterSurfaceEffect waterSurface;


        public WaterSample()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            Components.Add(new Isles.Components.ScreenshotCapturer(this));
        }

        protected override void LoadContent()
        {
            grid = new Axis(GraphicsDevice);

            camera = new FreeCamera();

            camera.Position = new Vector3(0, -5, 5);
            camera.Speed = 0.01f;
            camera.TurnSpeed = 10.0f;            

            skybox = new Cube(GraphicsDevice);
            
            skyboxEffect = new SkyBoxEffect(Content.Load<TextureCube>("Terrain/skybox"));
            skyboxEffect.FarClip = 100;

            model = Content.Load<Model>("Models/ship");
            batch = new ModelBatch();

            modelTransform = Matrix.CreateScale(0.001f) * 
                             Matrix.CreateRotationX(MathHelper.PiOver2) * 
                             Matrix.CreateTranslation(0, 0, 2);

            water = new Isles.Graphics.Primitives.Plane(GraphicsDevice, 60, 60, 128, 128);
            waterSurface = new WaterSurfaceEffect();
            waterSurface.AmbientLightColor = Vector3.One * 0.6f;
            waterSurface.WaveTessellationU = waterSurface.WaveTessellationV = 16;
            waterSurface.SpecularColor = Vector3.One * 0.6f;
            waterSurface.DiffuseColor = Vector3.One * 0.7f;
            waterSurface.LightSource.Position = new Vector3(-100, 100, 100);
            waterSurface.Speed = 0.1f;

            rtt = new RenderToTextureEffect(GraphicsDevice, 512, 512);


            waterSurface.WaveTexture = Content.Load<Texture2D>("Textures/WAVES2");

            PropertyBrowser.ShowForm("Editor").SelectedObject = waterSurface;

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            camera.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            
            DrawReflection(gameTime);

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RenderState.CullMode = CullMode.None;

            //grid.Draw(Matrix.Identity, camera.View, camera.Projection);
            skybox.Draw(skyboxEffect, gameTime, Matrix.Identity, camera.View, camera.Projection);

            batch.Draw(model, modelTransform, camera.View, camera.Projection);

            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q))
                GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            water.Draw(waterSurface, gameTime, Matrix.CreateRotationX(MathHelper.PiOver2), camera.View, camera.Projection);

            GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            base.Draw(gameTime);
        }

        private void DrawReflection(GameTime gameTime)
        {
            rtt.Begin();


            GraphicsDevice.Clear(Color.White);

            Matrix v = Matrix.CreateReflection(new Microsoft.Xna.Framework.Plane(Vector3.UnitZ, 0)) * camera.View;


            skybox.Draw(skyboxEffect, gameTime, Matrix.Identity, v, camera.Projection);
            batch.Draw(model, modelTransform, v, camera.Projection);

            
            waterSurface.ReflectionViewProjection = v * camera.Projection;
            waterSurface.ReflectionTexture = rtt.End();
        }


        [SampleMethod(Startup=false)]
        public static void Test()
        {
            using (WaterSample game = new WaterSample())
                game.Run();
        }
    }
}

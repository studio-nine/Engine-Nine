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
using Isles.Navigation;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class ShadowMappingSample : BasicModelViewerGame
    {
        Terrain terrain;
        Model tank;
        ModelBatch batch;

        ShadowMapEffect shadow;
        ShadowMapCasterEffect caster;
        ShadowMapReceiverEffect receiver;

        SpriteBatch sprite;

        protected override void LoadContent()
        {
            BasicTextureEffect basic = new BasicTextureEffect();

            basic.Texture = Content.Load<Texture2D>("Terrain/Tile");
            basic.TessellationU = basic.TessellationV = 16;


            tank = Content.Load<Model>("Models/Ship");
            tank.Bones[0].Transform = Matrix.CreateScale(0.002f) * 
                                      Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                      Matrix.CreateRotationY(MathHelper.PiOver2);

            sprite = new SpriteBatch(GraphicsDevice);

            Matrix lightViewProjection =
                Matrix.CreateLookAt(new Vector3(0, 0, 30), Vector3.Zero, Vector3.UnitX) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.1f, 100.0f);

            batch = new ModelBatch();
            shadow = new ShadowMapEffect(GraphicsDevice);
            caster = new ShadowMapCasterEffect();
            receiver = new ShadowMapReceiverEffect();

            caster.LightViewProjection = receiver.LightViewProjection = lightViewProjection;
            
            terrain = new Terrain(GraphicsDevice, Content.Load<TerrainGeometry>("Terrain/RF1"));
            terrain.Layers.Add(basic);
            terrain.Layers.Add(receiver);


            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Matrix tankTransform = Matrix.CreateTranslation(0, 0, 15);


            GraphicsDevice.RenderState.AlphaBlendEnable = false;

            // Shadowmap generation
            if (shadow.Begin())
            {
                batch.Begin();
                batch.Draw(tank, caster, gameTime, tankTransform, Matrix.Identity, Matrix.Identity);
                batch.End();

                receiver.Texture = shadow.End();
            }

            base.Draw(gameTime);


            GraphicsDevice.RenderState.AlphaBlendEnable = false;

                  
            terrain.Draw(gameTime, Camera.View, Camera.Projection);


            batch.Begin();
            batch.Draw(tank, tankTransform, Camera.View, Camera.Projection);
            //batch.Draw(tank, receiver, null, tankTransform, Camera.View, Camera.Projection);
            batch.End();

            if (Input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                sprite.Begin();
                sprite.Draw(shadow.ShadowMap, Vector2.Zero, Color.White);
                sprite.End();
            }
        }

        [SampleMethod]
        public static void Test()
        {
            using (ShadowMappingSample game = new ShadowMappingSample())
                game.Run();
        }
    }
}

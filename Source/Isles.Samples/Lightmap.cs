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
using Isles.Graphics.Illumination;
using Isles.Graphics.Effects;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class LightmapSample : Microsoft.Xna.Framework.Game
    {
        LightmapEffect lightmap;
        LightmapShrinker shinker;
        SpriteBatch sprite;
        Transitions.SinTransition<Vector3> position;

        public LightmapSample()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            sprite = new SpriteBatch(GraphicsDevice);

            lightmap = new LightmapEffect(GraphicsDevice, null, 900, 600);
            shinker = new LightmapShrinker(GraphicsDevice);

            position = new Isles.Transitions.SinTransition<Vector3>();
            position.Start = new Vector3(100, 400, 0);
            position.End = new Vector3(400, 400, 0);
            position.Effect = Isles.Transitions.TransitionEffect.Yoyo;

            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RenderState.AlphaBlendEnable = true;

            PointLight pt = new PointLight();

            pt.Radius = 20.0f;
            pt.Position = position.Update(gameTime);

            lightmap.LightSource.Clear();
            lightmap.LightSource.Add(pt);


            sprite.Begin();
            sprite.Draw(shinker.Shink(lightmap.GetTexture()), Vector2.Zero, Color.White);
            //sprite.Draw(lightmap.GetTexture(), Vector2.Zero, Color.White);
            sprite.End();

            base.Draw(gameTime);
        }

        [SampleMethod]
        public static void Test()
        {
            using (LightmapSample game = new LightmapSample())
                game.Run();
        }
    }
}

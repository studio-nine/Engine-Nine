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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Isles.Components;
using Isles.Graphics;
using Isles.Graphics.Cameras;
using Isles.Graphics.Primitives;
using Isles.Graphics.Models;
using Isles.Graphics.Landscape;
using Isles.Graphics.Effects;
using Isles.Movements;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class MovementSample : SampleGame
    {
        TopDownEditorCamera camera;
        Terrain terrain;
        Model tank;
        ModelBatch batch;
        
        CharactorMovement movement;

        private void XmlLoaderTest()
        {
            Services.AddService(typeof(ContentManager), Content);
            Services.AddService(typeof(ModelBatch), new ModelBatch());
            Services.AddService(typeof(INamedObjectManager), new NamedObjectManager());

            XmlLoader loader = new XmlLoader();

            object o = loader.Load<object>(XmlReader.Create("Content/Misc/Loader.ix"), Services);
        }

        protected override void LoadContent()
        {

            XmlLoaderTest();

            IsMouseVisible = true;

            camera = new TopDownEditorCamera(this);
            //camera = new FreeCamera(new Vector3(-40, -40, 60), 0.1f, 10f);

            BasicTextureEffect basic = new BasicTextureEffect();

            basic.Texture = Content.Load<Texture2D>("Terrain/Tile");
            basic.TessellationU = basic.TessellationV = 8;

            terrain = new Terrain(GraphicsDevice, Content.Load<TerrainGeometry>("Terrain/RF1"));
            terrain.Layers.Add(basic);


            tank = Content.Load<Model>("Models/Ship");
            tank.Bones[0].Transform = Matrix.CreateScale(0.002f) * 
                                      Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                      Matrix.CreateRotationY(MathHelper.PiOver2); ;

            batch = new ModelBatch();


            movement = new CharactorMovement();
            movement.Surface = terrain.Geometry;
            movement.Target = new Vector3(10, 0, 0);
        }


        protected override void Update(GameTime gameTime)
        {
            //camera.Update(gameTime);
            movement.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RenderState.AlphaBlendEnable = false;

            terrain.Draw(gameTime, camera.View, camera.Projection);

            batch.Begin();
            batch.Draw(tank, movement.Transform, camera.View, camera.Projection);
            batch.End();
        }

        [SampleMethod(Startup=true)]
        public static void Test()
        {
            using (MovementSample game = new MovementSample())
                game.Run();
        }
    }
}

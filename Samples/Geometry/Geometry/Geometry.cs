#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
#endregion

namespace GeometryGame
{
    [Category("Scene Management")]
    [DisplayName("Geometry")]
    [Description("")]
    public class GeometryTestGame : Microsoft.Xna.Framework.Game
    {
#if WINDOWS_PHONE
        private const int TargetFrameRate = 30;
        private const int BackBufferWidth = 800;
        private const int BackBufferHeight = 480;
#elif XBOX
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
#else
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 900;
        private const int BackBufferHeight = 600;
#endif

        const float PickBoxSize = 5f;

        Scene scene;
        PrimitiveBatch primitiveBatch;
        BoundingBox pickBox = new BoundingBox(Vector3.Zero, Vector3.One * PickBoxSize);
        List<IGeometry> geometries = new List<IGeometry>();
        List<Vector3> triangles = new List<Vector3>();
        List<IBoundable> boundables = new List<IBoundable>();

        public GeometryTestGame()
        {
#if !SILVERLIGHT
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;
#endif
            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new InputComponent(Window.Handle));
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));

            primitiveBatch = new PrimitiveBatch(GraphicsDevice, 65535);
            
            scene = new Scene(GraphicsDevice);
            //scene.Add(new DrawableModel(Content.Load<Model>("dude")) { });
            scene.Add(Content.Load<Geometry>("Tank"));
            scene.Add(new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("RF1")) { Position = new Vector3(-64, -64, 0) });            
            scene.Add(new AmbientLight(GraphicsDevice) { AmbientLightColor = Vector3.One });
            
            scene.Camera = new TopDownEditorCamera(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(47, 79, 79, 255));

            float offset = (float)gameTime.ElapsedGameTime.TotalSeconds * 30;

            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.LeftShift))
                offset *= 0.5f;

            if (keyboard.IsKeyDown(Keys.W))
            {
                pickBox.Min += Vector3.UnitY * offset;
                pickBox.Max += Vector3.UnitY * offset;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                pickBox.Min -= Vector3.UnitY * offset;
                pickBox.Max -= Vector3.UnitY * offset;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                pickBox.Min += Vector3.UnitX * offset;
                pickBox.Max += Vector3.UnitX * offset;
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                pickBox.Min -= Vector3.UnitX * offset;
                pickBox.Max -= Vector3.UnitX * offset;
            }
            if (keyboard.IsKeyDown(Keys.Z))
            {
                pickBox.Min += Vector3.UnitZ * offset;
                pickBox.Max += Vector3.UnitZ * offset;
            }
            if (keyboard.IsKeyDown(Keys.X))
            {
                pickBox.Min -= Vector3.UnitZ * offset;
                pickBox.Max -= Vector3.UnitZ * offset;
            }

            MouseState state = Mouse.GetState();
            Ray ray = GraphicsDevice.Viewport.CreatePickRay(state.X, state.Y, scene.Camera.View, scene.Camera.Projection);

            geometries.Clear();
            scene.FindAll(geometries);

            boundables.Clear();
            scene.FindAll(ref pickBox, boundables);
            scene.FindAll(ref ray, boundables);

            triangles.Clear();
            //foreach (IGeometry geometry in boundables.OfType<IGeometry>().Concat(geometries.OfType<Geometry>()))
            foreach (IGeometry geometry in geometries.OfType<Geometry>())
            {
                for (int i = 0; i < geometry.Indices.Length; i += 3)
                {
                    var triangle = new Triangle();
                    triangle.V1 = geometry.Positions[geometry.Indices[i]];
                    triangle.V2 = geometry.Positions[geometry.Indices[i + 1]];
                    triangle.V3 = geometry.Positions[geometry.Indices[i + 2]];

                    if (geometry.Transform.HasValue)
                    {
                        triangle.V1 = Vector3.Transform(triangle.V1, geometry.Transform.Value);
                        triangle.V2 = Vector3.Transform(triangle.V2, geometry.Transform.Value);
                        triangle.V3 = Vector3.Transform(triangle.V3, geometry.Transform.Value);
                    }

                    if (pickBox.Contains(triangle) != ContainmentType.Disjoint || triangle.Intersects(ray).HasValue)
                    {
                        triangles.Add(triangle.V1);
                        triangles.Add(triangle.V2);
                        triangles.Add(triangle.V3);
                    }
                }
            }

            //if (keyboard.IsKeyDown(Keys.Space))
            {
                //scene.Draw(gameTime.TotalGameTime);
            }
            //else
            {
                primitiveBatch.Begin(PrimitiveSortMode.Deferred, scene.Camera.View, scene.Camera.Projection);
                foreach (var geometry in geometries)
                    primitiveBatch.DrawGeometry(geometry, null, Color.Gray * 0.2f);
                foreach (var boundable in boundables)
                    primitiveBatch.DrawBox(boundable.BoundingBox, null, Color.Blue);
                primitiveBatch.Draw(triangles, null, null, Color.Red);
                primitiveBatch.DrawBox(pickBox, null, Color.Yellow);
                primitiveBatch.DrawSolidBox(pickBox, null, Color.Gray * 0.4f);
                primitiveBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using System.ComponentModel;
using Nine.Graphics.Primitives;
using Nine.Components;
#endregion

namespace DebuggerPrimitives
{
    [Category("Graphics")]
    [DisplayName("Basic Primitives")]
    [Description("This sample demenstrates the use of PrimitiveBatch to draw " +
                 "various commonly used 3D shapes and geometries.")]
    public class DebuggerPrimitiveGame : Microsoft.Xna.Framework.Game
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

        ModelViewerCamera camera;
        Geometry model;
        Texture2D butterfly;
        Texture2D lightning;
        PrimitiveBatch primitiveBatch;
        ModelBatch modelBatch;
        BasicEffect primitiveEffect;
        List<ICustomPrimitive> primitives;

        public DebuggerPrimitiveGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            camera = new ModelViewerCamera(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice, 4096);
            modelBatch = new ModelBatch(GraphicsDevice);

            model = Content.Load<Geometry>("peon");
            butterfly = Content.Load<Texture2D>("butterfly");
            lightning = Content.Load<Texture2D>("lightning");

            primitives = new List<ICustomPrimitive>();
            primitives.Add(new Sphere(GraphicsDevice));
            primitives.Add(new Centrum(GraphicsDevice));
            primitives.Add(new Cube(GraphicsDevice));
            primitives.Add(new Cylinder(GraphicsDevice));
            primitives.Add(new Dome(GraphicsDevice));
            primitives.Add(new Nine.Graphics.Primitives.Plane(GraphicsDevice));
            primitives.Add(new Torus(GraphicsDevice));
            primitives.Add(new Teapot(GraphicsDevice));

            primitiveEffect = new BasicEffect(GraphicsDevice);
            primitiveEffect.TextureEnabled = false;
            primitiveEffect.EnableDefaultLighting();
            primitiveEffect.DiffuseColor = Color.BlueViolet.ToVector3();

            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            BoundingFrustum frustum = new BoundingFrustum(
                Matrix.CreateLookAt(new Vector3(0, 15, 15), Vector3.Zero, Vector3.UnitZ) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 10));

            primitiveBatch.Begin(PrimitiveSortMode.Deferred, camera.View, camera.Projection);
            {
                primitiveBatch.DrawBillboard(butterfly, Vector3.Zero, 2, Color.White);
                primitiveBatch.DrawSphere(new BoundingSphere(Vector3.UnitX * 4, 1), 24, null, Color.White);
                primitiveBatch.DrawGrid(1, 128, 128, null, Color.White * 0.25f);
                primitiveBatch.DrawGrid(8, 16, 16, null, Color.Black);
                primitiveBatch.DrawLine(new Vector3(5, 5, 0), new Vector3(5, 5, 5), Color.Blue);
                primitiveBatch.DrawConstrainedBillboard(null, new Vector3(5, -5, 0), new Vector3(5, -5, 5), 0.05f, null, null, Color.Yellow);
                primitiveBatch.DrawConstrainedBillboard(lightning, new Vector3[] { new Vector3(5, -5, 0), new Vector3(5, 0, 2), new Vector3(5, 5, 0) }, 1f, null, null, Color.White);
                primitiveBatch.DrawArrow(Vector3.Zero, Vector3.UnitZ * 2, null, Color.White);
                primitiveBatch.DrawBox(new BoundingBox(-Vector3.One, Vector3.One), null, Color.White);
                primitiveBatch.DrawSolidBox(new BoundingBox(-Vector3.One, Vector3.One), null, Color.Yellow * 0.2f);
                primitiveBatch.DrawCircle(Vector3.UnitX * 2, 1, 24, null, Color.Yellow);
                primitiveBatch.DrawSolidSphere(new BoundingSphere(Vector3.UnitX * 4, 1), 24, null, Color.Red * 0.2f);
                primitiveBatch.DrawGeometry(model, Matrix.CreateTranslation(-4, 0, 0), Color.Yellow * 0.5f);
                primitiveBatch.DrawAxis(Matrix.CreateTranslation(-4, 0, 0));
                primitiveBatch.DrawFrustum(frustum, null, Color.White);
                primitiveBatch.DrawSolidFrustum(frustum, null, Color.Pink * 0.5f);
                primitiveBatch.DrawCentrum(new Vector3(-5, -2, 0), 2, 1, 24, null, Color.WhiteSmoke * 0.5f);
                primitiveBatch.DrawSolidCentrum(new Vector3(-5, -2, 0), 2, 1, 24, null, Color.LawnGreen * 0.3f);
                primitiveBatch.DrawCylinder(new Vector3(-5, -6, 0), 2, 1, 24, null, Color.WhiteSmoke * 0.5f);
                primitiveBatch.DrawSolidCylinder(new Vector3(-5, -6, 0), 2, 1, 24, null, Color.Lavender * 0.3f);
            }
            primitiveBatch.End();

            modelBatch.Begin(camera.View, camera.Projection);
            {
                for (int i = 0; i < primitives.Count; i++)
                {
                    Matrix world = Matrix.CreateTranslation(i * 4 - 16, 5, 0);

                    modelBatch.DrawPrimitive(primitives[i], world, primitiveEffect);
                }
            }
            modelBatch.End();

            base.Draw(gameTime);
        }
    }
}
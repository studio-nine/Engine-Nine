#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Primitives;
using Nine.Graphics.ObjectModel;
#endregion

namespace DebuggerPrimitives
{
    [Category("Graphics")]
    [DisplayName("Basic Primitives")]
    [Description("This sample demenstrates the use of primitiveGroup to draw " +
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
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 800;
#endif

        ModelViewerCamera camera;
        Geometry model;
        Texture2D butterfly;
        Texture2D lightning;
        Scene scene;
        PrimitiveGroup primitiveGroup;

        public DebuggerPrimitiveGame()
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

            model = Content.Load<Geometry>("peon");
            butterfly = Content.Load<Texture2D>("butterfly");
            lightning = Content.Load<Texture2D>("lightning");

            scene = new Scene(GraphicsDevice);
            scene.Add(new Sphere(GraphicsDevice));
            scene.Add(new Centrum(GraphicsDevice));
            scene.Add(new Box(GraphicsDevice));
            scene.Add(new Cylinder(GraphicsDevice));
            scene.Add(new Dome(GraphicsDevice));
            scene.Add(new Nine.Graphics.Primitives.Plane(GraphicsDevice));
            scene.Add(new Torus(GraphicsDevice));
            scene.Add(new Teapot(GraphicsDevice));
            
            scene.Add(primitiveGroup = new PrimitiveGroup(GraphicsDevice));
            
            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(47, 79, 79, 255));

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            BoundingFrustum frustum = new BoundingFrustum(
                Matrix.CreateLookAt(new Vector3(0, 15, 15), Vector3.Zero, Vector3.UnitZ) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 10));

            primitiveGroup.Clear();

            //primitiveGroup.AddBillboard(butterfly, Vector3.Zero, 2, Color.White);
            primitiveGroup.AddSphere(new BoundingSphere(Vector3.UnitX * 4, 1), 24, null, Color.White);
            primitiveGroup.AddGrid(1, 128, 128, null, Color.White * 0.25f);
            primitiveGroup.AddGrid(8, 16, 16, null, Color.Black);
            primitiveGroup.AddLine(new Vector3(5, 5, 0), new Vector3(5, 5, 5), new Color(0, 0, 255, 255));
            //primitiveGroup.AddConstrainedBillboard(null, new Vector3(5, -5, 0), new Vector3(5, -5, 5), 0.05f, null, null, new Color(255, 255, 0, 255));
            //primitiveGroup.AddConstrainedBillboard(lightning, new Vector3[] { new Vector3(5, -5, 0), new Vector3(5, 0, 2), new Vector3(5, 5, 0) }, 1f, null, null, Color.White);
            //primitiveGroup.AddArrow(Vector3.Zero, Vector3.UnitZ * 2, null, Color.White);
            primitiveGroup.AddBox(new BoundingBox(-Vector3.One, Vector3.One), null, Color.White);
            primitiveGroup.AddSolidBox(new BoundingBox(-Vector3.One, Vector3.One), null, new Color(255, 255, 0, 255) * 0.2f);
            primitiveGroup.AddCircle(Vector3.UnitX * 2, 1, 24, null, new Color(255, 255, 0, 255));
            primitiveGroup.AddSolidSphere(new BoundingSphere(Vector3.UnitX * 4, 1), 24, null, new Color(255, 0, 0, 255) * 0.2f);
            primitiveGroup.AddGeometry(model, Matrix.CreateTranslation(-4, 0, 0), new Color(255, 255, 0, 255) * 0.5f);
            //primitiveGroup.AddAxis(Matrix.CreateTranslation(-4, 0, 0));
            primitiveGroup.AddFrustum(frustum, null, Color.White);
            primitiveGroup.AddSolidFrustum(frustum, null, new Color(255, 192, 203, 255) * 0.5f);
            primitiveGroup.AddCentrum(new Vector3(-5, -2, 0), 2, 1, 24, null, new Color(245, 245, 245, 255) * 0.5f);
            primitiveGroup.AddSolidCentrum(new Vector3(-5, -2, 0), 2, 1, 24, null, new Color(124, 252, 0, 255) * 0.3f);
            primitiveGroup.AddCylinder(new Vector3(-5, -6, 0), 2, 1, 24, null, new Color(245, 245, 245, 255) * 0.5f);
            primitiveGroup.AddSolidCylinder(new Vector3(-5, -6, 0), 2, 1, 24, null, new Color(230, 230, 255, 255) * 0.3f);

            scene.Draw(gameTime.ElapsedGameTime);
            base.Draw(gameTime);
        }
    }
}
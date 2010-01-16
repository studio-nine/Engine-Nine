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
#endregion


namespace Isles.Samples
{
    public class BasicModelViewerGame : Microsoft.Xna.Framework.Game
    {
        Grid grid;
        Grid grid1;
        public bool ShowAxis { get; set; }
        public bool ShowGrid { get; set; }
        PropertyBrowser browser; 
       

        public Axis Axis { get; private set; }
        public Matrix AxisTransform { get; set; }
        public bool Wireframe { get; private set; }
        public BasicEffect BasicEffect { get; private set; }
        public ModelViewerCamera Camera { get; private set; }
        public FrameRate FrameRate { get; private set; }
        public Input Input { get { return Camera.Input; } }
        

        GraphicsDeviceManager graphicsManager;

        public BasicModelViewerGame()
        {
            graphicsManager = new GraphicsDeviceManager(this);
            graphicsManager.PreferredBackBufferWidth = 900;
            graphicsManager.PreferredBackBufferHeight = 600;

            ShowAxis = true;
            ShowGrid = true;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            AxisTransform = Matrix.Identity;

            Camera = new ModelViewerCamera(this, 30, 1, 200, Vector3.Up);
            Camera.Input.KeyDown += new EventHandler<KeyboardEventArgs>(Input_KeyDown);

            Components.Add(FrameRate = new FrameRate(this, "Fonts/Arial"));
        }

        void Input_KeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.W)
                Wireframe = !Wireframe;
            else if (e.Key == Microsoft.Xna.Framework.Input.Keys.A)
                ShowAxis = !ShowAxis;
            else if (e.Key == Microsoft.Xna.Framework.Input.Keys.G)
                ShowGrid = !ShowGrid;
            else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter &&
                Input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt))
                graphicsManager.ToggleFullScreen();
            else if (e.Key == Microsoft.Xna.Framework.Input.Keys.E)
            {
                browser = PropertyBrowser.ShowForm("Designer");
                browser.SelectedObject = this;
            }
        }

        protected override void LoadContent()
        {
            Axis = new Axis(GraphicsDevice);
            grid = new Grid(GraphicsDevice, 10, 10, 8, 8);
            grid1 = new Grid(GraphicsDevice, 10, 10, 2, 2);

            BasicEffect = new BasicEffect(GraphicsDevice, null);
            
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (ShowAxis)
                Axis.Draw(AxisTransform, Camera.View, Camera.Projection);

            if (ShowGrid)
            {
                grid.Draw(Matrix.Identity, Camera.View, Camera.Projection, Color.White);
                grid1.Draw(Matrix.Identity, Camera.View, Camera.Projection, Color.Red);
            }

            base.Draw(gameTime);

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            if (Wireframe)
                GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            else
                GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            base.Draw(gameTime);
        }
    }
}

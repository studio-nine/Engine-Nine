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
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Navigation;
using Nine.Animations;
#endregion

namespace NavigationSample
{
    /// <summary>
    /// Demonstrates how to control unit movement.
    /// </summary>
    public class NavigationGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;
        ScreenCamera screenCamera;

        Input input;
        ModelBatch modelBatch;
        PrimitiveBatch primitiveBatch;
        DrawableSurface terrain;
        BasicEffect basicEffect;

        GridObjectManager<Unit> objectManager;
        List<Unit> units;
        List<Unit> selectedUnits = new List<Unit>();

        Point beginSelect;
        Point endSelect;
        BoundingFrustum pickFrustum;

        Random random = new Random();
        BoundingRectangle bounds;

        Vector3 destination;

        public NavigationGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a top down perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);

            screenCamera = new ScreenCamera(GraphicsDevice, ScreenCameraCoordinate.TwoDimension);
            screenCamera.Input.Enabled = false;

            // Create a terrain based on the terrain geometry loaded from file
            terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("MountainHeightmap"), 16);
            terrain.TextureTransform = TextureTransform.CreateScale(0.5f, 0.5f);
            //terrain.Position = new Vector3(12f, 20f, -10);
            //terrain.Position = new Vector3(12f, 20f, 0);
            terrain.Invalidate();

            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);

            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = Content.Load<Texture2D>("checker");
            basicEffect.EnableDefaultLighting();
            basicEffect.SpecularColor = Vector3.Zero;

            // Initialize navigators
            bounds = new BoundingRectangle(terrain.BoundingBox);
            //objectManager = new QuadTreeObjectManager<Navigator>(bounds, 8);
            objectManager = new GridObjectManager<Unit>(bounds, 64, 64);
            units = new List<Unit>();

            Model model = Content.Load<Model>("Peon");

            ISpatialQuery<Navigator> friends = new SpatialQuery<Unit, Navigator>(objectManager) { Converter = o => o.Navigator };
            for (int i = 0; i < 100; i++)
            {
                Unit unit = new Unit(model, terrain, friends);
                unit.Navigator.Position = NextPosition();
                units.Add(unit);
            }

            // Initialize inputs
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(input_MouseMove);
            input.MouseUp += new EventHandler<MouseEventArgs>(input_MouseUp);
        }

        private Vector3 NextPosition()
        {
            // Randomize positions
            Vector3 position = new Vector3();

            position.X = (float)random.NextDouble() * (bounds.Max.X - bounds.Min.X) + bounds.Min.X;
            position.Y = (float)random.NextDouble() * (bounds.Max.Y - bounds.Min.Y) + bounds.Min.Y;
            position.Z = 0;

            return position;
        }

        void input_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectedUnits.Clear();
                beginSelect = endSelect = e.Position;
            }
            else if (e.Button == MouseButtons.Right)
            {
                Ray pickRay = GraphicsDevice.Viewport.CreatePickRay(e.X, e.Y, camera.View, camera.Projection);
                float? distance = terrain.Intersects(pickRay);

                if (distance.HasValue)
                {
                    destination = pickRay.Position + pickRay.Direction * distance.Value;

                    foreach (Unit unit in selectedUnits)
                    {
                        unit.Navigator.MoveTo(destination);
                    }
                }
            }
        }

        void input_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.IsLeftButtonDown)
            {
                endSelect = e.Position;
            }
        }

        void input_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.Position.X - beginSelect.X;
                int y = e.Position.Y - beginSelect.Y;

                if (x * x + y * y < 10)
                {
                    // Select single object from ray
                    Ray pickRay = GraphicsDevice.Viewport.CreatePickRay(e.X, e.Y, camera.View, camera.Projection);

                    Unit selected = objectManager.FindFirst(pickRay);

                    if (selected != null)
                        selectedUnits.Add(selected);
                }
                else
                {
                    // Select multiple objects from frustum
                    pickFrustum = GraphicsDevice.Viewport.CreatePickFrustum(beginSelect, e.Position, camera.View, camera.Projection);

                    foreach (Unit selected in objectManager.Find(pickFrustum))
                    {
                        selectedUnits.Add(selected);
                    }
                }

                beginSelect = endSelect = Point.Zero;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            objectManager.Clear();

            foreach (Unit unit in units)
            {
                unit.Update(gameTime);
                objectManager.Add(unit, unit.Navigator.Position, unit.Navigator.BoundingRadius);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (DrawableSurfacePatch patch in terrain.Patches)
            {
                // Cull patches that are outside the view frustum
                if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                {
                    // Setup matrices
                    basicEffect.World = patch.Transform;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    // Draw each path with the specified effect
                    patch.Draw(basicEffect);
                }
            }


            // Draw models
            modelBatch.Begin(camera.View, camera.Projection);
            {
                foreach (Unit unit in units)
                {
                    if (frustum.Contains(unit.Navigator.Position) == ContainmentType.Contains)
                        unit.Draw(gameTime, modelBatch, primitiveBatch);
                }
            }
            modelBatch.End();


            // Draw selected units
            primitiveBatch.Begin(camera.View, camera.Projection);
            {
                //primitiveBatch.DrawGrid(objectManager, null, Color.White);
                //primitiveBatch.DrawCollision(units[0].Model, Unit.WorldTransform * units[0].Navigator.Transform, Color.Firebrick);
                //if (pickFrustum != null)
                //    primitiveBatch.DrawSolidFrustum(pickFrustum, null, Color.LightPink);
                primitiveBatch.DrawArrow(destination + Vector3.UnitZ * 2, destination, null, Color.LightGreen * 0.4f);

                foreach (Unit unit in selectedUnits)
                {
                    primitiveBatch.DrawCylinder(unit.Navigator.Position, 0.5f, unit.Navigator.BoundingRadius, 12, null, Color.YellowGreen);
                }
            }
            primitiveBatch.End();


            primitiveBatch.Begin(screenCamera.View, screenCamera.Projection);
            {
                primitiveBatch.DrawRectangle(new Vector2(beginSelect.X, beginSelect.Y),
                                             new Vector2(endSelect.X, endSelect.Y), null, Color.LightGreen);
            }
            primitiveBatch.End();


            base.Draw(gameTime);
        }
    }
}

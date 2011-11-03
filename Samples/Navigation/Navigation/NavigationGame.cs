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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
using Nine.Navigation;
using Nine.Animations;
using Nine.Components;
using Model = Microsoft.Xna.Framework.Graphics.Model;
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

        GridSceneManager<LineSegment> walls;
        GridSceneManager<Unit> objectManager;
        List<Unit> units;
        List<Unit> selectedUnits = new List<Unit>();

        Point beginSelect;
        Point endSelect;
        BoundingFrustum pickFrustum;

        Random random = new Random();
        BoundingRectangle bounds;

        Vector3 destination;

        List<Unit> nearbyUnits = new List<Unit>();

        public NavigationGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = true;
            Window.AllowUserResizing = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            // Create a top down perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);

            screenCamera = new ScreenCamera(GraphicsDevice, ScreenCameraCoordinate.TwoDimension);
            screenCamera.Input.Enabled = false;

            // Create a terrain based on the terrain geometry loaded from file
            terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("MountainHeightmap"), 16);
            terrain.TextureTransform = TextureTransform.CreateScale(0.5f, 0.5f);
            terrain.Position = -terrain.Center;

            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);

            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = Content.Load<Texture2D>("checker");
            basicEffect.EnableDefaultLighting();
            basicEffect.SpecularColor = Vector3.Zero;

            bounds = new BoundingRectangle(terrain.BoundingBox);

            walls = new GridSceneManager<LineSegment>(bounds, 1, 1);

            walls.Add(new LineSegment(new Vector2(20, -10), new Vector2(20, 10)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(20, 10), new Vector2(25, 16)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(25, 16), new Vector2(30, 10)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(30, 10), new Vector2(30, -10)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(30, -10), new Vector2(25, -4)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(25, -4), new Vector2(20, -10)), terrain.BoundingBox);

            float corridorWidth = 2.0f;
            walls.Add(new LineSegment(new Vector2(-10, -corridorWidth), new Vector2(10, -corridorWidth)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(10, corridorWidth), new Vector2(-10, corridorWidth)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(10, 30), new Vector2(10, corridorWidth)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(10, -corridorWidth), new Vector2(10, -30)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(-10, -30), new Vector2(-10, -corridorWidth)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(-10, corridorWidth), new Vector2(-10, 30)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(-10, 30), new Vector2(10, 30)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(10, -30), new Vector2(-10, -30)), terrain.BoundingBox);
            

            /*
            walls.Add(new LineSegment(new Vector2(55.94017f, 46.50343f), new Vector2(8.139015f, 34.90085f)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(30.34578f, 59.6571f), new Vector2(55.94017f, 46.50343f)), terrain.BoundingBox);
            walls.Add(new LineSegment(new Vector2(8.139015f, 34.90085f), new Vector2(30.34578f, 59.6571f)), terrain.BoundingBox);
            */
            // Initialize navigators
            //objectManager = new QuadTreeObjectManager<Navigator>(bounds, 8);
            objectManager = new GridSceneManager<Unit>(bounds, 64, 64);
            units = new List<Unit>();

            Model model = Content.Load<Model>("Peon");

            int n = 20;
            ISpatialQuery<Navigator> friends = new SpatialQuery<Unit, Navigator>(objectManager) { Filter = null, Converter = o => o.Navigator };
            for (int i = 0; i < n; i++)
            {
                Unit unit = new Unit(model, terrain, friends, walls);
                //unit.Navigator.Position = NextPosition();
                unit.Navigator.Position = Vector3.UnitX * (i - n / 2);
                units.Add(unit);
            }

            //units[0].Navigator.MoveTo(new Vector3(20, 0, 0));

            // Initialize inputs
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            input.MouseMove += new EventHandler<MouseEventArgs>(input_MouseMove);
            input.MouseUp += new EventHandler<MouseEventArgs>(input_MouseUp);
            input.KeyDown += new EventHandler<KeyboardEventArgs>(input_KeyDown);
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

        void input_KeyDown(object sender, KeyboardEventArgs e)
        {
            foreach (Unit unit in selectedUnits)
            {
                if (e.Key == Keys.S)
                {
                    unit.Navigator.HoldPosition = false;
                    unit.Navigator.Stop();
                }
                else if (e.Key == Keys.H)
                {
                    unit.Navigator.HoldPosition = true;
                    unit.Navigator.Stop();
                }
            } 
            
            if (e.Key == Keys.Space)
            {
                units[0].Navigator.MoveTo(new Vector3(23.89655f, 53.31915f, 0));
            }
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
            if (e.IsButtonDown(MouseButtons.Left))
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

                    objectManager.FindAll(ref pickRay, selectedUnits);

                    if (selectedUnits.Count > 1)
                        selectedUnits.RemoveRange(1, selectedUnits.Count);
                }
                else
                {
                    // Select multiple objects from frustum
                    pickFrustum = GraphicsDevice.Viewport.CreatePickFrustum(beginSelect, e.Position, camera.View, camera.Projection);

                    objectManager.FindAll(ref pickFrustum, selectedUnits);
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
                objectManager.Add(unit, unit.Navigator.Position, unit.Navigator.SoftBoundingRadius);
            }

            foreach (Unit unit in units)
            {
                unit.Update(gameTime);
            }
            
            nearbyUnits.Clear();
            if (selectedUnits.Count > 0)
            {
                float range = (float)(selectedUnits[0].Navigator.SoftBoundingRadius + selectedUnits[0].Navigator.MaxSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                var bounds = new BoundingSphere(selectedUnits[0].Navigator.Position, range);
                objectManager.FindAll(ref bounds, nearbyUnits);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && selectedUnits.Count > 0)
                System.Diagnostics.Debug.WriteLine(selectedUnits[0].Navigator.Speed + "\t" + selectedUnits[0].Navigator.steerable.Forward.ToString());

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

            foreach (var patch in terrain.Patches)
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
                        ;// unit.Draw(gameTime, modelBatch, primitiveBatch);
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
                //primitiveBatch.DrawArrow(destination + Vector3.UnitZ * 2, destination, null, Color.LightGreen * 0.4f);
               
                foreach (Unit unit in units)
                {
                    primitiveBatch.DrawCylinder(unit.Navigator.Position, 0.5f, unit.Navigator.SoftBoundingRadius, 12, null, Color.YellowGreen);
                    primitiveBatch.DrawCylinder(unit.Navigator.Position, 0.5f, unit.Navigator.HardBoundingRadius, 12, null, Color.Pink);
                     
                    primitiveBatch.DrawArrow(unit.Navigator.Position, unit.Navigator.Position + Vector3.Normalize(new Vector3(unit.Navigator.steerable.Force, 0)) * 2, null, Color.Magenta);
                    primitiveBatch.DrawArrow(unit.Navigator.Position, unit.Navigator.Position + Vector3.Normalize(new Vector3(unit.Navigator.steerable.Forward, 0)) * 2, null, Color.Yellow);
                                        
                    if (unit.Navigator.steerable.Target.HasValue && selectedUnits.Contains(unit))
                    {
                        Vector3 target = new Vector3(unit.Navigator.steerable.Target.Value, 0);
                        primitiveBatch.DrawArrow(target + Vector3.UnitZ * 2, target, null, Color.Green);
                    }
                }
                foreach (Unit unit in nearbyUnits)
                {
                    ;// primitiveBatch.DrawCylinder(unit.Navigator.Position, 0.5f, unit.Navigator.SoftBoundingRadius, 12, null, Color.Red);
                }

                if (walls != null)
                {
                    foreach (LineSegment line in walls)
                    {
                        primitiveBatch.DrawLineSegment(line, 0.2f, 2, null, Color.White);
                    }
                }

                if (selectedUnits.Count > 0)
                {
                    //BoundingCircle? circle = ((Nine.Navigation.Steering.ObstacleAvoidanceBehavior)selectedUnits[0].Navigator.steerer.Behaviors[1]).CurrentObstacle;

                    //if (circle.HasValue)
                    //    primitiveBatch.DrawCircle(new Vector3(circle.Value.Center, 1), circle.Value.Radius, 24, null, Color.AliceBlue);
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

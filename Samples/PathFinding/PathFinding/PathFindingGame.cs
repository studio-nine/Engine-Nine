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
using Nine.Components;
#endregion

namespace PathFinding
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PathFindingGame : Microsoft.Xna.Framework.Game
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

        Input input;
        TopDownEditorCamera camera;
        PrimitiveBatch primitiveBatch;

        // The path graph to be searched
        PathGrid pathGraph;

        // An A* seach algorithm.
        GraphSearch<PathGridNode> graphSearch = new GraphSearch<PathGridNode>();

        // A list of nodes containing the result path.
        List<PathGridNode> path = new List<PathGridNode>();

        // Start node of the path.
        PathGridNode? start;


        public PathFindingGame()
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

            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);

            // Handle input events
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(OnMouseDown);


            // Create a path graph
            pathGraph = new PathGrid(0, 0, 128, 128, 64, 64);


            // Create some random obstacles
            Random random = new Random();

            for (int i = 0; i < 800; i++)
            {
                pathGraph.Mark(random.Next(pathGraph.SegmentCountX),
                               random.Next(pathGraph.SegmentCountY));
            }
        }

        /// <summary>
        /// Handle mouse down event.
        /// </summary>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!start.HasValue)
                {
                    // Get start path graph node.
                    start = GetPickedNode(e.X, e.Y);
                    path.Clear();
                }
                else
                {
                    PathGridNode? end = GetPickedNode(e.X, e.Y);

                    if (end.HasValue)
                    {
                        // Search from start to end.
                        // NOTE: the path return from A* search is from end to start.
                        path.Clear();
                        path.AddRange(
                            graphSearch.Search(pathGraph, start.Value, end.Value));

                        start = null;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to get picked path graph node from screen coordinates.
        /// </summary>
        private PathGridNode? GetPickedNode(int x, int y)
        {
            // Gets the pick ray from current mouse cursor
            Ray ray = GraphicsDevice.Viewport.CreatePickRay(x, y, camera.View, camera.Projection);
            // Test ray against ground plane
            float? distance = ray.Intersects(new Plane(Vector3.UnitZ, 0));

            if (distance.HasValue)
            {
                Vector3 target = ray.Position + ray.Direction * distance.Value;

                // Make sure we don't pick an obstacle.
                if (!pathGraph.IsMarked(target.X, target.Y))
                    return pathGraph[target.X, target.Y];
            }

            return null;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            primitiveBatch.Begin(camera.View, camera.Projection);
            {
                // Draw grid
                primitiveBatch.DrawGrid(pathGraph, null, Color.Gray);

                // Draw obstacles
                for (int x = 0; x < pathGraph.SegmentCountX; x++)
                {
                    for (int y = 0; y < pathGraph.SegmentCountY; y++)
                    {
                        if (pathGraph.IsMarked(x, y))
                        {
                            Vector3 center = new Vector3(pathGraph.SegmentToPosition(x, y), 0);

                            primitiveBatch.DrawSolidBox(center, Vector3.One * 2, null, Color.Gold);
                        }
                    }
                }

                // Draw start node
                if (start.HasValue)
                {
                    primitiveBatch.DrawSolidSphere(new Vector3(pathGraph.SegmentToPosition(start.Value.X, start.Value.Y), 0), 0.5f, 12, null, Color.Honeydew);
                }
                
                // Draw path
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 point1 = new Vector3(pathGraph.SegmentToPosition(path[i].X, path[i].Y), 0);
                    Vector3 point2 = new Vector3(pathGraph.SegmentToPosition(path[i + 1].X, path[i + 1].Y), 0);

                    primitiveBatch.DrawLine(point1, point2, null, Color.GreenYellow);
                }
            }
            primitiveBatch.End();

            base.Draw(gameTime);
        }
    }
}
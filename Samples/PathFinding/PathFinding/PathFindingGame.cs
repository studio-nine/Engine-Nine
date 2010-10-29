#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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

            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a topdown perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(Services);

            // Handle input events
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(OnMouseDown);


            // Create a path graph
            pathGraph = new PathGrid(128, 128, 0, 0, 64, 64);


            // Create some random obstacles
            Random random = new Random();

            for (int i = 0; i < 800; i++)
            {
                pathGraph.Mark(random.Next(pathGraph.TessellationX),
                               random.Next(pathGraph.TessellationY));
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
            Ray ray = PickEngine.RayFromScreen(GraphicsDevice, x, y, camera.View, camera.Projection);
            
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


            DebugVisual.Alpha = 1.0f;
            DebugVisual.View = camera.View;
            DebugVisual.Projection = camera.Projection;


            // Draw grid
            DebugVisual.DrawGrid(GraphicsDevice, Vector3.Zero, 2, 64, 64, Color.Gray);


            // Draw obstacles
            for (int x = 0; x < pathGraph.TessellationX; x++)
            {
                for (int y = 0; y < pathGraph.TessellationY; y++)
                {
                    if (pathGraph.IsMarked(x, y))
                    {
                        Vector3 center = new Vector3(pathGraph.GridToPosition(x, y), 0);

                        DebugVisual.DrawBox(GraphicsDevice,
                                            center, Vector3.One * 2,
                                            Matrix.Identity, Color.Gold);
                    }
                }
            }


            // Draw start node
            if (start.HasValue)
            {
                DebugVisual.DrawPoint(GraphicsDevice,
                                      new Vector3(pathGraph.GridToPosition(start.Value.X, start.Value.Y), 0),
                                      Color.Honeydew, 2);
            }


            // Draw path
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 point1 = new Vector3(pathGraph.GridToPosition(path[i].X, path[i].Y), 0);
                Vector3 point2 = new Vector3(pathGraph.GridToPosition(path[i + 1].X, path[i + 1].Y), 0);

                DebugVisual.DrawLine(GraphicsDevice, point1, point2, 0.4f, Color.GreenYellow);
            }


            base.Draw(gameTime);
        }
    }
}
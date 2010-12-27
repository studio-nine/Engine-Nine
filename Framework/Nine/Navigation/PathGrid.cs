#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Navigation
{
    #region PathGridNode
    /// <summary>
    /// Node used by PathGraph.
    /// </summary>
    public struct PathGridNode : IGraphNode
    {
        /// <summary>
        /// Identifier of this node.
        /// </summary>
        public int ID;

        /// <summary>
        /// X grid index of this node.
        /// </summary>
        public int X;

        /// <summary>
        /// Y grid index of this node.
        /// </summary>
        public int Y;

        int IGraphNode.ID { get { return ID; } }

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        public override string ToString()
        {
            return X + ", " + Y + ", " + ID;
        }
    }
    #endregion

    #region PathGrid
    /// <summary>
    /// A grid based path graph.
    /// </summary>
    public class PathGrid : UniformGrid, IGraph<PathGridNode>
    {
        private byte[,] data;

        /// <summary>
        /// Gets or sets the boundary of the PathGraph. 
        /// This value must be contained by Rectangle(0, 0, TessellationX, TessellationY).
        /// When the path graph is searched, the search process will be restricted to the boundary.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Creates a new PathGraph.
        /// </summary>
        public PathGrid(float width, float height, float x, float y, int countX, int countY)
            : base(width, height, x, y, countX, countY)
        {
            data = new byte[countX, countY];

            Bounds = new Rectangle(0, 0, countX, countY);
        }

        /// <summary>
        /// Marks one grid as obstacle. Input boundary is not checked.
        /// </summary>
        public void Mark(int x, int y)
        {
            data[x, y]++;
        }

        /// <summary>
        /// Marks the grid under the specified location as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public void Mark(float x, float y)
        {
            Point pt = PositionToGrid(Clamp(x, y));

            data[pt.X, pt.Y]++;
        }

        /// <summary>
        /// Unmark one grid. Input boundary is not checked.
        /// </summary>
        public void Unmark(int x, int y)
        {
            System.Diagnostics.Debug.Assert(data[x, y] > 0);

            data[x, y]--;
        }

        /// <summary>
        /// Unmark the grid under the specified location as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public void Unmark(float x, float y)
        {
            Point pt = PositionToGrid(Clamp(x, y));

            System.Diagnostics.Debug.Assert(data[pt.X, pt.Y] > 0);

            data[pt.X, pt.Y]--;
        }

        /// <summary>
        /// Checks if the specified grid is marked as obstacle.
        /// </summary>
        public bool IsMarked(int x, int y)
        {
            return data[x, y] > 0;
        }

        /// <summary>
        /// Checks if the grid under the specified location is marked as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public bool IsMarked(float x, float y)
        {
            Point pt = PositionToGrid(Clamp(x, y));

            return data[pt.X, pt.Y] > 0;
        }

        /// <summary>
        /// Gets total node count.
        /// </summary>
        public int Count
        {
            get { return GridCountX * GridCountY; }
        }

        /// <summary>
        /// Gets the path graph node under the specified grid.
        /// </summary>
        public PathGridNode this[float x, float y]
        {
            get
            {
                Point pt = PositionToGrid(Clamp(x, y));

                PathGridNode node;

                node.X = pt.X;
                node.Y = pt.Y;
                node.ID = pt.X + pt.Y * GridCountX;

                return node;
            }
        }

        /// <summary>
        /// Gets the path graph node under the specified location.
        /// Input location is truncated.
        /// </summary>
        public PathGridNode this[int x, int y]
        {
            get
            {
                PathGridNode node;

                node.X = x;
                node.Y = y;
                node.ID = x + y * GridCountX;

                return node;
            }
        }

        /// <summary>
        /// Gets all the adjacent edges of the specified node.
        /// </summary>
        public IEnumerable<GraphEdge<PathGridNode>> GetEdges(PathGridNode node)
        {
            int x = node.X;
            int y = node.Y;

            GraphEdge<PathGridNode> edge;

            edge.From = node;

            if (x > Bounds.Left && data[x - 1, y] == 0)
            {
                edge.To.X = x - 1;
                edge.To.Y = y;
                edge.To.ID = y * GridCountX + x - 1;
                edge.Cost = 1.0f;

                yield return edge;
            }
            if (x < Bounds.Right - 1 && data[x + 1, y] == 0)
            {
                edge.To.X = x + 1;
                edge.To.Y = y;
                edge.To.ID = y * GridCountX + x + 1;
                edge.Cost = 1.0f;

                yield return edge;
            }
            if (y > Bounds.Top && data[x, y - 1] == 0)
            {
                edge.To.X = x;
                edge.To.Y = y - 1;
                edge.To.ID = (y - 1) * GridCountX + x;
                edge.Cost = 1.0f;

                yield return edge;
            }
            if (y < Bounds.Bottom - 1 && data[x, y + 1] == 0)
            {
                edge.To.X = x;
                edge.To.Y = y + 1;
                edge.To.ID = (y + 1) * GridCountX + x;
                edge.Cost = 1.0f;

                yield return edge;
            }
            if (x > Bounds.Left && y > Bounds.Top && data[x - 1, y - 1] == 0)
            {
                edge.To.X = x - 1;
                edge.To.Y = y - 1;
                edge.To.ID = (y - 1) * GridCountX + x - 1;
                edge.Cost = 1.4142135f;

                yield return edge;
            }
            if (x > Bounds.Left && y < Bounds.Bottom - 1 && data[x - 1, y + 1] == 0)
            {
                edge.To.X = x - 1;
                edge.To.Y = y + 1;
                edge.To.ID = (y + 1) * GridCountX + x - 1;
                edge.Cost = 1.4142135f;

                yield return edge;
            }
            if (x < Bounds.Right - 1 && y > Bounds.Top && data[x + 1, y - 1] == 0)
            {
                edge.To.X = x + 1;
                edge.To.Y = y - 1;
                edge.To.ID = (y - 1) * GridCountX + x + 1;
                edge.Cost = 1.4142135f;

                yield return edge;
            }
            if (x < Bounds.Right - 1 && y < Bounds.Bottom - 1 && data[x + 1, y + 1] == 0)
            {
                edge.To.X = x + 1;
                edge.To.Y = y + 1;
                edge.To.ID = (y + 1) * GridCountX + x + 1;
                edge.Cost = 1.4142135f;

                yield return edge;
            }
        }

        /// <summary>
        /// Gets the heuristic value used by A star search.
        /// </summary>
        public float GetHeuristicValue(PathGridNode current, PathGridNode end)
        {
            int xx = current.X - end.X;
            int yy = current.Y - end.Y;

            return (float)Math.Sqrt(xx * xx + yy * yy);
        }
    }
    #endregion
}

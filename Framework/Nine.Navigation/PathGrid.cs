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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// A grid based path graph.
    /// </summary>
    public class PathGrid : UniformGrid, IGraph, IPathGraph
    {
        [ContentSerializer(Optional=true, ElementName="Data")]
        private byte[] data;

        /// <summary>
        /// Gets or sets the boundary of the PathGraph. 
        /// This value must be contained by Rectangle(0, 0, TessellationX, TessellationY).
        /// When the path graph is searched, the search process will be restricted to the boundary.
        /// </summary>
        public Rectangle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        private Rectangle bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathGrid"/> class.
        /// </summary>
        internal PathGrid() { }

        /// <summary>
        /// Creates a new PathGraph.
        /// </summary>
        public PathGrid(float x, float y, float width, float height, int countX, int countY) 
            : base(x, y, width, height, countX, countY)
        {
            data = new byte[countX * countY];

            Position = new Vector2(x, y);

            Bounds = new Rectangle(0, 0, countX, countY);
        }

        /// <summary>
        /// Marks one grid as obstacle. Input boundary is not checked.
        /// </summary>
        public void Mark(int x, int y)
        {
            // Assert x and y does not exceed limits
            System.Diagnostics.Debug.Assert(Contains(x, y));

            data[x + y * SegmentCountX]++;
        }

        /// <summary>
        /// Marks the grid under the specified location as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public void Mark(float x, float y)
        {
            Point pt = PositionToSegment(Clamp(x, y));

            // Assert x and y does not exceed limits
            System.Diagnostics.Debug.Assert(Contains(pt.X, pt.Y));

            data[pt.X + pt.Y * SegmentCountX]++;
        }

        /// <summary>
        /// Unmark one grid. Input boundary is not checked.
        /// </summary>
        public void Unmark(int x, int y)
        {
            System.Diagnostics.Debug.Assert(Contains(x, y) && data[x + y * SegmentCountX] > 0);

            data[x + y * SegmentCountX]--;
        }

        /// <summary>
        /// Unmark the grid under the specified location as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public void Unmark(float x, float y)
        {
            Point pt = PositionToSegment(Clamp(x, y));

            System.Diagnostics.Debug.Assert(Contains(pt.X, pt.Y) && data[pt.X + pt.Y * SegmentCountX] > 0);

            data[pt.X + pt.Y * SegmentCountX]--;
        }

        /// <summary>
        /// Checks if the specified grid is marked as obstacle.
        /// </summary>
        public bool IsMarked(int x, int y)
        {
            System.Diagnostics.Debug.Assert(Contains(x, y));

            return data[x + y * SegmentCountX] > 0;
        }

        /// <summary>
        /// Checks if the grid under the specified location is marked as obstacle. 
        /// Input location is truncated.
        /// </summary>
        public bool IsMarked(float x, float y)
        {
            Point pt = PositionToSegment(Clamp(x, y));

            System.Diagnostics.Debug.Assert(Contains(pt.X, pt.Y));

            return data[pt.X + pt.Y * SegmentCountX] > 0;
        }
        
        /// <summary>
        /// Gets total node count.
        /// </summary>
        public int NodeCount
        {
            get { return SegmentCountX * SegmentCountY; }
        }

        /// <summary>
        /// Gets the max count of edges for each node.
        /// </summary>
        public int MaxEdgeCount
        {
            get { return 8; }
        }

        /// <summary>
        /// Gets the path graph node under the specified grid.
        /// Input location is turncated.
        /// </summary>
        public int PositionToIndex(float x, float y)
        {
            Point pt = PositionToSegment(Clamp(x, y));
            return pt.X + pt.Y * SegmentCountX;
        }

        /// <summary>
        /// Gets the path graph node under the specified location.
        /// Input location is not truncated.
        /// </summary>
        public int SegmentToIndex(int x, int y)
        {
            return x + y * SegmentCountX;
        }

        /// <summary>
        /// Gets the location of the path node.
        /// </summary>
        public Vector2 IndexToPosition(int node)
        {
            return SegmentToPosition(node % SegmentCountX, node / SegmentCountX);
        }

        /// <summary>
        /// Gets all the adjacent edges of the specified node.
        /// </summary>
        public int GetEdges(int node, GraphEdge[] edges, int startIndex)
        {
            int count = 0;
            int x = node % SegmentCountX;
            int y = node / SegmentCountX;

            GraphEdge edge;
            edge.From = node;

            if (x > bounds.Left && data[x - 1 + y * SegmentCountX] == 0)
            {
                edge.To = y * SegmentCountX + x - 1;
                edge.Cost = 1.0f;

                edges[startIndex++] = edge;
                count++;
            }
            if (x < bounds.Right - 1 && data[x + 1 + y * SegmentCountX] == 0)
            {
                edge.To = y * SegmentCountX + x + 1;
                edge.Cost = 1.0f;

                edges[startIndex++] = edge;
                count++;
            }
            if (y > bounds.Top && data[x + (y - 1) * SegmentCountX] == 0)
            {
                edge.To = (y - 1) * SegmentCountX + x;
                edge.Cost = 1.0f;

                edges[startIndex++] = edge;
                count++;
            }
            if (y < bounds.Bottom - 1 && data[x + (y + 1) * SegmentCountX] == 0)
            {
                edge.To = (y + 1) * SegmentCountX + x;
                edge.Cost = 1.0f;

                edges[startIndex++] = edge;
                count++;
            }
            if (x > bounds.Left && y > bounds.Top && data[x - 1 + (y - 1) * SegmentCountX] == 0)
            {
                edge.To = (y - 1) * SegmentCountX + x - 1;
                edge.Cost = 1.4142135f;

                edges[startIndex++] = edge;
                count++;
            }
            if (x > bounds.Left && y < bounds.Bottom - 1 && data[x - 1 + (y + 1) * SegmentCountX] == 0)
            {
                edge.To = (y + 1) * SegmentCountX + x - 1;
                edge.Cost = 1.4142135f;

                edges[startIndex++] = edge;
                count++;
            }
            if (x < bounds.Right - 1 && y > bounds.Top && data[x + 1 + (y - 1) * SegmentCountX] == 0)
            {
                edge.To = (y - 1) * SegmentCountX + x + 1;
                edge.Cost = 1.4142135f;

                edges[startIndex++] = edge;
                count++;
            }
            if (x < bounds.Right - 1 && y < bounds.Bottom - 1 && data[x + 1 + (y + 1) * SegmentCountX] == 0)
            {
                edge.To = (y + 1) * SegmentCountX + x + 1;
                edge.Cost = 1.4142135f;

                edges[startIndex++] = edge;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Gets the heuristic value used by A star search.
        /// </summary>
        public float GetHeuristicValue(int current, int end)
        {
            int xx = current % SegmentCountX - end % SegmentCountX;
            int yy = current / SegmentCountX - end / SegmentCountX;

            return (float)Math.Sqrt(xx * xx + yy * yy);
        }

        public float GetHeuristicValueManhattan(int current, int end)
        {
            int xx = current % SegmentCountX - end % SegmentCountX;
            int yy = current / SegmentCountX - end / SegmentCountX;

            return Math.Abs(xx) + Math.Abs(yy);
        }

        public float GetHeuristicValueDiagonal(int current, int end)
        {
            int xx = Math.Abs(current % SegmentCountX - end % SegmentCountX);
            int yy = Math.Abs(current / SegmentCountX - end / SegmentCountX);

            int diagonal = Math.Min(xx, yy);
            int straight = xx + yy;

            return 1.4142135F * diagonal + (straight - 2 * diagonal);
        }

        public IAsyncResult QueryPathTaskAsync(Vector3 start, Vector3 end, float radius, IList<Vector3> wayPoints)
        {
            throw new NotImplementedException();
        }
    }
}

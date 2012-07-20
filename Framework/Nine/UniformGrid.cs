namespace Nine
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Basic 2D Space partition using uniform grids.

    /// </summary>
    [NotContentSerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UniformGrid
    {
        /// <summary>
        /// Gets or sets the number of columns (x) of the grid.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int SegmentCountX { get; internal protected set; }

        /// <summary>
        /// Gets or sets the number of rows (y) of the grid.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int SegmentCountY { get; internal protected set; }

        /// <summary>
        /// Gets or sets the top left position of the grid.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector2 Position { get; internal protected set; }

        /// <summary>
        /// Gets the width and height of the grid.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector2 Size { get; internal protected set; }

        int scaleX, scaleY;
        Point previousPoint;
        private Predicate<Point> result;
        private Predicate<Point> cachedTraversePointDelegate;
        private Predicate<Point> cachedTraverseVectorDelegate;

        internal UniformGrid()
        {
            cachedTraversePointDelegate = new Predicate<Point>(TraversePoint);
            cachedTraverseVectorDelegate = new Predicate<Point>(TraverseVector);
        }

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        public UniformGrid(float x, float y, float width, float height, int countX, int countY) : this()
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");
            if (countX <= 0)
                throw new ArgumentOutOfRangeException("countX");
            if (countY <= 0)
                throw new ArgumentOutOfRangeException("countY");

            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            SegmentCountX = countX;
            SegmentCountY = countY;
        }

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        public UniformGrid(float x, float y, float step, int countX, int countY) : this()
        {
            if (step <= 0)
                throw new ArgumentOutOfRangeException("step");
            if (countX <= 0)
                throw new ArgumentOutOfRangeException("countX");
            if (countY <= 0)
                throw new ArgumentOutOfRangeException("countY");

            Position = new Vector2(x, y);
            Size = new Vector2(step * countX, step * countY);
            SegmentCountX = countX;
            SegmentCountY = countY;
        }

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        public UniformGrid(BoundingRectangle bounds, int countX, int countY) : this()
        {
            if (countX <= 0)
                throw new ArgumentOutOfRangeException("countX");
            if (countY <= 0)
                throw new ArgumentOutOfRangeException("countY");

            SegmentCountX = countX;
            SegmentCountY = countY;

            Position = new Vector2(Math.Min(bounds.Max.X, bounds.Min.X),
                                   Math.Min(bounds.Max.Y, bounds.Min.Y));

            Size = new Vector2(Math.Abs(bounds.Max.X - bounds.Min.X),
                               Math.Abs(bounds.Max.Y - bounds.Min.Y));

            if (Size.X <= 0 || Size.Y <= 0)
                throw new ArgumentOutOfRangeException("bounds");
        }

        /// <summary>
        /// Gets whether the grid contains the specified position.
        /// </summary>
        public bool Contains(float x, float y)
        {
            return x >= Position.X && y >= Position.Y &&
                   x <= Position.X + Size.X && y <= Position.Y + Size.Y;
        }

        /// <summary>
        /// Gets whether the grid contains the specified position.
        /// </summary>
        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Gets whether the grid contains the specified index.
        /// </summary>
        public bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < SegmentCountX && y < SegmentCountY;
        }

        /// <summary>
        /// Clamps positions into the boundary.
        /// </summary>
        public Vector2 Clamp(float x, float y)
        {
            if (x < Position.X)
                x = Position.X;
            if (x > Position.X + Size.X)
                x = Position.X + Size.X;

            if (y < Position.Y)
                y = Position.Y;
            if (y > Position.Y + Size.Y)
                y = Position.Y + Size.Y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Clamps positions into the boundary.
        /// </summary>
        public Vector2 Clamp(Vector2 point)
        {
            return Clamp(point.X, point.Y);
        }

        /// <summary>
        /// Converts from world space to integral grid space.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point PositionToSegment(float x, float y)
        {
            Point pt = new Point();

            pt.X = (int)((x - Position.X) * SegmentCountX / Size.X);
            pt.Y = (int)((y - Position.Y) * SegmentCountY / Size.Y);

            if (pt.X < 0)
                pt.X = 0;
            if (pt.X >= SegmentCountX)
                pt.X = SegmentCountX - 1;

            if (pt.Y < 0)
                pt.Y = 0;
            if (pt.Y >= SegmentCountY)
                pt.Y = SegmentCountY - 1;

            return pt;
        }

        /// <summary>
        /// Converts from world space to integral grid space.
        /// </summary>
        public Point PositionToSegment(Vector2 point)
        {
            return PositionToSegment(point.X, point.Y);
        }

        /// <summary>
        /// Gets the center position of the specified integral grid.
        /// </summary>
        public Vector2 SegmentToPosition(int x, int y)
        {
            if (!Contains(x, y))
                throw new ArgumentOutOfRangeException();

            Vector2 v = new Vector2();

            v.X = (x + 0.5f) * Size.X / SegmentCountX;
            v.Y = (y + 0.5f) * Size.Y / SegmentCountY;

            return v + Position;
        }

        /// <summary>
        /// Gets the center position of the specified integral grid.
        /// </summary>
        public Vector2 SegmentToPosition(Point pt)
        {
            return SegmentToPosition(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets the bounding rectangle of the specified integral grid.
        /// </summary>
        public BoundingRectangle GetSegmentBounds(int x, int y)
        {
            if (!Contains(x, y))
                throw new ArgumentOutOfRangeException();

            Vector2 min = new Vector2();
            Vector2 max = new Vector2();

            min.X = (x) * Size.X / SegmentCountX;
            min.Y = (y) * Size.Y / SegmentCountY;
            max.X = (x + 1) * Size.X / SegmentCountX;
            max.Y = (y + 1) * Size.Y / SegmentCountY;

            return new BoundingRectangle(min + Position, max + Position);
        }

        /// <summary>
        /// Returns an enumeration of grids overlapping the specified bounds.
        /// </summary>
        public void Traverse(Vector3 position, Vector3 size, Predicate<Point> result)
        {
            Point min = PositionToSegment(Clamp(position.X - size.X / 2, position.Y - size.Y / 2));
            Point max = PositionToSegment(Clamp(position.X + size.X / 2, position.Y + size.Y / 2));

            for (int y = min.Y; y <= max.Y; y++)
                for (int x = min.X; x <= max.X; x++)
                    if (!result(new Point(x, y)))
                        return;
        }

        /// <summary>
        /// Returns an enumeration of grids overlapping the specified bounds.
        /// </summary>
        public void Traverse(BoundingBox boundingBox, Predicate<Point> result)
        {
            Traverse(boundingBox.GetCenter(), boundingBox.Max - boundingBox.Min, result);
        }

        /// <summary>
        /// Returns an enumeration of grids that floods from the specified position to the outside.
        /// </summary>
        public void Traverse(int x, int y, Predicate<Point> result)
        {
            if (Contains(x, y))
                if (!result(new Point(x, y)))
                    return;

            int maxRadius = Math.Max(SegmentCountX, SegmentCountY);
            for (int r = 1; r < maxRadius; r++)
            {
                Point offset = new Point();
                offset.X = offset.Y = -r;
                for (int currentDirection = 0; currentDirection < 4; currentDirection++)
                {
                    for (int i = 0; i < r * 2; i++)
                    {
                        offset.X += Directions[currentDirection].X;
                        offset.Y += Directions[currentDirection].Y;
                        if (Contains(x + offset.X, y + offset.Y))
                            if (!result(new Point(x + offset.X, y + offset.Y)))
                                return;
                    }
                }
            }
        }

        static Point[] Directions = new Point[] 
        {
            new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1),
        };

        /// <summary>
        /// Returns an enumeration of grids overlapping the specified line.
        /// </summary>
        public void Traverse(Point begin, Point end, Predicate<Point> result)
        {
            this.result = result;
            BresenhamLine(begin.X, begin.Y, end.X, end.Y, cachedTraversePointDelegate);
            this.result = null;
        }

        private bool TraversePoint(Point pt)
        {
            if (Contains(pt.X, pt.Y))
                return result(pt);
            return true;
        }

        /// <summary>
        /// Returns an enumeration of grids overlapping the specified ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="smallestPickableSize">
        /// The precision of line picking. A recommended value is half the radius of the
        /// smallest object to be picked.
        /// </param>
        public void Traverse(Ray ray, float smallestPickableSize, Predicate<Point> result)
        {
            Vector2 begin = new Vector2();
            Vector2 end = new Vector2();

            ray.Position.X -= Position.X;
            ray.Position.Y -= Position.Y;

            if (ProjectRay(ray, out begin, out end))
            {
                begin += Position;
                end += Position;

                Traverse(begin, end, smallestPickableSize, result);
            }
        }
        
        /// <summary>
        /// Returns an enumeration of grids overlapping the specified line.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="smallestPickableSize">
        /// The precision of line picking. A recommended value is half the radius of the
        /// smallest object to be picked.
        /// </param>
        public void Traverse(Vector2 begin, Vector2 end, float smallestPickableSize, Predicate<Point> result)
        {
            Point ptBegin = PositionToSegment(Clamp(begin));
            Point ptEnd = PositionToSegment(Clamp(end));

            if (smallestPickableSize <= 0)
                smallestPickableSize = float.MaxValue;

            float stepX = Size.X / SegmentCountX;
            float stepY = Size.Y / SegmentCountY;
            float step = Math.Min(stepX, stepY);

            if (smallestPickableSize > step)
                smallestPickableSize = step;

            scaleX = (int)(stepX / smallestPickableSize);
            scaleY = (int)(stepY / smallestPickableSize);

            previousPoint = new Point(int.MinValue, int.MinValue);
            
            this.result = result;
            BresenhamLine(ptBegin.X * scaleX, ptBegin.Y * scaleY, ptEnd.X * scaleX, ptEnd.Y * scaleY, cachedTraverseVectorDelegate);
            this.result = null;
        }

        private bool TraverseVector(Point pt)
        {
            Point next = new Point((int)(pt.X / scaleX), (int)(pt.Y / scaleY));

            if (next != previousPoint && Contains(next.X, next.Y))
            {
                previousPoint = next;
                return result(next);
            }
            return true;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }

        private static void BresenhamLine(int x0, int y0, int x1, int y1, Predicate<Point> result)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = 0;
            int ystep;
            int y = y0;
            if (y0 < y1) ystep = 1; else ystep = -1;
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    if (!result(new Point(y, x)))
                        break;
                }
                else
                {
                    if (!result(new Point(x, y)))
                        break;
                }
                error += deltay;
                if (2 * error >= deltax)
                {
                    y += ystep;
                    error -= deltax;
                }
            }
        }

        private bool ProjectRay(Ray ray, out Vector2 v1, out Vector2 v2)
        {
            v1 = Vector2.Zero;
            v2 = Vector2.Zero;

            // Get two vertices to draw a line through the
            // heightfield.
            //
            // 1. Project the ray to XY plane
            // 2. Compute the 2 intersections of the ray and
            //    terrain bounding box (Projected)
            // 3. Find the 2 points to draw
            int i = 0;
            Vector2[] points = new Vector2[2];

            // Line equation: y = k * (x - x0) + y0
            float k = ray.Direction.Y / ray.Direction.X;
            float invK = ray.Direction.X / ray.Direction.Y;
            float r = ray.Position.Y - ray.Position.X * k;
            if (r >= 0 && r <= Size.Y)
            {
                points[i++] = new Vector2(0, r);
            }
            r = ray.Position.Y + (Size.X - ray.Position.X) * k;
            if (r >= 0 && r <= Size.Y)
            {
                points[i++] = new Vector2(Size.X, r);
            }
            if (i < 2)
            {
                r = ray.Position.X - ray.Position.Y * invK;
                if (r >= 0 && r <= Size.X)
                    points[i++] = new Vector2(r, 0);
            }
            if (i < 2)
            {
                r = ray.Position.X + (Size.Y - ray.Position.Y) * invK;
                if (r >= 0 && r <= Size.X)
                    points[i++] = new Vector2(r, Size.Y);
            }

            if (i < 2)
                return false;

            // When ray position is inside the box, it should be one
            // of the starting point
            bool inside = ray.Position.X > 0 && ray.Position.X < Size.X &&
                          ray.Position.Y > 0 && ray.Position.Y < Size.Y;

            Vector2 rayPosition = new Vector2(ray.Position.X, ray.Position.Y);

            // Sort the 2 points to make the line follow the direction
            if (ray.Direction.X > 0)
            {
                if (points[0].X < points[1].X)
                {
                    v2 = points[1];
                    v1 = inside ? rayPosition : points[0];
                }
                else
                {
                    v2 = points[0];
                    v1 = inside ? rayPosition : points[1];
                }
            }
            else if (ray.Direction.X < 0)
            {
                if (points[0].X > points[1].X)
                {
                    v2 = points[1];
                    v1 = inside ? rayPosition : points[0];
                }
                else
                {
                    v2 = points[0];
                    v1 = inside ? rayPosition : points[1];
                }
            }

            return true;
        }
    }
}
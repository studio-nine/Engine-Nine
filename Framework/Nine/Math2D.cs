namespace Nine
{
    using System;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// Helper class for 2D math and geometries
    /// 
    /// Thanks for the code from Mat Buckland (fup@ai-junkie.com)
    /// </summary>
    internal static class Math2D
    {
        /// <summary>
        /// Default epsilion used all over Math2D
        /// </summary>
        public const float Epsilon = float.Epsilon;



        /// <summary>
        /// Test to see if two float equals using epslion

        /// </summary>
        public static bool FloatEquals(float n1, float n2)
        {
            return ((n1 - n2) < Epsilon) && ((n2 - n1) < Epsilon);
        }

        public static Vector2 Rotate90DegreesCcw(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 Rotate90DegreesCw(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }


        /// <summary>
        /// Test to see if a float equals zero using epslion

        /// </summary>
        public static bool FloatEqualsZero(float n)
        {
            return (n < Epsilon) && (n > -Epsilon);
        }



        /// <summary>
        /// Transform a world point p to local space specified by position and rotation
        /// </summary>
        /// <returns></returns>
        public static Vector2 WorldToLocal(Vector2 p, Vector2 translation, float rotation)
        {
            Vector2 v = new Vector2();

            // Apply translation
            p -= translation;

            if (rotation != 0)
            {
                // Apply rotation
                float sin = (float)Math.Sin(-rotation);
                float cos = (float)Math.Cos(-rotation);

                v.X = p.X * cos - p.Y * sin;
                v.Y = p.X * sin + p.Y * cos;
            }
            else
            {
                v.X = p.X;
                v.Y = p.Y;
            }

            return v;
        }



        /// <summary>
        /// Transform a local point p to world space specified by position and rotation
        /// </summary>
        /// <returns></returns>
        public static Vector2 LocalToWorld(Vector2 p, Vector2 translation, float rotation)
        {
            Vector2 v = new Vector2();

            if (rotation != 0)
            {
                // Apply rotation
                float sin = (float)Math.Sin(rotation);
                float cos = (float)Math.Cos(rotation);

                v.X = p.X * cos - p.Y * sin;
                v.Y = p.X * sin + p.Y * cos;
            }
            else
            {
                v.X = p.X;
                v.Y = p.Y;
            }

            // Apply translation
            return v + translation;
        }



        /// <summary>
        /// given a plane and a ray this function determines how far along the ray 
        /// an intersection occurs. Returns null if the ray is parallel
        /// </summary>
        public static float? RayPlaneIntersects(
            Vector2 rayOrigin, Vector2 rayDirection, Vector2 planePoint, Vector2 planeNormal)
        {
            float d = Vector2.Dot(planeNormal, planePoint);
            float numer = Vector2.Dot(planeNormal, rayOrigin) - d;
            float denom = Vector2.Dot(planeNormal, rayDirection);

            // Normal is parallel to vector
            if (FloatEqualsZero(denom))
                return null;

            return -(numer / denom);
        }



        /// <summary>
        /// Span relation
        /// </summary>
        public enum SpanType
        {
            Local, Front, Back
        }



        /// <summary>
        /// Gets the relation between a point and a plane
        /// </summary>
        public static SpanType PointLineRelation(
            Vector2 point, Vector2 planePoint, Vector2 planeNormal)
        {
            float d = Vector2.Dot(planeNormal, planePoint - point);

            if (d < -Epsilon)
                return SpanType.Front;

            if (d > Epsilon)
                return SpanType.Back;

            return SpanType.Local;
        }



        /// <summary>
        /// Test to see if a ray intersects a circle
        /// </summary>
        public static float? RayCircleIntersectionTest(
            Vector2 rayOrigin, Vector2 rayHeading, Vector2 circle, float radius)
        {
            Vector2 toCircle = circle - rayOrigin;

            float length = toCircle.Length();
            float v = Vector2.Dot(toCircle, rayHeading);
            float d = radius * radius - (length * length - v * v);

            // No intersection, return null
            if (d < 0)
                return null;

            if (v < 0)
                return v + (float)Math.Sqrt(d);

            return v - (float)Math.Sqrt(d);
        }



        /// <summary>
        /// Whether a ray intersects a circle
        /// </summary>
        public static bool RayCircleIntersects(
            Vector2 rayOrigin, Vector2 rayHeading, Vector2 circle, float radius)
        {
            Vector2 toCircle = circle - rayOrigin;

            float length = toCircle.Length();
            float v = Vector2.Dot(toCircle, rayHeading);
            float d = radius * radius - (length * length - v * v);

            return d < 0;
        }



        /// <summary>
        /// Given a point P and a circle of radius R centered at C this function
        /// determines the two points on the circle that intersect with the 
        /// tangents from P to the circle. Returns false if P is within the circle.
        /// thanks to Dave Eberly for this one.
        /// </summary>
        public static bool GetTangentPoints(
            Vector2 C, float R, Vector2 P, ref Vector2 T1, ref Vector2 T2)
        {
            Vector2 PmC = P - C;

            float sqrLen = PmC.LengthSquared();
            float rSqr = R * R;

            // P is inside or on the circle
            if (sqrLen <= rSqr)
                return false;

            float InvSqrLen = 1.0f / sqrLen;
            float Root = (float)Math.Sqrt(Math.Abs(sqrLen - rSqr));

            T1.X = C.X + R * (R * PmC.X - PmC.Y * Root) * InvSqrLen;
            T1.Y = C.Y + R * (R * PmC.Y + PmC.X * Root) * InvSqrLen;
            T2.X = C.X + R * (R * PmC.X + PmC.Y * Root) * InvSqrLen;
            T2.Y = C.Y + R * (R * PmC.Y - PmC.X * Root) * InvSqrLen;

            return true;
        }



        /// <summary>
        /// given a line segment AB and a point P, this function returns the
        /// shortest distance between a point on AB and P.
        /// </summary>
        /// <returns></returns>
        public static float DistanceToLineSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be a
            float dotA = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);

            if (dotA <= 0)
                return Vector2.Distance(a, p);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be b
            float dotB = (p.X - b.X) * (a.X - b.X) + (p.Y - b.Y) * (a.Y - b.Y);

            if (dotB <= 0)
                return Vector2.Distance(b, p);

            //calculate the point along AB that is the closest to p
            Vector2 Point = a + ((b - a) * dotA) / (dotA + dotB);

            //calculate the distance p-Point
            return Vector2.Distance(p, Point);
        }



        /// <summary>
        /// given a line AB and a point P, this function returns the
        /// shortest distance between a line AB and P.
        /// </summary>
        /// <returns></returns>
        public static float DistanceToLine(Vector2 a, Vector2 b, Vector2 p)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be a
            float dotA = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be b
            float dotB = (p.X - b.X) * (a.X - b.X) + (p.Y - b.Y) * (a.Y - b.Y);

            //calculate the point along AB that is the closest to p
            Vector2 Point = a + ((b - a) * dotA) / (dotA + dotB);

            //calculate the distance p-Point
            return Vector2.Distance(p, Point);
        }



        /// <summary>
        /// given a line AB and a point P, this function returns the
        /// shortest distance between a line AB and P.
        /// </summary>
        /// <returns></returns>
        public static float DistanceToLineSquared(Vector2 a, Vector2 b, Vector2 p)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be a
            float dotA = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be b
            float dotB = (p.X - b.X) * (a.X - b.X) + (p.Y - b.Y) * (a.Y - b.Y);

            //calculate the point along AB that is the closest to p
            Vector2 Point = a + ((b - a) * dotA) / (dotA + dotB);

            //calculate the distance p-Point
            return Vector2.DistanceSquared(p, Point);
        }



        /// <summary>
        /// given a line segment AB and a point P, this function returns the
        /// shortest distance between a point on AB and P.
        /// N represents the vector from the closest point on AB to P.
        /// </summary>
        /// <returns></returns>
        public static float DistanceToLineSegment(Vector2 a, Vector2 b, Vector2 p, out Vector2 n)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be a
            float dotA = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);

            if (dotA <= 0)
            {
                n = Vector2.Normalize(p - a);
                if (float.IsNaN(n.X))
                    n = Rotate90DegreesCcw(Vector2.Normalize(b - a));
                return Vector2.Distance(a, p);
            }

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be b
            float dotB = (p.X - b.X) * (a.X - b.X) + (p.Y - b.Y) * (a.Y - b.Y);

            if (dotB <= 0)
            {
                n = Vector2.Normalize(p - b);
                if (float.IsNaN(n.X))
                    n = Rotate90DegreesCcw(Vector2.Normalize(b - a));
                return Vector2.Distance(b, p);
            }

            //calculate the point along AB that is the closest to p
            Vector2 Point = a + ((b - a) * dotA) / (dotA + dotB);

            //calculate the distance p-Point
            n = Vector2.Normalize(p - Point);
            if (float.IsNaN(n.X))
                n = Rotate90DegreesCcw(Vector2.Normalize(b - a));
            return Vector2.Distance(p, Point);
        }



        /// <summary>
        /// given a line segment AB and a point P, this function returns the
        /// shortest distance squared between a point on AB and P.
        /// </summary>
        /// <returns></returns>
        public static float DistanceToLineSegmentSquared(Vector2 a, Vector2 b, Vector2 p)
        {
            //if the angle is obtuse between PA and AB is obtuse then the closest
            //vertex must be a
            float dotA = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);

            if (dotA <= 0)
                return Vector2.DistanceSquared(a, p);

            //if the angle is obtuse between PB and AB is obtuse then the closest
            //vertex must be b
            float dotB = (p.X - b.X) * (a.X - b.X) + (p.Y - b.Y) * (a.Y - b.Y);

            if (dotB <= 0)
                return Vector2.DistanceSquared(b, p);

            //calculate the point along AB that is the closest to p
            Vector2 Point = a + ((b - a) * dotA) / (dotA + dotB);

            //calculate the distance p-Point
            return Vector2.DistanceSquared(p, Point);
        }


        /// <summary>
        /// Gets the nearest distance from point P to the specified rectangle
        /// </summary>
        public static float DistanceToRectangle(Vector2 min, Vector2 max,
                                                Vector2 translation, float rotation, Vector2 pWorld)
        {
            Vector2 p = WorldToLocal(pWorld, translation, rotation);

            if (p.X > max.X)
            {
                if (p.Y > max.Y)
                {
                    Vector2 edge = p - max;
                    return edge.Length();
                }

                if (p.Y < min.Y)
                {
                    Vector2 edge = new Vector2();
                    edge.X = max.X;
                    edge.Y = min.Y;
                    edge -= p;
                    return edge.Length();
                }

                return p.X - max.X;
            }

            if (p.X < min.X)
            {
                if (p.Y > max.Y)
                {
                    Vector2 edge = new Vector2();
                    edge.X = min.X;
                    edge.Y = max.Y;
                    edge -= p;
                    return edge.Length();
                }

                if (p.Y < min.Y)
                {
                    Vector2 edge = min - p;
                    return edge.Length();
                }

                return min.X - p.X;
            }

            if (p.Y > max.Y)
                return p.Y - max.Y;

            if (p.Y < min.Y)
                return min.Y - p.Y;

            // Inside the rectangle
            return 0;
        }


        /// <summary>
        /// Given 2 lines in 2D space AB, CD this returns true if an 
        /// intersection occurs.
        /// </summary>
        public static bool LineSegmentIntersects(
            Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float rTop = (a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y);
            float sTop = (a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y);

            float Bot = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);

            if (Bot == 0)//parallel
            {
                return (FloatEqualsZero(rTop) && FloatEqualsZero(sTop));
            }

            float invBot = 1.0f / Bot;
            float r = rTop * invBot;
            float s = sTop * invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                //lines intersect
                return true;
            }

            //lines do not intersect
            return false;
        }



        /// <summary>
        /// Given 2 lines in 2D space AB, CD this returns true if an 
        /// intersection occurs and sets dist to the distance the intersection
        /// occurs along AB. Also sets the 2d vector point to the point of
        /// intersection
        /// </summary>
        public static float? LineSegmentIntersectionTest(
            Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float rTop = (a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y);
            float sTop = (a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y);

            float Bot = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);

            if (Bot == 0)//parallel
            {
                if (FloatEqualsZero(rTop) && FloatEqualsZero(sTop))
                    return 0;

                return null;
            }

            float invBot = 1.0f / Bot;
            float r = rTop * invBot;
            float s = sTop * invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                //lines intersect
                return Vector2.Distance(a, b) * r;
            }

            //lines do not intersect
            return null;
        }



        /// <summary>
        /// Given 2 lines in 2D space AB, CD this returns true if an 
        /// intersection occurs and sets dist to the distance the intersection
        /// occurs along AB. Also sets the 2d vector point to the point of
        /// intersection
        /// </summary>
        public static bool LineSegmentIntersectionTest(
            Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref float distance, ref Vector2 point)
        {
            float rTop = (a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y);
            float sTop = (a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y);

            float Bot = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);

            if (Bot == 0)//parallel
            {
                if (FloatEqualsZero(rTop) && FloatEqualsZero(sTop))
                {
                    distance = 0;
                    point = a;
                    return true;
                }

                return false;
            }

            float invBot = 1.0f / Bot;
            float r = rTop * invBot;
            float s = sTop * invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                //lines intersect
                distance = Vector2.Distance(a, b) * r;
                point = a + r * (b - a);
                return true;
            }

            //lines do not intersect
            return false;
        }



        /// <summary>
        /// Tests two polygons for intersection.
        /// </summary>
        /// <remarks>This algorithm does not check for enclosure</remarks>
        public static bool PolygonIntersects(Vector2[] object1, Vector2[] object2)
        {
            // Test each line segment of object1 against each segment of object2
            for (int i = 0; i < object1.Length - 1; ++i)
                for (int j = 0; j < object2.Length - 1; j++)
                    if (LineSegmentIntersects(
                        object2[j], object2[j + 1], object1[i], object1[i + 1]))
                    {
                        return true;
                    }

            return false;
        }



        /// <summary>
        /// Tests to see if a polygon and a line segment intersects
        /// </summary>
        /// <remarks>This algorithm does not check for enclosure</remarks>
        public static bool PolygonSegmentIntersects(Vector2[] polygon, Vector2 a, Vector2 b)
        {
            for (int i = 0; i < polygon.Length - 1; ++i)
                if (LineSegmentIntersects(
                    polygon[i], polygon[i + 1], a, b))
                {
                    return true;
                }

            return false;
        }



        /// <summary>
        /// Tests to see if two circle overlaps
        /// </summary>
        /// <returns></returns>
        public static ContainmentType CircleIntersects(
            Vector2 c1, float r1, Vector2 c2, float r2)
        {
            float distBetweenCenters = Vector2.Distance(c2, c1);

            if ((distBetweenCenters < (r1 + r2)))
            {
                if ((distBetweenCenters < Math.Abs(r1 - r2)))
                    return ContainmentType.Contains;

                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }



        /// <summary>
        /// Given two circles this function calculates the intersection points
        /// of any overlap. This function assumes that the two circles overlaps.
        /// 
        /// see http://astronomy.swin.edu.au/~pbourke/geometry/2circle/
        /// </summary>
        /// <returns></returns>
        public static void CircleIntersectionPoints(
            Vector2 v1, float r1, Vector2 v2, float r2, out Vector2 p1, out Vector2 p2)
        {
            p1 = new Vector2();
            p2 = new Vector2();

            //calculate the distance between the circle centers
            double d = Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y));

            //Now calculate the distance from the center of each circle to the center
            //of the line which connects the intersection points.
            double a = (r1 - r2 + (d * d)) / (2 * d);


            //MAYBE A TEST FOR EXACT OVERLAP? 

            //calculate the point P2 which is the center of the line which 
            //connects the intersection points
            double p2X, p2Y;

            p2X = v1.X + a * (v2.X - v1.X) / d;
            p2Y = v1.Y + a * (v2.Y - v1.Y) / d;

            //calculate first point
            double h1 = Math.Sqrt((r1 * r1) - (a * a));

            p1.X = (float)(p2X - h1 * (v2.Y - v1.Y) / d);
            p1.Y = (float)(p2Y + h1 * (v2.X - v1.X) / d);


            //calculate second point
            double h2 = Math.Sqrt((r2 * r2) - (a * a));

            p2.X = (float)(p2X + h2 * (v2.Y - v1.Y) / d);
            p2.Y = (float)(p2Y - h2 * (v2.X - v1.X) / d);
        }



        /// <summary>
        /// Tests to see if a point is in a circle
        /// </summary>
        public static bool PointInCircle(Vector2 p, Vector2 c, float r)
        {
            return Vector2.DistanceSquared(p, c) < r * r;
        }



        /// <summary>
        /// Returns true if the line segemnt AB intersects with a circle at
        /// position P with radius r
        /// </summary>
        /// <returns></returns>
        public static ContainmentType LineSegmentCircleIntersects(
            Vector2 a, Vector2 b, Vector2 c, float r)
        {
            // First determine the distance from the center of the circle to
            // the line segment (working in distance squared space)
            float distToLineSq = DistanceToLineSegmentSquared(a, b, c);

            if (distToLineSq < r * r)
            {
                if (PointInCircle(a, c, r) &&
                    PointInCircle(b, c, r))
                    return ContainmentType.Contains;

                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }



        /// <summary>
        /// Given a line segment AB and a circle position and radius, this function
        /// determines if there is an intersection and stores the position of the 
        /// closest intersection in the reference IntersectionPoint.
        /// 
        /// returns null if no intersection point is found
        /// </summary>
        /// <returns></returns>
        public static Vector2? LineSegmentCircleClosestIntersectionPoint(
            Vector2 a, Vector2 b, Vector2 c, float r)
        {
            Vector2 toBNorm = Vector2.Normalize(b - a);

            //move the circle into the local space defined by the vector B-A with origin
            //at A
            Vector2 LocalPos = WorldToLocal(c, a, (float)Math.Atan2(toBNorm.Y, toBNorm.X));
            
            //if the local position + the radius is negative then the circle lays behind
            //point A so there is no intersection possible. If the local x pos minus the 
            //radius is greater than length A-B then the circle cannot intersect the 
            //line segment
            if ((LocalPos.X + r >= 0) &&
               ((LocalPos.X - r) * (LocalPos.X - r) <= Vector2.DistanceSquared(b, a)))
            {
                //if the distance from the x axis to the object's position is less
                //than its radius then there is a potential intersection.
                if (Math.Abs(LocalPos.Y) < r)
                {
                    //now to do a line/circle intersection test. The center of the 
                    //circle is represented by A, B. The intersection points are 
                    //given by the formulae x = A +/-sqrt(r^2-B^2), y=0. We only 
                    //need to look at the smallest positive value of x.
                    float aa = LocalPos.X;
                    float bb = LocalPos.Y;

                    float ip = aa - (float)Math.Sqrt(r * r - bb * bb);

                    if (ip <= 0)
                    {
                        ip = aa + (float)Math.Sqrt(r * r - bb * bb);
                    }

                    return a + toBNorm * ip;
                }
            }

            return null;
        }



        /// <summary>
        /// Tests to see if a rectangle contains a point.
        /// Note that min should be smaller than max.
        /// </summary>
        /// <returns></returns>
        public static bool PointInRectangle(
            Vector2 p, Vector2 min, Vector2 max)
        {
            return (p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y);
        }



        /// <summary>
        /// Tests to see if a rectangle contains a point. 
        /// v1 and v2 are in local space relative to position and rotation
        /// </summary>
        /// <returns></returns>
        public static bool PointInRectangle(
            Vector2 p, Vector2 min, Vector2 max, Vector2 position, float rotation)
        {
            // Transform p to local space
            Vector2 pLocal = WorldToLocal(p, position, rotation);

            return PointInRectangle(pLocal, min, max);
        }



        /// <summary>
        /// Tests to see if a rectangle and a line segment intersects.
        /// </summary>
        /// <returns></returns>
        public static ContainmentType LineSegmentRectangleIntersects(
            Vector2 a, Vector2 b, Vector2 min, Vector2 max)
        {
            Vector2 v1 = new Vector2();
            Vector2 v2 = new Vector2();

            v1.X = min.X;
            v1.Y = max.Y;
            v2.X = max.X;
            v2.Y = min.Y;

            // Test to see if the line segment intersects
            // 4 rectangle edges

            if (LineSegmentIntersects(a, b, min, v1))
                return ContainmentType.Intersects;

            if (LineSegmentIntersects(a, b, v1, max))
                return ContainmentType.Intersects;

            if (LineSegmentIntersects(a, b, max, v2))
                return ContainmentType.Intersects;

            if (LineSegmentIntersects(a, b, v2, min))
                return ContainmentType.Intersects;

            // Contains
            if (PointInRectangle(a, min, max))
                return ContainmentType.Contains;

            // No intersection
            return ContainmentType.Disjoint;
        }



        /// <summary>
        /// Returns true if two rectangles intersect.
        /// This algorithm does not check for enclosure
        /// </summary>
        /// <returns></returns>
        public static ContainmentType RectangleIntersects(
            Vector2 min1, Vector2 max1, Vector2 position1, float rotation1,
            Vector2 min2, Vector2 max2, Vector2 position2, float rotation2)
        {
            // Compute 8 vertices of the two rectangle
            Vector2[] rect1 = new Vector2[4];
            Vector2[] rect2 = new Vector2[4];

            rect1[0] = min1;
            rect1[2] = max1;
            rect1[1].X = min1.X;
            rect1[1].Y = max1.Y;
            rect1[3].X = max1.X;
            rect1[3].Y = min1.Y;

            rect2[0] = min2;
            rect2[2] = max2;
            rect2[1].X = min2.X;
            rect2[1].Y = max2.Y;
            rect2[3].X = max2.X;
            rect2[3].Y = min2.Y;

            // Transform rectangle 2 to rectangle 1 local space
            for (int i = 0; i < 4; ++i)
            {
                rect1[i] = LocalToWorld(rect1[i], position1, rotation1);
                rect2[i] = LocalToWorld(rect2[i], position2, rotation2);
            }

            // Polygon intersection test
            if (PolygonIntersects(rect1, rect2))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (PointInRectangle(rect1[i], min2, max2, position2, rotation2))
                        return ContainmentType.Intersects;

                    if (PointInRectangle(rect2[i], min1, max1, position1, rotation1))
                        return ContainmentType.Intersects;
                }

                return ContainmentType.Contains;
            }

            // No intersection
            return ContainmentType.Disjoint;
        }



        /// <summary>
        /// Returns true if a rectangle and a circle intersects.
        /// This algorithm does not check for enclosure.
        /// </summary>
        public static ContainmentType RectangleCircleIntersects(
            Vector2 min, Vector2 max, Vector2 rectanglePosition, float rotation,
            Vector2 circlePosition, float circleRadius)
        {
            // Compute 8 vertices of the two rectangle
            Vector2[] rect = new Vector2[4];

            rect[0] = min;
            rect[2] = max;
            rect[1].X = min.X;
            rect[1].Y = max.Y;
            rect[3].X = max.X;
            rect[3].Y = min.Y;

            for (int i = 0; i < 4; ++i)
            {
                // Transform to world space
                rect[i] = LocalToWorld(rect[i], rectanglePosition, rotation);
            }

            if (LineSegmentCircleIntersects(
                rect[0], rect[1], circlePosition, circleRadius) != ContainmentType.Disjoint)
                return ContainmentType.Intersects;

            if (LineSegmentCircleIntersects(
                rect[1], rect[2], circlePosition, circleRadius) != ContainmentType.Disjoint)
                return ContainmentType.Intersects;

            if (LineSegmentCircleIntersects(
                rect[2], rect[3], circlePosition, circleRadius) != ContainmentType.Disjoint)
                return ContainmentType.Intersects;

            if (LineSegmentCircleIntersects(
                rect[3], rect[0], circlePosition, circleRadius) != ContainmentType.Disjoint)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }
    }
}
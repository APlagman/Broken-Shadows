using System;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Visibility
{
    public static class VectorMath
    {

        /// <summary>
        /// Computes the intersection point of the line p1-p2 with p3-p4
        /// </summary>        
        public static Vector2 LineLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            // From http://paulbourke.net/geometry/pointlineplane/
            var s = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X))
                    / ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

            Vector2 v = new Vector2(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
            return v;
        }

        /// <summary>
        /// Returns if the point is 'left' of the line p1-p2
        /// </summary>        
        public static bool LeftOf(Vector2 p1, Vector2 p2, Vector2 point)
        {
            float slope = (p2.Y - p1.Y) / (p2.X - p1.X);
            float testX = (point.Y - p1.Y) / slope + p1.X;
            return point.X < testX;
        }

        /// <summary>
        /// Returns a slightly shortened version of the vector:
        /// p * (1 - f) + q * f
        /// </summary>        
        public static Vector2 Interpolate(Vector2 p, Vector2 q, float f)
        {
            return new Vector2(p.X * (1.0f - f) + q.X * f, p.Y * (1.0f - f) + q.Y * f);
        }

        /// <summary>
        /// Calculates whether line segments 'p1q1' and 'p2q2' intersect.
        /// </summary>
        public static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && IsOnSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && IsOnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && IsOnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && IsOnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
        }

        /// <summary>
        /// Given three colinear points, checks if point 'q' lies on the line segment 'pr'.
        /// </summary>
        /// <returns></returns>
        private static bool IsOnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.X <= Max(p.X, r.X) && q.X >= Min(p.X, r.X) &&
                q.Y <= Max(p.Y, r.Y) && q.Y >= Min(p.Y, r.Y))
                return true;

            return false;
        }

        private static float Max(float one, float two)
        {
            if (one > two)
                return one;
            return two;
        }

        private static float Min(float one, float two)
        {
            if (one > two)
                return one;
            return two;
        }

        /// <summary>
        /// Finds the orientation of ordered triplet (p1, p2, p3).
        /// </summary>
        /// <returns>0, if p, q, and r are colinear; 1, if the points are clockwise; 2, if the points are counterclockwise.</returns>
        private static int Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float val = (p2.Y - p1.Y) * (p3.X - p2.X) -
                      (p2.X - p1.X) * (p3.Y - p2.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }
    }
}

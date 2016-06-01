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
            return new Vector2(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
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
    }
}

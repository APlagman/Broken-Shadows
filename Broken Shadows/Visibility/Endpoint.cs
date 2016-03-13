/**
From http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
Written by Roy Triesscheijn.
*/
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Visibility
{
    /// <summary>    
    /// The end-point of a segment    
    /// </summary>
    internal class Endpoint
    {
        /// <summary>
        /// Position of the segment
        /// </summary>
        internal Vector2 Position { get; set; }

        /// <summary>
        /// If this end-point is a begin or end end-point
        /// of a segment (each segment has only one begin and one end end-point
        /// </summary>
        internal bool Begin { get; set; }

        /// <summary>
        /// The segment this end-point belongs to
        /// </summary>
        internal Segment Segment { get; set; }

        /// <summary>
        /// The angle of the end-point relative to the location of the visibility test
        /// </summary>
        internal float Angle { get; set; }

        internal Endpoint()
        {
            Position = Vector2.Zero;
            Begin = false;
            Segment = null;
            Angle = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Endpoint)
            {
                Endpoint other = (Endpoint)obj;

                return Position.Equals(other.Position) &&
                        Begin.Equals(other.Begin) &&
                        Angle.Equals(other.Angle);

                // We do not care about the segment being the same 
                // since that would create a circular reference
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() +
                    Begin.GetHashCode() +
                    Angle.GetHashCode();
        }

        public override string ToString()
        {
            return "{ p:" + Position.ToString() + "a: " + Angle + " in " + Segment.ToString() + "}";
        }
    }
}

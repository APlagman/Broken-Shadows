
using System;
/**
From http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
Written by Roy Triesscheijn.
*/
namespace Broken_Shadows.Visibility
{
    /// <summary>
    /// Represents an occluding line segment in the visibility mesh
    /// </summary>
    internal class Segment
    {
        /// <summary>
        /// First and second end-points of the segment
        /// </summary>
        internal Endpoint P1 { get; set; }
        internal Endpoint P2 { get; set; }

        internal Segment()
        {
            P1 = null;
            P2 = null;
        }

        public override bool Equals(object obj)
        {
            if (obj is Segment)
            {
                Segment other = (Segment)obj;

                return P1.Equals(other.P1) &&
                        P2.Equals(other.P2);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return P1.GetHashCode() +
                    P2.GetHashCode();
        }

        public override string ToString()
        {
            return "{" + P1.Position.ToString() + ", " + P2.Position.ToString() + "}";
        }
    }
}

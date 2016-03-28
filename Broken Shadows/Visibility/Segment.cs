
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
    public class Segment
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

        public override string ToString()
        {
            return "{" + P1.Position.ToString() + ", " + P2.Position.ToString() + "}";
        }
    }
}

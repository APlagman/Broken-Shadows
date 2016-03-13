/**
From http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
Written by Roy Triesscheijn.
*/
using System.Collections.Generic;

namespace Broken_Shadows.Visibility
{
    internal class EndpointComparer : IComparer<Endpoint>
    {
        internal EndpointComparer() { }

        // Helper: comparison function for sorting points by angle
        public int Compare(Endpoint a, Endpoint b)
        {
            // Traverse in angle order
            if (a.Angle > b.Angle) { return 1; }
            if (a.Angle < b.Angle) { return -1; }
            // But for ties we want Begin nodes before End nodes
            if (!a.Begin && b.Begin) { return 1; }
            if (a.Begin && !b.Begin) { return -1; }

            return 0;
        }
    }
}

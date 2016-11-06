using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Visibility
{
    internal class SegmentIntersectionException : Exception
    {
        public Segment A { get; private set; }
        public Segment B { get; private set; }
        public Vector2 IntersectPoint { get; private set; }

        public SegmentIntersectionException()
        {
        }

        public SegmentIntersectionException(Segment a, Segment b, Vector2 p)
        {
            A = a;
            B = b;
            IntersectPoint = p;
        }

        public SegmentIntersectionException(string message) : base(message)
        {
        }

        public SegmentIntersectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SegmentIntersectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
/**
From http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
Written by Roy Triesscheijn.
*/
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Visibility
{
    /// <summary>
    /// Class which computes a mesh that represents which regions are 
    /// visible from the origin point given a set of occluders
    /// </summary>
    public class VisibilityComputer
    {
        private static int[][] dirs = { new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, 0 }, new int[] { 0, 1 } };
        private static int[] toStep = { 3, 0, 3, 0 };

        private List<Endpoint> endpoints;
        public List<Segment> Segments { get; private set; }
        
        public Vector2 Origin { get; set; }
        public float Radius { get; set; }

        private EndpointComparer radialComparer;

        public VisibilityComputer(Vector2 origin, float radius)
        {
            Segments = new List<Segment>();
            endpoints = new List<Endpoint>();
            radialComparer = new EndpointComparer();

            Origin = origin;
            Radius = radius;
            LoadBoundaries();
        }

        /// <summary>
        /// Add a square shaped occluder
        /// </summary>        
        public void AddSquareOccluder(Vector2 position, float width, float rotation)
        {
            float x = position.X;
            float y = position.Y;

            // The distance to each corner is the half of the width times sqrt(2)
            float Radius = width * 0.5f * 1.41f;

            // Add Pi/4 to get the corners
            rotation += MathHelper.PiOver4;

            Vector2[] corners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                corners[i] = new Vector2
                    (
                        (float)Math.Cos(rotation + i * Math.PI * 0.5) * Radius + x,
                        (float)Math.Sin(rotation + i * Math.PI * 0.5) * Radius + y
                    );
            }

            AddSegment(corners[0], corners[1]);
            AddSegment(corners[1], corners[2]);
            AddSegment(corners[2], corners[3]);
            AddSegment(corners[3], corners[0]);
        }

        public void AddLevelOccludersAsSquare(Level level)
        {
            List<Segment> segments = new List<Segment>();
            Vector2 offset = StateHandler.Get().GridPos;
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (level.IsTileWall(x, y))
                    {
                        AddSquareOccluder(
                            new Vector2(x * GlobalDefines.TileSize + offset.X, y * GlobalDefines.TileSize + offset.Y),
                            GlobalDefines.TileSize,
                            0
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Based on the given level, creates and joins neighboring light-occluding objects to simplify the intensity of visibility calculation.
        /// </summary>
        /// <param name="level">The level containing occluding objects.</param>
        public void AddJoinedLevelOccluders(Level level)
        {
            List<Segment> segments = new List<Segment>();
            Vector2 offset = StateHandler.Get().GridPos;
            int w = level.Width * 2 + 2;
            int h = level.Height * 2 + 2;
            bool[] seen = new bool[w * h];
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (level.IsTileWall(x, y))
                    {
                        float centerX = x + 0.5f;
                        float centerY = y + 0.5f;
                        for (int i = 0; i < dirs.GetLength(0); i++)
                        {
                            int[] frntDir = dirs[i];
                            int[] perpDir = dirs[(i + 1) % dirs.GetLength(0)];
                            int neighborX = x + frntDir[0];
                            int neighborY = y + frntDir[1];
                            if (!level.IsTileWall(neighborX, neighborY))
                            {
                                int faceIndex = (x + 1) * 2 + frntDir[0] + ((y + 1) * 2 + frntDir[1]) * w;
                                if (!seen[faceIndex])
                                {
                                    seen[faceIndex] = true;
                                    float x1 = centerX + frntDir[0] * 0.5f + perpDir[0] * 0.5f;
                                    float y1 = centerY + frntDir[1] * 0.5f + perpDir[1] * 0.5f;
                                    float x2 = centerX + frntDir[0] * 0.5f - perpDir[0] * 0.5f;
                                    float y2 = centerY + frntDir[1] * 0.5f - perpDir[1] * 0.5f;
                                    int signX = 1;
                                    if (x1 > x2)
                                    {
                                        float t = x1;
                                        x1 = x2;
                                        x2 = t;
                                        signX = -1;
                                    }
                                    int signY = 1;
                                    if (y1 > y2)
                                    {
                                        float t = y1;
                                        y1 = y2;
                                        y2 = t;
                                        signY = -1;
                                    }
                                    int stepX = dirs[toStep[i]][0];
                                    int stepY = dirs[toStep[i]][1];
                                    int joinX = x;
                                    int joinY = y;
                                    while (true)
                                    {
                                        joinX += stepX;
                                        joinY += stepY;
                                        neighborX = joinX + frntDir[0];
                                        neighborY = joinY + frntDir[1];
                                        if (level.IsTileWall(joinX, joinY) && !level.IsTileWall(neighborX, neighborY))
                                        {
                                            x2 = joinX + 0.5f + frntDir[0] * 0.5f - perpDir[0] * signX * 0.5f;
                                            y2 = joinY + 0.5f + frntDir[1] * 0.5f - perpDir[1] * signY * 0.5f;
                                            int index = (joinX + 1) * 2 + frntDir[0] + ((joinY + 1) * 2 + frntDir[1]) * w;
                                            seen[index] = true;
                                        }
                                        else {
                                            break;
                                        }
                                    }
                                    AddSegment(new Vector2(x1 * GlobalDefines.TileSize + offset.X - GlobalDefines.TileSize / 2, 
                                                           y1 * GlobalDefines.TileSize + offset.Y - GlobalDefines.TileSize / 2), 
                                               new Vector2(x2 * GlobalDefines.TileSize + offset.X - GlobalDefines.TileSize / 2, 
                                                           y2 * GlobalDefines.TileSize + offset.Y - GlobalDefines.TileSize / 2));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a line shaped occluder
        /// </summary>        
        public void AddLineOccluder(Vector2 p1, Vector2 p2)
        {
            AddSegment(p1, p2);
        }

        // Add a segment, where the first point shows up in the
        // visualization but the second one does not. (Every endpoint is
        // part of two segments, but we want to only show them once.)
        private void AddSegment(Vector2 p1, Vector2 p2)
        {
            Segment segment = new Segment();
            Endpoint endPoint1 = new Endpoint();
            Endpoint endPoint2 = new Endpoint();

            endPoint1.Position = p1;
            endPoint1.Segment = segment;

            endPoint2.Position = p2;
            endPoint2.Segment = segment;

            segment.P1 = endPoint1;
            segment.P2 = endPoint2;

            Segments.Add(segment);
            endpoints.Add(endPoint1);
            endpoints.Add(endPoint2);
        }

        /// <summary>
        /// Remove all occluders
        /// </summary>
        public void ClearOccluders()
        {
            Segments.Clear();
            endpoints.Clear();

            LoadBoundaries();
        }

        /// <summary>
        /// Helper function to construct segments along the outside perimiter
        /// in order to limit the Radius of the light
        /// </summary>        
        private void LoadBoundaries()
        {
            AddSquareOccluder(Origin, Radius * 2, 0);
        }

        // Processes segments so that we can sort them later
        private void UpdateSegments()
        {
            foreach (Segment segment in Segments)
            {
                // NOTE: future optimization: we could record the quadrant
                // and the y/x or x/y ratio, and sort by (quadrant,
                // ratio), instead of calling atan2. See
                // <https://github.com/mikolalysenko/compare-slope> for a
                // library that does this.

                segment.P1.Angle = (float)Math.Atan2(segment.P1.Position.Y - Origin.Y,
                                                        segment.P1.Position.X - Origin.X);
                segment.P2.Angle = (float)Math.Atan2(segment.P2.Position.Y - Origin.Y,
                                                        segment.P2.Position.X - Origin.X);

                // Map angle between -Pi and Pi
                float dAngle = segment.P2.Angle - segment.P1.Angle;
                if (dAngle <= -MathHelper.Pi) { dAngle += MathHelper.TwoPi; }
                if (dAngle > MathHelper.Pi) { dAngle -= MathHelper.TwoPi; }

                segment.P1.Begin = (dAngle > 0.0f);
                segment.P2.Begin = !segment.P1.Begin;
            }
        }

        // Helper: do we know that segment a is in front of b?
        // Implementation not anti-symmetric (that is to say,
        // _segment_in_front_of(a, b) != (!_segment_in_front_of(b, a)).
        // Also note that it only has to work in a restricted set of cases
        // in the visibility algorithm; I don't think it handles all
        // cases. See http://www.redblobgames.com/articles/visibility/segment-sorting.html
        private bool SegmentInFrontOf(Segment a, Segment b, Vector2 relativeTo)
        {
            // NOTE: we slightly shorten the segments so that
            // intersections of the endpoints (common) don't count as
            // intersections in this algorithm
            bool a1 = VectorMath.LeftOf(a.P2.Position, a.P1.Position, VectorMath.Interpolate(b.P1.Position, b.P2.Position, 0.1f));
            bool a2 = VectorMath.LeftOf(a.P2.Position, a.P1.Position, VectorMath.Interpolate(b.P2.Position, b.P1.Position, 0.1f));
            bool a3 = VectorMath.LeftOf(a.P2.Position, a.P1.Position, relativeTo);

            bool b1 = VectorMath.LeftOf(b.P2.Position, b.P1.Position, VectorMath.Interpolate(a.P1.Position, a.P2.Position, 0.1f));
            bool b2 = VectorMath.LeftOf(b.P2.Position, b.P1.Position, VectorMath.Interpolate(a.P2.Position, a.P1.Position, 0.1f));
            bool b3 = VectorMath.LeftOf(b.P2.Position, b.P1.Position, relativeTo);

            // NOTE: this algorithm is probably worthy of a short article
            // but for now, draw it on paper to see how it works. Consider
            // the line A1-A2. If both B1 and B2 are on one side and
            // relativeTo is on the other side, then A is in between the
            // viewer and B. We can do the same with B1-B2: if A1 and A2
            // are on one side, and relativeTo is on the other side, then
            // B is in between the viewer and A.
            if (b1 == b2 && b2 != b3) return true;
            if (a1 == a2 && a2 == a3) return true;
            if (a1 == a2 && a2 != a3) return false;
            if (b1 == b2 && b2 == b3) return false;

            // If A1 != A2 and B1 != B2 then we have an intersection.
            // Expose it for the GUI to show a message. A more robust
            // implementation would split segments at intersections so
            // that part of the segment is in front and part is behind.
            if (a1 != a2 && b1 != b2)
                throw new SegmentIntersectionException(a, b, VectorMath.LineLineIntersection(a.P1.Position, a.P2.Position, b.P1.Position, b.P2.Position));
            return false;

            // NOTE: previous implementation was a.d < b.d. That's simpler
            // but trouble when the segments are of dissimilar sizes. If
            // you're on a grid and the segments are similarly sized, then
            // using distance will be a simpler and faster implementation.
        }

        private void SplitSegments(Segment a, Segment b, Vector2 intersection)
        {
            AddSegment(a.P1.Position, intersection);
            AddSegment(b.P1.Position, intersection);
            a.P1.Position = intersection;
            b.P1.Position = intersection;
        }

        /// <summary>
        /// Computes the visibility polygon and returns the vertices
        /// of the triangle fan (minus the center vertex)
        /// </summary>        
        public List<Vector2> Compute()
        {
            bool retry;
            List<Vector2> output = new List<Vector2>();
            LinkedList<Segment> open = new LinkedList<Segment>();

            do
            {
                retry = false;
                try
                {
                    output.Clear();
                    open.Clear();
                    CheckIntersections();
                    UpdateSegments();

                    endpoints.Sort(radialComparer);

                    float currentAngle = 0;

                    // At the beginning of the sweep we want to know which
                    // segments are active. The simplest way to do this is to make
                    // a pass collecting the segments, and make another pass to
                    // both collect and process them. However it would be more
                    // efficient to go through all the segments, figure out which
                    // ones intersect the initial sweep line, and then sort them.
                    for (int pass = 0; pass < 2; pass++)
                    {
                        foreach (Endpoint p in endpoints)
                        {
                            Segment currentOld = (open.Count == 0) ? null : open.First.Value;

                            if (p.Begin)
                            {
                                // Insert into the right place in the list
                                var node = open.First;
                                while (node != null && SegmentInFrontOf(p.Segment, node.Value, Origin))
                                {
                                    node = node.Next;
                                }

                                if (node == null)
                                {
                                    open.AddLast(p.Segment);
                                }
                                else
                                {
                                    open.AddBefore(node, p.Segment);
                                }
                            }
                            else
                            {
                                open.Remove(p.Segment);
                            }


                            Segment currentNew = null;
                            if (open.Count != 0)
                            {
                                currentNew = open.First.Value;
                            }

                            if (currentOld != currentNew)
                            {
                                if (pass == 1)
                                {
                                    AddTriangle(output, currentAngle, p.Angle, currentOld);

                                }
                                currentAngle = p.Angle;
                            }
                        }
                    }
                }
                catch (SegmentIntersectionException e)
                {
                    System.Diagnostics.Debug.WriteLine("Segment from " + e.A.P1.Position + " to " + e.A.P2.Position + " and segment from " + e.B.P1.Position + " to " + e.B.P2.Position +
                        " intersected at " + e.IntersectPoint);
                    SplitSegments(e.A, e.B, e.IntersectPoint);
                    retry = true;
                };
                
            } while (retry);
            
            return output;
        }

        private void AddTriangle(List<Vector2> triangles, float angle1, float angle2, Segment segment)
        {
            Vector2 p1 = Origin;
            Vector2 p2 = new Vector2(Origin.X + (float)Math.Cos(angle1), Origin.Y + (float)Math.Sin(angle1));
            Vector2 p3 = Vector2.Zero;
            Vector2 p4 = Vector2.Zero;

            if (segment != null)
            {
                // Stop the triangle at the intersecting segment
                p3.X = segment.P1.Position.X;
                p3.Y = segment.P1.Position.Y;
                p4.X = segment.P2.Position.X;
                p4.Y = segment.P2.Position.Y;
            }
            else
            {
                // Stop the triangle at a fixed distance; this probably is
                // not what we want, but it never gets used in the demo
                p3.X = Origin.X + (float)Math.Cos(angle1) * Radius * 2;
                p3.Y = Origin.Y + (float)Math.Sin(angle1) * Radius * 2;
                p4.X = Origin.X + (float)Math.Cos(angle2) * Radius * 2;
                p4.Y = Origin.Y + (float)Math.Sin(angle2) * Radius * 2;
            }

            Vector2 pBegin = VectorMath.LineLineIntersection(p3, p4, p1, p2);

            p2.X = Origin.X + (float)Math.Cos(angle2);
            p2.Y = Origin.Y + (float)Math.Sin(angle2);

            Vector2 pEnd = VectorMath.LineLineIntersection(p3, p4, p1, p2);

            triangles.Add(pBegin);
            triangles.Add(pEnd);
        }

        private void CheckIntersections()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment s1 = Segments[i];
                for (int j = 0; j < Segments.Count; j++)
                {
                    Segment s2 = Segments[j];
                    if (!s1.Equals(s2) && VectorMath.DoIntersect(s1.P1.Position, s1.P2.Position, s2.P1.Position, s2.P2.Position))
                    {
                        if (s1.P1.Position.Equals(s2.P1.Position) || s1.P1.Position.Equals(s2.P2.Position) || s1.P2.Position.Equals(s2.P1.Position) || s1.P2.Position.Equals(s2.P2.Position)) // Segments intersect at a corner.
                            continue;
                        Vector2 inter = VectorMath.LineLineIntersection(s1.P1.Position, s1.P2.Position, s2.P1.Position, s2.P2.Position);
                        //System.Diagnostics.Debug.WriteLine("Splitting " + s1.P1.Position + "-" + s1.P2.Position + " and " + s2.P1.Position + "-" + s2.P2.Position + " at " + inter);
                        SplitSegments(s1, s2, inter);
                    }
                }
            }
        }
    }
}

﻿/**
From http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
Written by Roy Triesscheijn.
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Broken_Shadows.Visibility;
using System.Collections.Generic;
using System.Linq;

namespace Broken_Shadows.Graphics
{
    public class PointLight
    {
        public bool Recalculate { get; set; }
        public VisibilityComputer Visibility { get; private set; }
        public List<Vector2> Encounters { get; private set; }
        private Effect lightEffect;

        public Vector2 Position { get; set; }
        public float Radius { get; }
        public Color Color { get; }
        public float Power { get; }

        public PointLight(Effect lightEffect, Vector2 position, float radius)
            : this(lightEffect, position, radius, Color.White, 1.0f) { }

        public PointLight(Effect lightEffect, Vector2 position, float radius, Color color)
            : this(lightEffect, position, radius, color, 1.0f) { }

        public PointLight(Effect lightEffect, Vector2 position, float radius, Color color, float power)
        {
            this.lightEffect = lightEffect;

            Position = position;
            Radius = radius;
            Color = color;
            Power = power;

            Encounters = new List<Vector2>();
            Visibility = new VisibilityComputer(Position, Radius);
        }

        public void Render(GraphicsDevice device, Level level, IEnumerable<Objects.GameObject> obstacles)
        {
            if (Power > 0)
            {
                //System.Diagnostics.Debug.WriteLine("Rendering with " + obstacles.Count() + " obstacles.");
                Visibility.Origin = Position;
                Visibility.Radius = Radius;

                if (Recalculate || Encounters.Count == 0)
                {
                    Visibility.ClearOccluders();
                    Visibility.AddJoinedLevelOccluders(level);
                    foreach (Objects.GameObject o in obstacles)
                    {
                        float width = o.Pose.Scale.X * o.Texture.Width;
                        Visibility.AddSquareOccluder(o.Pose.Position, width, o.Pose.Rotation);
                    }
                    Encounters = new List<Vector2>();
                    Encounters = Visibility.Compute();
                    Recalculate = false;
                }

                // Generate a triangle list from the encounter points
                VertexPositionTexture[] vertices;
                short[] indices;

                TriangleListFromEncounters(Encounters, out vertices, out indices);

                // Project the vertices to the screen
                ProjectVertices(vertices, device.PresentationParameters.BackBufferWidth,
                                          device.PresentationParameters.BackBufferHeight);

                // Apply the effect
                lightEffect.Parameters["lightSource"].SetValue(Position);
                lightEffect.Parameters["lightColor"].SetValue(Color.ToVector3() * Power);
                lightEffect.Parameters["lightRadius"].SetValue(Radius);
                lightEffect.Techniques[0].Passes[0].Apply();

                // Draw the light on screen, using the triangle fan from the computed
                // visibility mesh so that the light only influences the area that can be 
                // "seen" from the light's position.
                //System.Diagnostics.Debug.WriteLine("Encounters: " + Encounters.Count);
                device.DrawUserIndexedPrimitives
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3
                );
            }
        }

        public void ShiftEncounters(Vector2 toShift)
        {
            for (int e = 0; e < Encounters.Count; e++)
            {
                Encounters[e] += toShift;
            }
        }

        public void ShiftSegments(Vector2 toShift)
        {
            for (int s = 0; s < Visibility.Segments.Count; s++)
            {
                Visibility.Segments[s].P1.Position += toShift;
                Visibility.Segments[s].P2.Position += toShift;
            }
        }

        private void ProjectVertices(VertexPositionTexture[] vertices, float screenWidth, float screenHeight)
        {
            float halfScreenWidth = screenWidth / 2.0f;
            float halfScreenHeight = screenHeight / 2.0f;

            // Computes the screen coordinates from the world coordinates
            for (int i = 0; i < vertices.Length; i++)
            {
                VertexPositionTexture current = vertices[i];

                current.Position.X = (current.Position.X / halfScreenWidth) - 1.0f;
                current.Position.Y = ((screenHeight - current.Position.Y) / halfScreenHeight) - 1.0f;

                vertices[i] = current;
            }
        }

        private void TriangleListFromEncounters(List<Vector2> encounters,
            out VertexPositionTexture[] vertexArray, out short[] indexArray)
        {
            //System.Diagnostics.Debug.WriteLine(encounters.Count);
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

            // Add a vertex for the center of the mesh
            vertices.Add(new VertexPositionTexture(new Vector3(Position.X, Position.Y, 0),
                Position));

            // Add all the other encounter points as vertices
            // storing their world position as UV coordinates
            foreach (Vector2 vertex in encounters)
            {
                vertices.Add(new VertexPositionTexture(new Vector3(vertex.X, vertex.Y, 0),
                   vertex));
            }

            // Compute the indices to form triangles
            List<short> indices = new List<short>();
            for (int i = 0; i < encounters.Count; i += 2)
            {
                indices.Add(0);
                indices.Add((short)(i + 2));
                indices.Add((short)(i + 1));
            }

            vertexArray = vertices.ToArray<VertexPositionTexture>();
            indexArray = indices.ToArray<short>();
        }
    }
}

using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    /// <summary>
    /// Represents the pose of an entity in 2D space
    /// </summary>
    public class Pose2D
    {
        /// <summary>
        /// Position of the entity
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Rotation of the entity
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Scale of the entity
        /// </summary>
        public Vector2 Scale { get; set; }

        public Pose2D()
            : this(Vector2.Zero)
        { }

        public Pose2D(Vector2 position)
            : this(position, 0.0f)
        { }

        public Pose2D(Vector2 position, float rotation)
            : this(position, rotation, 1.0f)
        { }

        public Pose2D(Vector2 position, float rotation, float scale)
            : this(position, rotation, new Vector2(scale, scale))
        { }

        public Pose2D(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Pose2D(Matrix matrix)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            matrix.Decompose(out scale, out rotation, out translation);

            Position = new Vector2(translation.X, translation.Y);
            Rotation = rotation.Z;
            Scale = new Vector2(scale.X, scale.Y);
        }

        /// <summary>
        /// Calculates the matrix from the pose
        /// </summary>        
        public Matrix ToMatrix()
        {
            return Matrix.CreateScale(new Vector3(Scale.X, Scale.Y, 1)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
        }

        /// <summary>
        /// Returns a new pose with the scale and rotation of this pose
        /// and the added position of both 
        /// </summary>        
        public Pose2D AddPosition(Pose2D other)
        {
            return new Pose2D(Position + other.Position, Rotation);
        }

        /// <summary>
        /// Returns a new pose with the position and scale of this pose
        /// and the added rotation of both
        /// </summary>        
        public Pose2D AddRotation(Pose2D other)
        {
            return new Pose2D(Position, Rotation + other.Rotation);
        }

        /// <summary>
        /// Returns a new pose with the position and rotation of this pose
        /// and the added scale of both
        /// </summary>        
        public Pose2D AddScale(Pose2D other)
        {
            return new Pose2D(Position, Rotation, Scale + other.Scale);
        }

        /// <summary>
        /// Returns a new pose with all components added
        /// </summary>        
        public Pose2D AddAll(Pose2D other)
        {
            return new Pose2D(Position + other.Position,
                              Rotation + other.Rotation,
                              Scale + other.Scale);
        }

        /// <summary>
        /// Calculates the current pose of an entity that has a relative
        /// offset to this pose
        /// </summary>        
        public Pose2D RelativeOffset(Vector2 offset)
        {
            return new Pose2D(Position +
                Vector2.Transform(offset, Matrix.CreateRotationZ(Rotation)), Rotation);
        }

        /// <summary>
        /// Calculates the current pose of an entity that has a relative
        /// offset to this pose
        /// </summary>        
        public Pose2D RelativeOffset(Vector2 offsetPosition, float offsetAngle)
        {
            return new Pose2D(Position +
                Vector2.Transform(offsetPosition, Matrix.CreateRotationZ(Rotation)),
                Rotation + offsetAngle);
        }
    }
}

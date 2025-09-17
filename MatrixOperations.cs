using OpenTK.Mathematics;

namespace Windows_Engine
{
    public class MatrixOperations
    {
        // Identity matrix
        public Matrix4 Identity => Matrix4.Identity;

        // Scaling matrix
        public Matrix4 Scale(float factor) => Matrix4.CreateScale(factor);
        public Matrix4 ExampleScale => Matrix4.CreateScale(1.5f);

        // Rotation matrix (around Y-axis)
        public Matrix4 RotationY(float angleInDegrees) => Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angleInDegrees));
        public Matrix4 ExampleRotation => Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45f));

        // Matrix multiplication
        public Matrix4 Multiply(Matrix4 left, Matrix4 right) => left * right;

        // Combined transformation: scale then rotate
        public Matrix4 CombinedScaleRotate(float scaleFactor, float angleInDegrees)
        {
            var scale = Matrix4.CreateScale(scaleFactor);
            var rotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angleInDegrees));
            return scale * rotation;
        }

        // Transform a vector
        public Vector3 TransformSampleVector(Vector3 input, Matrix4 transform)
        {
            return Vector3.TransformPosition(input, transform);
        }

        // Check if a matrix is identity
        public static bool IsIdentity(Matrix4 matrix)
        {
            return matrix == Matrix4.Identity;
        }
    }
}
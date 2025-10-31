using OpenTK.Mathematics;

namespace Windows_Engine
{
    public static class MatrixOperations
    {
        // Property, not a method
        public static Matrix4 Identity => Matrix4.Identity;

        public static Matrix4 Scale(float sx, float sy, float sz) => Matrix4.CreateScale(sx, sy, sz);

        public static Matrix4 RotationX(float radians) => Matrix4.CreateRotationX(radians);

        public static Matrix4 RotationY(float radians) => Matrix4.CreateRotationY(radians);

        public static Matrix4 RotationZ(float radians) => Matrix4.CreateRotationZ(radians);

        public static Matrix4 Translate(float tx, float ty, float tz) => Matrix4.CreateTranslation(tx, ty, tz);

        public static Matrix4 Multiply(Matrix4 a, Matrix4 b) => a * b;
    }
}

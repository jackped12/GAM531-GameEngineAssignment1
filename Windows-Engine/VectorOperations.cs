using OpenTK.Mathematics;

namespace Windows_Engine
{
    public static class VectorOperations
    {
        public static Vector3 A { get; } = new(1, 2, 3);
        public static Vector3 B { get; } = new(4, 5, 6);

        public static Vector3 Add => A + B;
        public static Vector3 Subtract => A - B;
        public static float Dot => Vector3.Dot(A, B);
        public static Vector3 Cross => Vector3.Cross(A, B);
    }
}

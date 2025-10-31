using OpenTK.Mathematics;

namespace Windows_Engine
{
    public static class VectorOperations
    {
        public static Vector3 A => new Vector3(1, 2, 3);
        public static Vector3 B => new Vector3(4, 5, 6);

        public static Vector3 Add(Vector3 a, Vector3 b) => a + b;

        public static Vector3 Subtract(Vector3 a, Vector3 b) => a - b;

        public static float Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);

        public static Vector3 Cross(Vector3 a, Vector3 b) => Vector3.Cross(a, b);
    }
}

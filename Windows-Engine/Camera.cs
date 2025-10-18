using OpenTK.Mathematics;

namespace Windows_Engine
{
    public class Camera
    {
        public Vector3 Position = new Vector3(0, 2, 6);
        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;

        public float Yaw = -90f;
        public float Pitch = 0f;
        public float MouseSensitivity = 0.1f;

        public void UpdateDirection(float yawOffset, float pitchOffset)
        {
            Yaw += yawOffset * MouseSensitivity;
            Pitch -= pitchOffset * MouseSensitivity;
            Pitch = MathHelper.Clamp(Pitch, -89f, 89f);

            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Normalize();

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }

        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;
    }
}

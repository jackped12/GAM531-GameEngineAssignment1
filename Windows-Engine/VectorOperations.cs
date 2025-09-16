using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace WindowEngine
{
    public class VectorOperations
    {
        private Vector3 a = new Vector3(1, 2, 3);
        private Vector3 b = new Vector3(2, 3, 4);
        private float dot;
        private Vector3 cross;
        private Vector3 normalized;
        private int vectorVBO;
        private int vectorVAO;

        public VectorOperations()
        {
            // Calculate vector operations
            dot = Vector3.Dot(a, b);
            cross = Vector3.Cross(a, b);
            normalized = Vector3.Normalize(cross);
        }

        public void InitializeBuffers()
        {
            // Define vertices for lines representing vectors a, b, cross, and normalized
            // Each line starts at origin (0,0,0) and ends at the vector's coordinates
            // We'll scale vectors down to fit in NDC (-1 to 1 range)
            float scale = 0.2f; // Scale to make vectors visible in window
            float[] vectorVertices = new float[]
            {
                // Vector a (red): origin to a
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // Origin with RGB color
                a.X * scale, a.Y * scale, a.Z * scale, 1.0f, 0.0f, 0.0f,

                // Vector b (green): origin to b
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                b.X * scale, b.Y * scale, b.Z * scale, 0.0f, 1.0f, 0.0f,

                // Cross product (blue): origin to cross
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                cross.X * scale, cross.Y * scale, cross.Z * scale, 0.0f, 0.0f, 1.0f,

                // Normalized cross (cyan): origin to normalized
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                normalized.X * scale, normalized.Y * scale, normalized.Z * scale, 0.0f, 1.0f, 1.0f
            };

            // Generate and bind VBO for vector lines
            vectorVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vectorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vectorVertices.Length * sizeof(float), vectorVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Generate and configure VAO
            vectorVAO = GL.GenVertexArray();
            GL.BindVertexArray(vectorVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vectorVBO);
            // Position (x, y, z)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // Color (r, g, b)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Render(int shaderProgramHandle)
        {
            // Use the shader program
            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vectorVAO);
            // Draw each vector as a line (2 vertices per line, 4 lines total)
            GL.DrawArrays(PrimitiveType.Lines, 0, 8);
            GL.BindVertexArray(0);
        }

        public void Cleanup()
        {
            GL.DeleteBuffer(vectorVBO);
            GL.DeleteVertexArray(vectorVAO);
        }
    }
}
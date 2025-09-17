using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;

namespace Windows_Engine
{
    public class Game : GameWindow
    {
        private float rotationAngle = 0f;
        private int shaderProgram;
        private int vao, vbo;
        private int rotationUniform;

        // Use DllImport instead of LibraryImport -> no unsafe code required
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public Game()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = (800, 600),
                Title = "Vector & Matrix Demo"
            })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            AllocConsole(); // Attach console

            GL.ClearColor(0.2f, 0.2f, 0.3f, 1f);

            string vertexShaderSrc = """
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;
                uniform mat4 rotationMatrix;
                out vec3 vColor;
                void main()
                {
                    gl_Position = rotationMatrix * vec4(aPosition, 1.0);
                    vColor = aColor;
                }
                """;

            string fragmentShaderSrc = """
                #version 330 core
                in vec3 vColor;
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(vColor, 1.0);
                }
                """;

            // Compile shaders
            int vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vertexShaderSrc);
            GL.CompileShader(vShader);

            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fragmentShaderSrc);
            GL.CompileShader(fShader);

            // Link shader program
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vShader);
            GL.AttachShader(shaderProgram, fShader);
            GL.LinkProgram(shaderProgram);

            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);

            rotationUniform = GL.GetUniformLocation(shaderProgram, "rotationMatrix");

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            rotationAngle += (float)args.Time; // animate rotation
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            float[] vertexData =
            {
                0f,  0.5f, 0f, 1f, 0f, 0f, // top (red)
               -0.5f,-0.5f, 0f, 0f, 1f, 0f, // left (green)
                0.5f,-0.5f, 0f, 0f, 0f, 1f  // right (blue)
            };

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.UseProgram(shaderProgram);

            // Combine scale + rotation using Multiply
            var scaleMatrix = MatrixOperations.Scale(0.5f, 0.5f, 0.5f);
            var rotationMatrix = MatrixOperations.RotationZ(rotationAngle);
            var transformMatrix = MatrixOperations.Multiply(rotationMatrix, scaleMatrix);

            GL.UniformMatrix4(rotationUniform, false, ref transformMatrix);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }
    }
}

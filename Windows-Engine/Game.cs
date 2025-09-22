using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace Windows_Engine
{
    public partial class Game : GameWindow
    {
        private int gridVao, gridVbo;
        private float rotationAngle = 0f;
        private int shaderProgram;
        private int vao, vbo;
        private int mvpUniform;
        private Matrix4 projection;
        // Added position vector for movement
        private Vector3 cubePosition = Vector3.Zero;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        public Game()
            : base(GameWindowSettings.Default, new NativeWindowSettings { ClientSize = (800, 600), Title = "3D Cube Demo" })
        { }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            base.OnLoad();
            AllocConsole();
            VSync = VSyncMode.On;
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.ClearColor(0.2f, 0.2f, 0.3f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

            string glVersion = GL.GetString(StringName.Version);
            Console.WriteLine($"OpenGL Version: {glVersion}");

            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f), (float)Size.X / Size.Y,
                0.1f, 100f);

            string vertexShaderSrc = """
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;
                uniform mat4 mvp;
                out vec3 vColor;
                void main()
                {
                    gl_Position = mvp * vec4(aPosition, 1.0);
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

            int vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vertexShaderSrc);
            GL.CompileShader(vShader);
            GL.GetShader(vShader, ShaderParameter.CompileStatus, out int vStatus);
            if (vStatus != (int)All.True)
            {
                Console.WriteLine("Vertex shader compile error:");
                Console.WriteLine(GL.GetShaderInfoLog(vShader));
            }

            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fragmentShaderSrc);
            GL.CompileShader(fShader);
            GL.GetShader(fShader, ShaderParameter.CompileStatus, out int fStatus);
            if (fStatus != (int)All.True)
            {
                Console.WriteLine("Fragment shader compile error:");
                Console.WriteLine(GL.GetShaderInfoLog(fShader));
            }

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vShader);
            GL.AttachShader(shaderProgram, fShader);
            GL.LinkProgram(shaderProgram);
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus != (int)All.True)
            {
                Console.WriteLine("Program link error:");
                Console.WriteLine(GL.GetProgramInfoLog(shaderProgram));
            }
            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);

            mvpUniform = GL.GetUniformLocation(shaderProgram, "mvp");
            if (mvpUniform == -1) Console.WriteLine("Warning: 'mvp' uniform not found.");
            GL.UseProgram(shaderProgram);

            float[] vertexData = {
                // Front face (red)
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,

                // Back face (green)
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f, -0.5f, -0.5f,  0f, 1f, 0f,

                // Left face (blue)
                -0.5f, -0.5f, -0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,

                // Right face (yellow)
                 0.5f, -0.5f, -0.5f,  1f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 1f, 0f,

                // Top face (magenta)
                -0.5f,  0.5f, -0.5f,  1f, 0f, 1f,
                 0.5f,  0.5f, -0.5f,  1f, 0f, 1f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 1f,
                -0.5f,  0.5f, -0.5f,  1f, 0f, 1f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 1f,
                -0.5f,  0.5f,  0.5f,  1f, 0f, 1f,

                // Bottom face (cyan)
                -0.5f, -0.5f, -0.5f,  0f, 1f, 1f,
                 0.5f, -0.5f, -0.5f,  0f, 1f, 1f,
                 0.5f, -0.5f,  0.5f,  0f, 1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 1f, 1f,
                 0.5f, -0.5f,  0.5f,  0f, 1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 1f, 1f,
            };

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (IntPtr)0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            GL.BindVertexArray(0);
            Console.WriteLine($"VAO: {vao}, VBO: {vbo}, ShaderProgram: {shaderProgram}");
            // --- GRID AXIS SETUP START ---
            float[] gridVertices = {
        // X-axis (red)
        -5.0f, 0.0f, 0.0f,  1.0f, 0.0f, 0.0f,
         5.0f, 0.0f, 0.0f,  1.0f, 0.0f, 0.0f,

        // Y-axis (green)
         0.0f, -5.0f, 0.0f,  0.0f, 1.0f, 0.0f,
         0.0f,  5.0f, 0.0f,  0.0f, 1.0f, 0.0f,

        // Z-axis (blue)
         0.0f, 0.0f, -5.0f,  0.0f, 0.0f, 1.0f,
         0.0f, 0.0f,  5.0f,  0.0f, 0.0f, 1.0f,
    };

            gridVao = GL.GenVertexArray();
            gridVbo = GL.GenBuffer();

            GL.BindVertexArray(gridVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, gridVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, gridVertices.Length * sizeof(float), gridVertices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (IntPtr)0);
            GL.EnableVertexAttribArray(0);

            // Color attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0); // Unbind VAO
                                   // --- GRID AXIS SETUP END ---
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            rotationAngle += 0.01f;

            // Added movement logic
            float moveSpeed = 5.0f * (float)args.Time;

            if (KeyboardState.IsKeyDown(Keys.W))
            {
                cubePosition.Z -= moveSpeed;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                cubePosition.Z += moveSpeed;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                cubePosition.X -= moveSpeed;
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                cubePosition.X += moveSpeed;
            }

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgram);

            // Combine rotation and translation for the model matrix
            var model = Matrix4.CreateRotationY(rotationAngle) * Matrix4.CreateTranslation(cubePosition);

            var view = Matrix4.LookAt(new Vector3(2.5f, 2.5f, 2.5f), Vector3.Zero, Vector3.UnitY);
            var mvp = model * view * projection;

            GL.UniformMatrix4(mvpUniform, false, ref mvp);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            // --- Draw the Grid ---
            // The grid is static, so its model matrix is the identity matrix.
            var gridModel = Matrix4.Identity;
            var gridMvp = gridModel * view * projection;
            GL.UniformMatrix4(mvpUniform, false, ref gridMvp);
            GL.BindVertexArray(gridVao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 6); // 6 vertices (2 per axis)
            GL.BindVertexArray(0);

            var err = GL.GetError();
            if (err != OpenTK.Graphics.OpenGL.ErrorCode.NoError)
                Console.WriteLine($"GL Error: {err}");

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathF.PI / 4f,
                (float)e.Width / e.Height,
                0.1f, 100f);
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
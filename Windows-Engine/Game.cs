using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Windows_Engine
{
    public partial class Game : GameWindow
    {
        private Matrix4 rotation = MatrixOperations.Identity();
        private Vector3 position = Vector3.Zero;
        private float scaleFactor = 1.0f;
        private double totalTime = 0.0;
        private bool hasTranslated = false; // Flag to stop after one translation
        private int shaderProgram;
        private int vao, vbo;
        private int mvpUniform;
        private int texture;
        private int texSamplerUniform;
        private Matrix4 projection;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        public Game()
            : base(GameWindowSettings.Default, new NativeWindowSettings { ClientSize = new Vector2i(800, 600), Title = "Textured Cube" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            AllocConsole();
            VSync = VSyncMode.On;
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.ClearColor(0.2f, 0.2f, 0.3f, 1f);
            GL.Enable(EnableCap.DepthTest);
            string vertexShaderSource = """
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec2 aTexCoord;
                uniform mat4 mvp;
                out vec2 vTexCoord;
                void main()
                {
                    gl_Position = mvp * vec4(aPosition, 1.0);
                    vTexCoord = aTexCoord;
                }
                """;

            string fragmentShaderSource = """
                #version 330 core
                in vec2 vTexCoord;
                out vec4 FragColor;
                uniform sampler2D tex;
                void main()
                {
                    FragColor = texture(tex, vTexCoord);
                }
                """;

            int vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vertexShaderSource);
            GL.CompileShader(vShader);
            GL.GetShader(vShader, ShaderParameter.CompileStatus, out int vStatus);
            if (vStatus != (int)All.True)
            {
                Console.WriteLine("Vertex shader compile error:");
                Console.WriteLine(GL.GetShaderInfoLog(vShader));
            }

            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fragmentShaderSource);
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
            texSamplerUniform = GL.GetUniformLocation(shaderProgram, "tex");

            GL.UseProgram(shaderProgram);
            GL.Uniform1(texSamplerUniform, 0); // Texture unit 0

            // Vertex data: position (x,y,z) + tex coords (u,v)
            float[] vertices = {
                // Front face
                -0.5f, -0.5f,  0.5f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f,

                // Back face
                -0.5f, -0.5f, -0.5f,  1f, 0f,
                -0.5f,  0.5f, -0.5f,  1f, 1f,
                 0.5f,  0.5f, -0.5f,  0f, 1f,
                -0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f,
                 0.5f, -0.5f, -0.5f,  0f, 0f,

                // Left face
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                -0.5f,  0.5f, -0.5f,  1f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 1f,

                // Right face
                 0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                 0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                 0.5f, -0.5f,  0.5f,  0f, 1f,

                // Top face
                -0.5f,  0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 1f,
            };

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            GL.BindVertexArray(0);

            // Load texture from file or fallback gradient
            try
            {
                texture = TextureLoader.LoadTexture("Assets/texture.jpg");
                Console.WriteLine("Texture loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Texture loading failed: {ex.Message}");
                texture = TextureLoader.CreateGradientTexture(256, 256);
                Console.WriteLine("Fallback gradient texture created.");
            }

            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                (float)Size.X / Size.Y,
                0.1f, 100f);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            totalTime += args.Time;
            float deltaTime = (float)args.Time;
            float rotSpeed = 2.0f;
            float moveSpeed = 1.0f;
            float scaleSpeed = 0.5f;

            bool rotInput = false;
            bool moveInput = false;
            bool scaleInput = false;

            // Rotation controls (user)
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                var rot = MatrixOperations.RotationX(rotSpeed * deltaTime);
                rotation = MatrixOperations.Multiply(rotation, rot);
                rotInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                var rot = MatrixOperations.RotationX(-rotSpeed * deltaTime);
                rotation = MatrixOperations.Multiply(rotation, rot);
                rotInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                var rot = MatrixOperations.RotationY(rotSpeed * deltaTime);
                rotation = MatrixOperations.Multiply(rotation, rot);
                rotInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                var rot = MatrixOperations.RotationY(-rotSpeed * deltaTime);
                rotation = MatrixOperations.Multiply(rotation, rot);
                rotInput = true;
            }

            // Translation controls (user, using vector operations)
            if (KeyboardState.IsKeyDown(Keys.Up))
            {
                position += VectorOperations.Add.Normalized() * moveSpeed * deltaTime;
                moveInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.Down))
            {
                position -= VectorOperations.Add.Normalized() * moveSpeed * deltaTime;
                moveInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.Left))
            {
                position += VectorOperations.Cross.Normalized() * moveSpeed * deltaTime;
                moveInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.Right))
            {
                position -= VectorOperations.Cross.Normalized() * moveSpeed * deltaTime;
                moveInput = true;
            }

            // Scale controls (user)
            if (KeyboardState.IsKeyDown(Keys.Q))
            {
                scaleFactor += scaleSpeed * deltaTime;
                scaleInput = true;
            }
            if (KeyboardState.IsKeyDown(Keys.E))
            {
                scaleFactor -= scaleSpeed * deltaTime;
                scaleInput = true;
            }
            scaleFactor = Math.Max(0.1f, scaleFactor);

            // Auto animation if no user input
            if (!rotInput)
            {
                // Auto rotate around Y axis using MatrixOperations
                var autoRot = MatrixOperations.RotationY(0.5f * deltaTime);
                rotation = MatrixOperations.Multiply(rotation, autoRot);
            }
            if (!moveInput)
            {
                // Perform one-time translation and stop
                if (!hasTranslated)
                {
                    Vector3 direction = VectorOperations.Subtract;
                    float translationAmount = 0.05f;
                    Vector3 delta = direction * translationAmount;
                    Vector3 oldPosition = position;
                    position += delta;

                    // Log the math involved
                    Console.WriteLine("\n=== Translation Math Log ===");
                    Console.WriteLine($"Vector A: {VectorOperations.A}");
                    Console.WriteLine($"Vector B: {VectorOperations.B}");
                    Console.WriteLine($"Subtract Vector (A - B): {direction}");
                    Console.WriteLine($"Dot Product (A · B): {VectorOperations.Dot}");
                    Console.WriteLine($"Cross Product (A × B): {VectorOperations.Cross}");
                    Console.WriteLine($"Add Vector (A + B): {VectorOperations.Add}");
                    Console.WriteLine($"Translation Amount: {translationAmount}");
                    Console.WriteLine($"Delta Translation: {delta}");
                    Console.WriteLine($"Initial Position: {oldPosition}");
                    Console.WriteLine($"New Position: {position}");
                    Console.WriteLine($"Translation Matrix:\n{MatrixOperations.Translate(position.X, position.Y, position.Z)}");
                    Console.WriteLine("=== End Log ===");

                    hasTranslated = true;
                }
            }
            if (!scaleInput)
            {
                // Auto oscillate scale
                scaleFactor = 1.0f + 0.2f * MathF.Sin((float)totalTime);
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

            // Build model matrix: translate * rotation * scale
            var scaleMat = MatrixOperations.Scale(scaleFactor, scaleFactor, scaleFactor);
            var transMat = MatrixOperations.Translate(position.X, position.Y, position.Z);
            var model = MatrixOperations.Multiply(transMat, MatrixOperations.Multiply(rotation, scaleMat));

            var view = Matrix4.LookAt(new Vector3(2.5f, 2.5f, 2.5f), Vector3.Zero, Vector3.UnitY);
            var mvp = MatrixOperations.Multiply(model, MatrixOperations.Multiply(view, projection));

            GL.UniformMatrix4(mvpUniform, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine($"OpenGL Error: {error}");
            }

            SwapBuffers();
        }

        protected override void OnResize(OpenTK.Windowing.Common.ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                (float)e.Width / e.Height,
                0.1f, 100f);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
            GL.DeleteTexture(texture);
        }
    }
}
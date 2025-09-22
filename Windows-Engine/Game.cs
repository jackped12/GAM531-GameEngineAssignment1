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
        private float rotationAngle = 0f;
        private int shaderProgram;
        private int vao, vbo;
        private int mvpUniform;
        private int texture;
        private Matrix4 projection;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        public Game()
            : base(GameWindowSettings.Default, new NativeWindowSettings { Size = (800, 600), Title = "3D Cube Demo" })
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
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            string glVersion = GL.GetString(StringName.Version);
            Console.WriteLine($"OpenGL Version: {glVersion}");
            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),16f/9f,
                0.1f, 100f);
            string vertexShaderSrc = """
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;
                layout(location = 2) in vec2 aTexCoord;
                uniform mat4 mvp;
                out vec3 vColor;
                out vec2 vTexCoord;
                void main()
                {
                    gl_Position = mvp * vec4(aPosition, 1.0);
                    vColor = aColor;
                    vTexCoord = aTexCoord;
                }
                """;
            string fragmentShaderSrc = """
                #version 330 core
                in vec3 vColor;
                in vec2 vTexCoord;
                uniform sampler2D texture1;
                out vec4 FragColor;
                void main()
                {
                    FragColor = texture(texture1, vTexCoord) * vec4(vColor, 1.0);
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
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            byte[] texData = { 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 0 };
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 2, 2, 0, PixelFormat.Rgb, PixelType.UnsignedByte, texData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.UseProgram(shaderProgram);
            int texUniform = GL.GetUniformLocation(shaderProgram, "texture1");
            if (texUniform == -1) Console.WriteLine("Warning: 'texture1' uniform not found.");
            GL.Uniform1(texUniform, 0);
            float[] vertexData = {
                // Front face
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,  0f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 0f, 0f,  0f, 1f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,  1f, 1f,

                // Back face
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,  0f, 0f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f,  0f, 1f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,  1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,  1f, 1f,
                 0.5f, -0.5f, -0.5f,  0f, 1f, 0f,  1f, 0f,

                // Left face
                -0.5f, -0.5f, -0.5f,  0f, 0f, 1f,  0f, 0f,
                -0.5f,  0.5f, -0.5f,  0f, 0f, 1f,  0f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,  1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f, 1f,  0f, 0f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,  1f, 0f,

                // Right face
                 0.5f, -0.5f, -0.5f,  1f, 1f, 0f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 1f, 0f,  0f, 1f,
                 0.5f,  0.5f,  0.5f,  1f, 1f, 0f,  1f, 1f,
                 0.5f, -0.5f, -0.5f,  1f, 1f, 0f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f, 0f,  1f, 1f,
                 0.5f, -0.5f,  0.5f,  1f, 1f, 0f,  1f, 0f,

                // Top face
                -0.5f,  0.5f, -0.5f,  1f, 0f, 1f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f, 1f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 1f,  1f, 1f,
                -0.5f,  0.5f, -0.5f,  1f, 0f, 1f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 1f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  1f, 0f, 1f,  0f, 1f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,  0f, 1f, 1f,  0f, 0f,
                 0.5f, -0.5f, -0.5f,  0f, 1f, 1f,  1f, 0f,
                 0.5f, -0.5f,  0.5f,  0f, 1f, 1f,  1f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 1f, 1f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  0f, 1f, 1f,  1f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 1f, 1f,  0f, 1f,
            };
            vbo = GL.GenBuffer();
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);
            
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));
            GL.EnableVertexAttribArray(2);
            GL.BindVertexArray(0);
            Console.WriteLine($"VAO: {vao}, VBO: {vbo}, ShaderProgram: {shaderProgram}, Texture: {texture}");
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            rotationAngle += 0.01f;
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
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            var model = Matrix4.CreateRotationY(rotationAngle);
            var view = Matrix4.LookAt(new Vector3(2.5f, 2.5f, 2.5f), Vector3.Zero, Vector3.UnitY);
            var mvp = model * view * projection;
        
            GL.UniformMatrix4(mvpUniform, false, ref mvp); 
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
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
            GL.DeleteTexture(texture);
        }
    }
}
// Game.cs
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Windows_Engine
{
    public class Game : GameWindow
    {
        // Transforms
        private Matrix4 rotation = MatrixOperations.Identity;   // property, not a method
        private Vector3 position = Vector3.Zero;
        private float scaleFactor = 1.0f;

        // GL objects
        private int shaderProgram;
        private int vao, vbo;

        // Uniforms
        private int uModel, uView, uProj;
        private int uLightPos, uLightColor, uLightIntensity;
        private int uViewPos;
        private int uMatAmbient, uMatDiffuse, uMatSpecular, uMatShininess;

        // Camera
        private Vector3 camPos = new Vector3(0, 0, 3);
        private Vector3 camFront = -Vector3.UnitZ;
        private Vector3 camUp = Vector3.UnitY;
        private float yaw = -90f;
        private float pitch = 0f;
        private bool firstMouse = true;
        private Vector2 lastMouse;
        private bool mouseLook = false;

        // Projection
        private Matrix4 projection;

        public Game()
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Windows_Engine - Phong Lighting"
            })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // Shaders
            string vertexSrc = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec3 vFragPos;
out vec3 vNormal;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    vFragPos = worldPos.xyz;

    // Transform normals by inverse-transpose of model
    mat3 normalMat = mat3(transpose(inverse(uModel)));
    vNormal = normalize(normalMat * aNormal);

    gl_Position = uProj * uView * worldPos;
}";

            string fragmentSrc = @"
#version 330 core
in vec3 vFragPos;
in vec3 vNormal;

out vec4 FragColor;

uniform vec3 uViewPos;

uniform vec3 uLightPos;
uniform vec3 uLightColor;
uniform float uLightIntensity;

uniform vec3 uMatAmbient;
uniform vec3 uMatDiffuse;
uniform vec3 uMatSpecular;
uniform float uMatShininess;

void main()
{
    // Ambient
    vec3 ambient = uMatAmbient * uLightColor * uLightIntensity;

    // Diffuse
    vec3 N = normalize(vNormal);
    vec3 L = normalize(uLightPos - vFragPos);
    float diff = max(dot(N, L), 0.0);
    vec3 diffuse = uMatDiffuse * diff * uLightColor * uLightIntensity;

    // Specular (Blinn-Phong)
    vec3 V = normalize(uViewPos - vFragPos);
    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), uMatShininess);
    vec3 specular = uMatSpecular * spec * uLightColor * uLightIntensity;

    FragColor = vec4(ambient + diffuse + specular, 1.0);
}";

            int vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vertexSrc);
            GL.CompileShader(vShader);
            GL.GetShader(vShader, ShaderParameter.CompileStatus, out int vok);
            if (vok != (int)All.True)
                throw new Exception("Vertex shader error: " + GL.GetShaderInfoLog(vShader));

            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fragmentSrc);
            GL.CompileShader(fShader);
            GL.GetShader(fShader, ShaderParameter.CompileStatus, out int fok);
            if (fok != (int)All.True)
                throw new Exception("Fragment shader error: " + GL.GetShaderInfoLog(fShader));

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vShader);
            GL.AttachShader(shaderProgram, fShader);
            GL.LinkProgram(shaderProgram);
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int linked);
            if (linked != (int)All.True)
                throw new Exception("Program link error: " + GL.GetProgramInfoLog(shaderProgram));
            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);

            // Uniforms
            uModel = GL.GetUniformLocation(shaderProgram, "uModel");
            uView = GL.GetUniformLocation(shaderProgram, "uView");
            uProj = GL.GetUniformLocation(shaderProgram, "uProj");
            uLightPos = GL.GetUniformLocation(shaderProgram, "uLightPos");
            uLightColor = GL.GetUniformLocation(shaderProgram, "uLightColor");
            uLightIntensity = GL.GetUniformLocation(shaderProgram, "uLightIntensity");
            uViewPos = GL.GetUniformLocation(shaderProgram, "uViewPos");
            uMatAmbient = GL.GetUniformLocation(shaderProgram, "uMatAmbient");
            uMatDiffuse = GL.GetUniformLocation(shaderProgram, "uMatDiffuse");
            uMatSpecular = GL.GetUniformLocation(shaderProgram, "uMatSpecular");
            uMatShininess = GL.GetUniformLocation(shaderProgram, "uMatShininess");

            // Cube vertices with per-face normals
            float s = 0.5f;
            float[] vertices =
            {
                // pos              // normal
                -s,-s, s, 0,0,1,  s,-s, s, 0,0,1,  s, s, s, 0,0,1,
                -s,-s, s, 0,0,1,  s, s, s, 0,0,1, -s, s, s, 0,0,1,

                -s,-s,-s, 0,0,-1, -s, s,-s, 0,0,-1,  s, s,-s, 0,0,-1,
                -s,-s,-s, 0,0,-1,  s, s,-s, 0,0,-1,  s,-s,-s, 0,0,-1,

                -s,-s,-s, -1,0,0, -s,-s, s, -1,0,0, -s, s, s, -1,0,0,
                -s,-s,-s, -1,0,0, -s, s, s, -1,0,0, -s, s,-s, -1,0,0,

                 s,-s,-s, 1,0,0,   s, s,-s, 1,0,0,  s, s, s, 1,0,0,
                 s,-s,-s, 1,0,0,   s, s, s, 1,0,0,  s,-s, s, 1,0,0,

                -s, s,-s, 0,1,0,  -s, s, s, 0,1,0,  s, s, s, 0,1,0,
                -s, s,-s, 0,1,0,   s, s, s, 0,1,0,  s, s,-s, 0,1,0,

                -s,-s,-s, 0,-1,0,  s,-s,-s, 0,-1,0,  s,-s, s, 0,-1,0,
                -s,-s,-s, 0,-1,0,  s,-s, s, 0,-1,0, -s,-s, s, 0,-1,0,
            };

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.BindVertexArray(0);

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), (float)Size.X / Size.Y, 0.1f, 100f);

            // Set material and light
            GL.UseProgram(shaderProgram);
            GL.Uniform3(uMatAmbient, new Vector3(0.1f, 0.1f, 0.1f));
            GL.Uniform3(uMatDiffuse, new Vector3(0.8f, 0.4f, 0.3f));
            GL.Uniform3(uMatSpecular, new Vector3(0.8f, 0.8f, 0.8f));
            GL.Uniform1(uMatShininess, 64.0f);

            GL.Uniform3(uLightPos, new Vector3(2.0f, 2.0f, 2.0f));
            GL.Uniform3(uLightColor, new Vector3(1.0f, 1.0f, 1.0f));
            GL.Uniform1(uLightIntensity, 1.5f);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var kb = KeyboardState;
            var ms = MouseState;

            // Right mouse = look
            if (ms.IsButtonDown(MouseButton.Right))
            {
                if (!mouseLook) { mouseLook = true; firstMouse = true; }
            }
            else mouseLook = false;

            float moveSpeed = 3.0f * (float)args.Time;
            Vector3 camRight = Vector3.Normalize(Vector3.Cross(camFront, camUp));

            if (kb.IsKeyDown(Keys.W)) camPos += camFront * moveSpeed;
            if (kb.IsKeyDown(Keys.S)) camPos -= camFront * moveSpeed;
            if (kb.IsKeyDown(Keys.A)) camPos -= camRight * moveSpeed;
            if (kb.IsKeyDown(Keys.D)) camPos += camRight * moveSpeed;
            if (kb.IsKeyDown(Keys.Space)) camPos += camUp * moveSpeed;
            if (kb.IsKeyDown(Keys.LeftControl)) camPos -= camUp * moveSpeed;

            float rotSpeed = 1.5f * (float)args.Time;
            if (kb.IsKeyDown(Keys.Left)) rotation = MatrixOperations.Multiply(rotation, MatrixOperations.RotationY(rotSpeed));
            if (kb.IsKeyDown(Keys.Right)) rotation = MatrixOperations.Multiply(rotation, MatrixOperations.RotationY(-rotSpeed));
            if (kb.IsKeyDown(Keys.Up)) rotation = MatrixOperations.Multiply(rotation, MatrixOperations.RotationX(rotSpeed));
            if (kb.IsKeyDown(Keys.Down)) rotation = MatrixOperations.Multiply(rotation, MatrixOperations.RotationX(-rotSpeed));

            if (kb.IsKeyDown(Keys.Escape)) Close();

            if (mouseLook)
            {
                Vector2 cur = ms.Position;
                if (firstMouse) { lastMouse = cur; firstMouse = false; }
                float sensitivity = 0.1f;
                float xoffset = (cur.X - lastMouse.X) * sensitivity;
                float yoffset = (lastMouse.Y - cur.Y) * sensitivity;
                lastMouse = cur;

                yaw += xoffset;
                pitch += yoffset;
                pitch = Math.Clamp(pitch, -89f, 89f);

                Vector3 dir;
                dir.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
                dir.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
                dir.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
                camFront = Vector3.Normalize(dir);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            var model = MatrixOperations.Multiply(
                MatrixOperations.Translate(position.X, position.Y, position.Z),
                MatrixOperations.Multiply(rotation, MatrixOperations.Scale(scaleFactor, scaleFactor, scaleFactor))
            );
            var view = Matrix4.LookAt(camPos, camPos + camFront, camUp);

            GL.UniformMatrix4(uModel, false, ref model);
            GL.UniformMatrix4(uView, false, ref view);
            GL.UniformMatrix4(uProj, false, ref projection);
            GL.Uniform3(uViewPos, camPos);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), (float)e.Width / e.Height, 0.1f, 100f);
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

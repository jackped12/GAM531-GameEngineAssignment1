using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Windows_Engine;

namespace Windows_Engine
{
    public class Game : GameWindow
    {
        private int shaderProgram;
        private int vaoCube, vaoPyramid, vaoGround;
        private int texGround, texCube;
        private Camera camera = new Camera();
        private Interact interactable;

        private bool firstMouse = true;
        private Vector2 lastMousePos;
        private bool lightOn = true;

        private Matrix4 projection;
        private int uModel, uView, uProj, uViewPos, uLightPos, uLightColor, uLightIntensity;
        private int uMatAmbient, uMatDiffuse, uMatSpecular, uMatShininess;
        private int uTexture;

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws)
        {
            CursorState = CursorState.Grabbed;

        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
            GL.Enable(EnableCap.DepthTest);

            string vertexShader = System.IO.File.ReadAllText("Shaders/vertex.glsl");
            string fragmentShader = System.IO.File.ReadAllText("Shaders/fragment.glsl");


            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vertexShader);
            GL.CompileShader(v);
            CheckShaderCompile(v);

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fragmentShader);
            GL.CompileShader(f);
            CheckShaderCompile(f);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, v);
            GL.AttachShader(shaderProgram, f);
            GL.LinkProgram(shaderProgram);
            CheckProgramLink(shaderProgram);

            GL.DeleteShader(v);
            GL.DeleteShader(f);

            uModel = GL.GetUniformLocation(shaderProgram, "model");
            uView = GL.GetUniformLocation(shaderProgram, "view");
            uProj = GL.GetUniformLocation(shaderProgram, "projection");
            uViewPos = GL.GetUniformLocation(shaderProgram, "viewPos");
            uLightPos = GL.GetUniformLocation(shaderProgram, "lightPos");
            uLightColor = GL.GetUniformLocation(shaderProgram, "lightColor");
            uLightIntensity = GL.GetUniformLocation(shaderProgram, "lightIntensity");
            uMatAmbient = GL.GetUniformLocation(shaderProgram, "matAmbient");
            uMatDiffuse = GL.GetUniformLocation(shaderProgram, "matDiffuse");
            uMatSpecular = GL.GetUniformLocation(shaderProgram, "matSpecular");
            uMatShininess = GL.GetUniformLocation(shaderProgram, "matShininess");
            uTexture = GL.GetUniformLocation(shaderProgram, "uTexture");

            vaoCube = CreateMesh(ShapeFactory.CreateCubeTextured());
            vaoPyramid = CreateMesh(ShapeFactory.CreatePyramidTextured());
            vaoGround = CreateMesh(ShapeFactory.CreatePlaneTextured(10f));

            texCube = TextureLoader.LoadTexture("Assets/texture.png");
            texGround = TextureLoader.LoadTexture("Assets/floor.png");

            interactable = new Interact(new Vector3(2f, 0.5f, -2f));

            float aspectRatio = (float)Size.X / Size.Y;
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspectRatio, 0.1f, 100f);
        }

        private int CreateMesh(float[] vertices)
        {
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
            return vao;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            var kb = KeyboardState;
            var ms = MouseState;

            if (kb.IsKeyDown(Keys.Escape))
                Close();

            float cameraSpeed = 5f * (float)args.Time;
            Vector3 right = Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up));

            if (kb.IsKeyDown(Keys.W))
                camera.Position += camera.Front * cameraSpeed;
            if (kb.IsKeyDown(Keys.S))
                camera.Position -= camera.Front * cameraSpeed;
            if (kb.IsKeyDown(Keys.A))
                camera.Position -= right * cameraSpeed;
            if (kb.IsKeyDown(Keys.D))
                camera.Position += right * cameraSpeed;

            if (ms.IsButtonDown(MouseButton.Right))
            {
                if (firstMouse)
                {
                    lastMousePos = ms.Position;
                    firstMouse = false;
                }

                float xoffset = ms.Position.X - lastMousePos.X;
                float yoffset = ms.Position.Y - lastMousePos.Y;
                lastMousePos = ms.Position;

                camera.UpdateDirection(xoffset, yoffset);
            }
            else firstMouse = true;

            if (kb.IsKeyPressed(Keys.E))
            {
                if (interactable.TryCollect(camera.Position))
                    Console.WriteLine("Item collected!");
                else
                    lightOn = !lightOn;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgram);
            GL.Uniform3(uViewPos, camera.Position);
            GL.Uniform3(uLightPos, new Vector3(0f, 4f, 4f));
            GL.Uniform3(uLightColor, new Vector3(1f, 1f, 1f));
            GL.Uniform1(uLightIntensity, lightOn ? 2.0f : 0.0f);

            GL.Uniform3(uMatAmbient, new Vector3(0.2f));
            GL.Uniform3(uMatDiffuse, new Vector3(0.8f, 0.6f, 0.4f));
            GL.Uniform3(uMatSpecular, new Vector3(0.8f));
            GL.Uniform1(uMatShininess, 64f);

            Matrix4 view = camera.GetViewMatrix();
            GL.UniformMatrix4(uView, false, ref view);
            GL.UniformMatrix4(uProj, false, ref projection);

            // Draw ground
            GL.BindVertexArray(vaoGround);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texGround);
            GL.Uniform1(uTexture, 0);
            Matrix4 model = Matrix4.CreateTranslation(0f, -1f, 0f);
            GL.UniformMatrix4(uModel, false, ref model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // Draw cube
            GL.BindVertexArray(vaoCube);
            GL.BindTexture(TextureTarget.Texture2D, texCube);
            GL.Uniform1(uTexture, 0);
            model = Matrix4.Identity;
            GL.UniformMatrix4(uModel, false, ref model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // Draw pyramid item if not collected
            if (!interactable.IsCollected)
            {
                GL.BindVertexArray(vaoPyramid);
                GL.BindTexture(TextureTarget.Texture2D, texCube); // reuse cube tex
                GL.Uniform1(uTexture, 0);
                model = Matrix4.CreateTranslation(interactable.ItemPosition);
                GL.UniformMatrix4(uModel, false, ref model);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 18);
            }

            SwapBuffers();
        }

        private void CheckShaderCompile(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == (int)All.False)
                throw new Exception(GL.GetShaderInfoLog(shader));
        }
        private void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
            if (status == (int)All.False)
                throw new Exception(GL.GetProgramInfoLog(program));
        }
    }

    public static class ShapeFactory
    {
        // Position(x,y,z), Normal(x,y,z), TexCoord(u,v)
        public static float[] CreateCubeTextured()
        {
            float s = 0.5f;
            return new float[]
            {
                // back face
                -s,-s,-s,  0,0,-1, 0,0,
                 s,-s,-s,  0,0,-1, 1,0,
                 s, s,-s,  0,0,-1, 1,1,
                 s, s,-s,  0,0,-1, 1,1,
                -s, s,-s,  0,0,-1, 0,1,
                -s,-s,-s,  0,0,-1, 0,0,

                // front face
                -s,-s, s,  0,0,1, 0,0,
                 s,-s, s,  0,0,1, 1,0,
                 s, s, s,  0,0,1, 1,1,
                 s, s, s,  0,0,1, 1,1,
                -s, s, s,  0,0,1, 0,1,
                -s,-s, s,  0,0,1, 0,0,

                // left face
                -s,  s, s, -1,0,0, 1,0,
                -s,  s,-s, -1,0,0, 1,1,
                -s, -s,-s, -1,0,0, 0,1,
                -s, -s,-s, -1,0,0, 0,1,
                -s, -s, s, -1,0,0, 0,0,
                -s,  s, s, -1,0,0, 1,0,

                // right face
                 s,  s, s, 1,0,0, 1,0,
                 s,  s,-s, 1,0,0, 1,1,
                 s, -s,-s, 1,0,0, 0,1,
                 s, -s,-s, 1,0,0, 0,1,
                 s, -s, s, 1,0,0, 0,0,
                 s,  s, s, 1,0,0, 1,0,

                // bottom face
                -s, -s,-s, 0,-1,0, 0,1,
                 s, -s,-s, 0,-1,0, 1,1,
                 s, -s, s, 0,-1,0, 1,0,
                 s, -s, s, 0,-1,0, 1,0,
                -s, -s, s, 0,-1,0, 0,0,
                -s, -s,-s, 0,-1,0, 0,1,

                // top face
                -s,  s,-s, 0,1,0, 0,1,
                 s,  s,-s, 0,1,0, 1,1,
                 s,  s, s, 0,1,0, 1,0,
                 s,  s, s, 0,1,0, 1,0,
                -s,  s, s, 0,1,0, 0,0,
                -s,  s,-s, 0,1,0, 0,1,
            };
        }

        public static float[] CreatePyramidTextured()
        {
            return new float[]
            {
                // Base (two triangles)
                -0.5f, 0f, -0.5f, 0, -1, 0, 0, 0,
                 0.5f, 0f, -0.5f, 0, -1, 0, 1, 0,
                 0.5f, 0f,  0.5f, 0, -1, 0, 1, 1,

                 0.5f, 0f,  0.5f, 0, -1, 0, 1, 1,
                -0.5f, 0f,  0.5f, 0, -1, 0, 0, 1,
                -0.5f, 0f, -0.5f, 0, -1, 0, 0, 0,

                // Sides
                -0.5f, 0f, -0.5f, 0, 0.707f, -0.707f, 0, 0,
                 0.5f, 0f, -0.5f, 0, 0.707f, -0.707f, 1, 0,
                 0f,  0.8f,  0f, 0, 0.707f, -0.707f, 0.5f, 1,

                 0.5f, 0f, -0.5f, 0.707f, 0, -0.707f, 0, 0,
                 0.5f, 0f,  0.5f, 0.707f, 0, -0.707f, 1, 0,
                 0f,  0.8f,  0f, 0.707f, 0, -0.707f, 0.5f, 1,

                 0.5f, 0f,  0.5f, 0, 0.707f, 0.707f, 0, 0,
                -0.5f, 0f,  0.5f, 0, 0.707f, 0.707f, 1, 0,
                 0f,  0.8f,  0f, 0, 0.707f, 0.707f, 0.5f, 1,

                -0.5f, 0f,  0.5f, -0.707f, 0, 0.707f, 0, 0,
                -0.5f, 0f, -0.5f, -0.707f, 0, 0.707f, 1, 0,
                 0f,  0.8f,  0f, -0.707f, 0, 0.707f, 0.5f, 1,
            };
        }

        public static float[] CreatePlaneTextured(float size)
        {
            float s = size / 2f;
            return new float[]
            {
                -s, 0, -s, 0, 1, 0, 0, 0,
                 s, 0, -s, 0, 1, 0, 1, 0,
                 s, 0,  s, 0, 1, 0, 1, 1,

                 s, 0,  s, 0, 1, 0, 1, 1,
                -s, 0,  s, 0, 1, 0, 0, 1,
                -s, 0, -s, 0, 1, 0, 0, 0,
            };
        }
    }
}

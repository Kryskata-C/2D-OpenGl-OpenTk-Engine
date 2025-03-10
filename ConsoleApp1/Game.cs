using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using StbImageSharp;
using System.IO;
using ImGuiNET; 

namespace OpenTKGame
{
    public class Game : GameWindow
    {
        private int _vao, _vbo, ebo, _vao2, _vbo2, ebo2, _shaderProgram;
        int texture, vao3, vbo3, shaderProgram;
        float playerXOffset = 0.06f;
        private int playerVao, playerVbo, playerEbo, playerTexture;
        ImGuiController _controller;

        float[] playerVertices =
        {
            -0.03f,0.04f,0.0f,0.0f,
             0.03f,0.04f,1.0f,0.0f,
             0.03f,-0.04f,1.0f,1.0f,
            -0.03f,-0.04f,0.0f,1.0f
        };
        uint[] playerIndices = { 0, 1, 2, 2, 3, 0 };
        float[] vertices3 =
        {
            -0.5f,-0.5f,0.0f,0.0f,
             0.5f,-0.5f,1.0f,0.0f,
             0.5f,0.5f,1.0f,1.0f,
            -0.5f,0.5f,0.0f,1.0f
        };
        uint[] indices3 = { 0, 1, 2, 2, 3, 0 };
        private readonly float[] _vertices =
        {
           -0.03f,0.04f,0.0f,
            0.03f,0.04f,0.0f,
            0.03f,-0.04f,0.0f,
           -0.03f,-0.04f,0.0f
        };
        private readonly float[] _vertices2 =
        {
           -0.1f,-0.3f,0.0f,
            0.1f,-0.3f,0.0f,
            0.1f,-0.38f,0.0f,
           -0.1f,-0.38f,0.0f
        };

        private readonly uint[] indices = { 0, 1, 2, 2, 3, 0 };
        private readonly uint[] indices2 = { 0, 1, 2, 2, 3, 0 };

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            CenterWindow();
        }

        protected override void OnLoad()
        {
            Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
            base.OnLoad();
            

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            texture = LoadTexture(@"C:\Users\chris\OneDrive\Desktop\OpenTkProject\ConsoleApp1\ConsoleApp1\Textures\shotgun2.png");

            vao3 = GL.GenVertexArray();
            vbo3 = GL.GenBuffer();
            ebo2 = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            GL.BindVertexArray(0);

            _vao2 = GL.GenVertexArray();
            _vbo2 = GL.GenBuffer();
            ebo2 = GL.GenBuffer();
            GL.BindVertexArray(_vao2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo2);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices2.Length * sizeof(float), _vertices2, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo2);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices2.Length * sizeof(uint), indices2, BufferUsageHint.StaticDraw);
            GL.BindVertexArray(0);

            _shaderProgram = CreateShaderProgram("vertex.glsl", "fragment.glsl");
            GL.BindVertexArray(vao3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo3);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices3.Length * sizeof(float), vertices3, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo2);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices3.Length * sizeof(uint), indices3, BufferUsageHint.StaticDraw);
            shaderProgram = CreateShader();
            GL.UseProgram(shaderProgram);

            int posLocation = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(posLocation);
            GL.VertexAttribPointer(posLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            int texLocation = GL.GetAttribLocation(shaderProgram, "aTexCoord");

            GL.EnableVertexAttribArray(texLocation);
            GL.VertexAttribPointer(texLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            playerVao = GL.GenVertexArray();
            playerVbo = GL.GenBuffer();
            playerEbo = GL.GenBuffer();
            playerTexture = LoadTexture(@"C:\\Users\\chris\\OneDrive\\Desktop\\OpenTkProject\\ConsoleApp1\\ConsoleApp1\\Textures\\SoliderRotated.png");

            GL.BindVertexArray(playerVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, playerVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, playerVertices.Length * sizeof(float), playerVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, playerEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, playerIndices.Length * sizeof(uint), playerIndices, BufferUsageHint.StaticDraw);
            int posLocation2 = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(posLocation2);
            GL.VertexAttribPointer(posLocation2, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            int texLocation2 = GL.GetAttribLocation(shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(texLocation2);
            GL.VertexAttribPointer(texLocation2, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            bool blendWasEnabled = GL.IsEnabled(EnableCap.Blend);
            bool depthTestWasEnabled = GL.IsEnabled(EnableCap.DepthTest);
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            _controller.Update(this, (float)args.Time);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            GL.UseProgram(shaderProgram);

            GL.BindVertexArray(playerVao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, playerTexture);
            GL.DrawElements(PrimitiveType.Triangles, playerIndices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UseProgram(0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            ImGui.DockSpaceOverViewport();
            ImGui.ShowDemoWindow();
            ImGui.Begin("Debug");
            ImGui.Text("Hello, ImGui!");
            ImGui.Text($"Player VAO: {playerVao}, Texture: {playerTexture}");
            ImGui.Text($"Viewport: {viewport[0]}, {viewport[1]}, {viewport[2]}, {viewport[3]}");
            ImGui.End();

            _controller.Render();
            ImGuiController.CheckGLError("End of frame");

            if (!blendWasEnabled) GL.Disable(EnableCap.Blend);
            if (depthTestWasEnabled) GL.Enable(EnableCap.DepthTest);

            SwapBuffers();
        }



        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);
            KeyboardState k = KeyboardState;
            float playerBottom = _vertices[7];
            float playerTop = _vertices[1];
            float playerLeft = _vertices[0];
            float playerRight = _vertices[3];
            float groundTop = _vertices2[1];
            float groundBottom = _vertices2[7];
            float groundLeft = _vertices2[0];
            float groundRight = _vertices2[3];

            bool isOverlappingHorizontally = playerRight > groundLeft && playerLeft < groundRight;
            bool isOverlappingVertically = playerBottom > groundBottom && playerTop < groundTop;
            bool canMoveUp = playerTop < 1.0f && !(isOverlappingHorizontally && playerBottom < groundTop && playerTop + 0.00005f >= groundBottom);
            bool canMoveDown = playerBottom > -1.0f && !(isOverlappingHorizontally && playerTop > groundBottom && playerBottom - 0.00005f <= groundTop);
            bool canMoveLeft = playerLeft > -1.0f && !(isOverlappingVertically && playerRight > groundLeft && playerLeft - 0.00005f <= groundRight);
            bool canMoveRight = playerRight < 1.0f && !(isOverlappingVertically && playerLeft < groundRight && playerRight + 0.00005f >= groundLeft);

            MouseState mouse = MouseState;
            Vector2 mousePos = mouse.Position;
            float normalizedX = (mousePos.X / Size.X) * 2.0f - 1.0f;
            float normalizedY = 1.0f - (mousePos.Y / Size.Y) * 2.0f;
            Vector2 movement = Vector2.Zero;

            if (k.IsKeyDown(Keys.W) && canMoveUp) movement.Y += 0.00005f;
            if (k.IsKeyDown(Keys.S) && canMoveDown) movement.Y -= 0.00005f;
            if (k.IsKeyDown(Keys.A) && canMoveLeft) movement.X -= 0.00005f;
            if (k.IsKeyDown(Keys.D) && canMoveRight) movement.X += 0.00005f;
            for (int i = 0; i < _vertices.Length; i += 3)
            {
                _vertices[i] += movement.X;
                _vertices[i + 1] += movement.Y;
            }
            float playerCenterX = (_vertices[0] + _vertices[3]) / 2;
            float playerCenterY = (_vertices[1] + _vertices[4]) / 2;
            Vector2 direction = new Vector2(normalizedX - playerCenterX, normalizedY - playerCenterY);
            float playerRotation = MathF.Atan2(direction.Y, direction.X);
            float playerWidth = 0.20f;
            float playerHeight = 0.20f;
            float cosA = MathF.Cos(playerRotation);
            float sinA = MathF.Sin(playerRotation);

            playerVertices[0] = playerCenterX + (-playerWidth / 2) * cosA - (playerHeight / 2) * sinA;
            playerVertices[1] = playerCenterY + (-playerWidth / 2) * sinA + (playerHeight / 2) * cosA;
            playerVertices[4] = playerCenterX + (playerWidth / 2) * cosA - (playerHeight / 2) * sinA;
            playerVertices[5] = playerCenterY + (playerWidth / 2) * sinA + (playerHeight / 2) * cosA;
            playerVertices[8] = playerCenterX + (playerWidth / 2) * cosA - (-playerHeight / 2) * sinA;
            playerVertices[9] = playerCenterY + (playerWidth / 2) * sinA + (-playerHeight / 2) * cosA;
            playerVertices[12] = playerCenterX + (-playerWidth / 2) * cosA - (-playerHeight / 2) * sinA;
            playerVertices[13] = playerCenterY + (-playerWidth / 2) * sinA + (-playerHeight / 2) * cosA;

            float gunWidthScale = 0.18f;
            float gunHeightScale = 0.18f;
            float gunRotationScale = 1.0f;
            float gunXOffset = 0.13f;
            float gunYOffset = 0.0f;
            float halfGunWidth = 0.5f * gunWidthScale;
            float halfGunHeight = 0.5f * gunHeightScale;
            float gunOffsetX = gunXOffset * cosA - gunYOffset * sinA;
            float gunOffsetY = gunXOffset * sinA + gunYOffset * cosA;
            float gunCenterX = playerCenterX + gunOffsetX;
            float gunCenterY = playerCenterY + gunOffsetY;
            float gunCos = MathF.Cos(playerRotation * gunRotationScale);
            float gunSin = MathF.Sin(playerRotation * gunRotationScale);

            vertices3[0] = gunCenterX + (-halfGunWidth) * gunCos - (-halfGunHeight) * gunSin;
            vertices3[1] = gunCenterY + (-halfGunWidth) * gunSin + (-halfGunHeight) * gunCos;
            vertices3[4] = gunCenterX + (halfGunWidth) * gunCos - (-halfGunHeight) * gunSin;
            vertices3[5] = gunCenterY + (halfGunWidth) * gunSin + (-halfGunHeight) * gunCos;
            vertices3[8] = gunCenterX + (halfGunWidth) * gunCos - (halfGunHeight) * gunSin;
            vertices3[9] = gunCenterY + (halfGunWidth) * gunSin + (halfGunHeight) * gunCos;
            vertices3[12] = gunCenterX + (-halfGunWidth) * gunCos - (halfGunHeight) * gunSin;
            vertices3[13] = gunCenterY + (-halfGunWidth) * gunSin + (halfGunHeight) * gunCos;
            GL.BindBuffer(BufferTarget.ArrayBuffer, playerVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, playerVertices.Length * sizeof(float), playerVertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo3);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices3.Length * sizeof(float), vertices3);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(ebo);
            GL.DeleteBuffer(_vbo2);
            GL.DeleteVertexArray(_vao2);
            GL.DeleteBuffer(ebo2);
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteBuffer(playerVbo);
            GL.DeleteVertexArray(playerVao);
            GL.DeleteBuffer(playerEbo);
            GL.DeleteProgram(shaderProgram);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _controller.MouseScroll(e.Offset);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        private int CreateShaderProgram(string vertexPath, string fragmentPath)
        {
            int vertexShader = CompileShader(vertexPath, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentPath, ShaderType.FragmentShader);
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) { Console.WriteLine(GL.GetProgramInfoLog(program)); }
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        private int CompileShader(string filePath, ShaderType type)
        {
            string shaderSource = LoadShaderSource(filePath);
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) { Console.WriteLine($"Shader compilation failed ({type}): {GL.GetShaderInfoLog(shader)}"); }
            return shader;
        }

        private string LoadShaderSource(string filePath)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string fullPath = Path.Combine(projectDir, "Shaders", filePath);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Shader file not found: {fullPath}");
                return "";
            }
            return File.ReadAllText(fullPath);
        }

        int LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: Texture file not found at {path}");
                return 0;
            }
            StbImage.stbi_set_flip_vertically_on_load(1);
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                if (image == null)
                {
                    Console.WriteLine("ERROR: Failed to load texture, unknown image type.");
                    return 0;
                }
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            return id;
        }

        int CreateShader()
        {
            string vertexShaderSource = @"
        #version 330 core
        layout (location=0) in vec2 aPosition;
        layout (location=1) in vec2 aTexCoord;
        out vec2 TexCoord;
        void main()
        {
            gl_Position=vec4(aPosition,0.0,1.0);
            TexCoord=aTexCoord;
        }";
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            string fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        in vec2 TexCoord;
        uniform sampler2D texture1;
        void main()
        {
            FragColor=texture(texture1,TexCoord);
        }";
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return shaderProgram;
        }
    }
}

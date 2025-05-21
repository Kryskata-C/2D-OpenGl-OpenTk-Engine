using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using StbImageSharp;
using System.IO;
using System.Linq.Expressions;
using OpenTK_Project;
using System.ComponentModel.Design;

namespace OpenTKGame
{
    public class Game : GameWindow
    {
        private float _playerCenterX;
        private float _playerCenterY;

        // Debugging tools
        private bool lightingEnabled = true;
        private bool showCollisions = false;
        private string currentMap = "Main";
        //-----------------------------------

        //file directories path findign stuff
        private static readonly string projectDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
        );

        private float playerCollisionOffsetLeft = -0.05f;
        private float playerCollisionOffsetRight = 0.00f;
        private float playerCollisionOffsetTop = -0.01f;
        private float playerCollisionOffsetBottom = 0.010f;

        private float playerCollisionScaleX = 1f;
        private float playerCollisionScaleY = 1.7f;


        int mapWidth = (int)(2.0f / 0.12f);   
        int mapHeight = (int)(2.0f / 0.15f);  


        private int _vao, _vbo, ebo, _shaderProgram;
        private int texture, vao3, vbo3, shaderProgram;
        private int playerVao, playerVbo, playerEbo, playerTexture;
        private int backgroundVao, backgroundVbo, backgroundEbo, backgroundTexture;

        // The bounding boxes for squares
        private List<SquareData> squares = new List<SquareData>();


        //Texture Locations
        private static readonly string TexturesDir = Path.Combine(projectDir, "Textures");
        private static readonly string MapsDir = Path.Combine(projectDir, "Maps");
        private static readonly string TilesDir = Path.Combine(TexturesDir, "Dirt Tiles");


        private string boxTextureLoc = Path.Combine(TexturesDir, "box.png");
        private string GreenSquareTextureLoc = Path.Combine(TexturesDir, "GreenSquare.png");
        private string MainMap = Path.Combine(MapsDir, "MainMap.txt");
        private string Map1 = Path.Combine(MapsDir, "Map1.txt");
        private string Dirt_Main = Path.Combine(TilesDir, "Dirt_Main.png");
        private string Dirt_GrassSide = Path.Combine(TilesDir, "Dirt_GrassSide.png");
        private string Dirt_GrassBottomLeft = Path.Combine(TilesDir, "Dirt_GrassBottomLeft.png");

        private SquareData boxSquare;


        //Edit mode shit
        private int highlightedTileX = -1;
        private int highlightedTileY = -1;
        private SquareData? highlightSquare = null;
        private bool editMode = false;
        private SquareData highlightTemplate;
        private bool highlightInitialized = false;

        //What object to palce selection in edit mode 
        private string selectedEditObject = "Box"; // default
        private bool selectingObject = false;
        private string[] availableObjects = new string[] { "Box", "Dirt_GrassBottomLeft", "Dirt_GrassSide","Dirt_Main"};


        //Animations
        private List<int> idleTextures = new List<int>();
        private List<int> walkTextures = new List<int>();
        private List<int> reloadTextures = new List<int>();
        private int currentAnimFrame = 0;
        private double animTimer = 0;
        private double animSpeed = 0.08;
        private bool isReloading = false;
        private double reloadAnimSpeed = 0.06;
        private double reloadTimer = 0;
        private int reloadFrame = 0;




        // Player geometry
        float[] playerVertices =
        {
            -0.03f,  0.04f,  0.0f, 0.0f,
             0.03f,  0.04f,  1.0f, 0.0f,
             0.03f, -0.04f,  1.0f, 1.0f,
            -0.03f, -0.04f,  0.0f, 1.0f
        };
        uint[] playerIndices = { 0, 1, 2, 2, 3, 0 };

        // Background geometry
        float[] backgroundVertices =
        {
            -1.0f, -1.0f,   0.0f, 0.0f,
             1.0f, -1.0f,   1.0f, 0.0f,
             1.0f,  1.0f,   1.0f, 1.0f,
            -1.0f,  1.0f,   0.0f, 1.0f
        };
        uint[] backgroundIndices = { 0, 1, 2, 2, 3, 0 };

        // Gun geometry
        float[] vertices3 =
        {
            -0.5f, -0.5f,   0.0f, 0.0f,
             0.5f, -0.5f,   1.0f, 0.0f,
             0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f,   0.0f, 1.0f
        };
        uint[] indices3 = { 0, 1, 2, 2, 3, 0 };

       
        private readonly float[] _vertices =
        {
           -0.03f,  0.04f,  0.0f,
            0.03f,  0.04f,  0.0f,
            0.03f, -0.04f,  0.0f,
           -0.03f, -0.04f,  0.0f
        };

        //values for the map edditor
        int rows = 12;
        int cols = 16;
        float tileWidth = 0.12f;
        float tileHeight = 0.15f;

        float startX = -0.94f;
        float startY = -0.93f;
        string[][] mapArray = new string[12][];
        string[][] positionArray = new string[12][];



        List<string> tilePositions = new List<string>();


        // Debug rendering shader + VAO for bounding boxes
        private int debugShader;
        private int debugVao;
        private int debugVbo;

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            CenterWindow();
        }

        private void UpdateSquareBuffer(ref SquareData square)
        {
            float halfW = square.HalfW;
            float halfH = square.HalfH;
            float cx = square.CenterX;
            float cy = square.CenterY;

            float[] updatedVertices =
            {
                cx - halfW, cy - halfH,  0.0f, 0.0f,
                cx + halfW, cy - halfH,  1.0f, 0.0f,
                cx + halfW, cy + halfH,  1.0f, 1.0f,
                cx - halfW, cy + halfH,  0.0f, 1.0f
            };  

            GL.BindBuffer(BufferTarget.ArrayBuffer, square.Vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, updatedVertices.Length * sizeof(float), updatedVertices);
        }


        private double walkAnimSpeed = 0.05; //lower = faster
        private double idleAnimSpeed = 0.08; 

        private void LoadPlayerAnimations()
        {
            string idleDir = Path.Combine(TexturesDir, "Animation Rifle Idle");
            string walkDir = Path.Combine(TexturesDir, "Animation Rifle Walk");
            string reloadDir = Path.Combine(TexturesDir, "Animation Rifle Reload");

            for (int i = 0; i <= 19; i++)
            {
                string idlePath = Path.Combine(idleDir, $"survivor-idle_rifle_{i}.png");
                if (File.Exists(idlePath))
                {
                    idleTextures.Add(LoadTexture(idlePath));
                }

                string walkPath = Path.Combine(walkDir, $"survivor-move_rifle_{i}.png");
                if (File.Exists(walkPath))
                {
                    walkTextures.Add(LoadTexture(walkPath));
                }

                string reloadPath = Path.Combine(reloadDir, $"survivor-reload_rifle_{i}.png");
                if (File.Exists(reloadPath))
                {
                    reloadTextures.Add(LoadTexture(reloadPath));
                }
            }

            playerTexture = idleTextures[0]; 
        }


        protected override void OnLoad()
        {
            base.OnLoad();
            Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
           

            //Create + Use main shader + no lighting shader
            shaderProgram = CreateLightingShader();
            GL.UseProgram(shaderProgram);
            noLightingShaderProgram = CreateNoLightingShader();

            //Edit mode square setup
            highlightTemplate = CreateSquare(0, 0, tileWidth, tileHeight, GreenSquareTextureLoc,false);
            highlightInitialized = true;
    

            //Player Setup
            playerVao = GL.GenVertexArray();
            playerVbo = GL.GenBuffer();
            playerEbo = GL.GenBuffer();
            playerTexture = LoadTexture(
                Path.Combine(TexturesDir, "survivor-idle_rifle_0.png")
            );

            GL.BindVertexArray(playerVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, playerVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, playerVertices.Length * sizeof(float), playerVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, playerEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, playerIndices.Length * sizeof(uint), playerIndices, BufferUsageHint.StaticDraw);

            int posLocationPlayer = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(posLocationPlayer);
            GL.VertexAttribPointer(posLocationPlayer, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            int texLocationPlayer = GL.GetAttribLocation(shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(texLocationPlayer);
            GL.VertexAttribPointer(texLocationPlayer, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);


            //Animations
            LoadPlayerAnimations();

            //Background Setup
            backgroundVao = GL.GenVertexArray();
            backgroundVbo = GL.GenBuffer();
            backgroundEbo = GL.GenBuffer();
            backgroundTexture = LoadTexture(
                Path.Combine(TexturesDir, "tileable_grass.png")
            );

            GL.BindVertexArray(backgroundVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, backgroundVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, backgroundVertices.Length * sizeof(float), backgroundVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, backgroundEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, backgroundIndices.Length * sizeof(uint), backgroundIndices, BufferUsageHint.StaticDraw);

            int posLocationBg = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(posLocationBg);
            GL.VertexAttribPointer(posLocationBg, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            int texLocationBg = GL.GetAttribLocation(shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(texLocationBg);
            GL.VertexAttribPointer(texLocationBg, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);

            //Enable alpha blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            
            squares.Add(boxSquare);

            //Debug rendering shader + VAO
            debugShader = CreateDebugShader();
            debugVao = GL.GenVertexArray();
            debugVbo = GL.GenBuffer();
            GL.BindVertexArray(debugVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, debugVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 2 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);

            // Load map layout from file
            string[] lines = File.ReadAllLines(MainMap);

            for (int y = 0; y < rows; y++)
            {
                mapArray[y] = new string[cols];
                positionArray[y] = new string[cols];

                string[] tokens = lines[y].Split(' ');

                for (int x = 0; x < cols; x++)
                {
                    float worldX = startX + x * tileWidth;
                    float worldY = startY + y * tileHeight;

                    mapArray[y][x] = tokens[x]; 
                    positionArray[y][x] = $"{worldX:F2},{worldY:F2}";
                }
            }
            CreateMap();

        }


        public void CreateMap()
        {
            //Dont remove this (idk how it works but if you move the square the new possition gets collision while the old collision that used to stay there with the "invisible" square is removed)
            squares.Clear(); 

            for (int i = 0; i < positionArray.Length; i++)
            {
                for (int j = 0; j < positionArray[i].Length; j++)
                {
                    string[] pos = positionArray[i][j].Split(',');
                    float x = float.Parse(pos[0]);
                    float y = float.Parse(pos[1]);

                    if (mapArray[i][j] == "S") 
                    {
                        boxSquare = CreateSquare(x, y, 0.12f, 0.15f, boxTextureLoc,true);
                        squares.Add(boxSquare);
                    }
                }
            }
        }





        private int ebo3;

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            bool blendWasEnabled = GL.IsEnabled(EnableCap.Blend);
            bool depthTestWasEnabled = GL.IsEnabled(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);


            //Handle lighting 
            if (lightingEnabled)
            {
                GL.UseProgram(shaderProgram);
            }

            else
            {
                GL.UseProgram(noLightingShaderProgram);
            }

            // 1) Window size uniform
            int windowSizeLoc = GL.GetUniformLocation(shaderProgram, "u_WindowSize");
            GL.Uniform2(windowSizeLoc, (float)ClientSize.X, (float)ClientSize.Y);

            // 2) Player position (using the fields we set in OnUpdateFrame)
            float playerNdcX = _playerCenterX;
            float playerNdcY = _playerCenterY;
            int playerPosLoc = GL.GetUniformLocation(shaderProgram, "u_LightPos");
            GL.Uniform2(playerPosLoc, playerNdcX, playerNdcY);

            // 4) Light radius
            int radiusLoc = GL.GetUniformLocation(shaderProgram, "u_LightRadius");
            GL.Uniform1(radiusLoc, 0.7f);

            // 5) Light color
            int colorLoc = GL.GetUniformLocation(shaderProgram, "u_LightColor");
            GL.Uniform3(colorLoc, 1.0f, 1.0f, 1.0f);

            // Draw background
            GL.BindVertexArray(backgroundVao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, backgroundTexture);
            GL.DrawElements(PrimitiveType.Triangles, backgroundIndices.Length, DrawElementsType.UnsignedInt, 0);

            //Draw edit mode square
            if (editMode && highlightSquare.HasValue)
            {
                var hs = highlightSquare.Value;
                GL.BindVertexArray(hs.Vao);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, hs.Texture);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }
            else if (editMode && !highlightSquare.HasValue)
            {
                Console.WriteLine("Going into edit mode has failed");
            }

            // Draw squares
            foreach (var sq in squares)
            {
                GL.BindVertexArray(sq.Vao);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, sq.Texture);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            // Draw player
            GL.BindVertexArray(playerVao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, playerTexture);
            GL.DrawElements(PrimitiveType.Triangles, playerIndices.Length, DrawElementsType.UnsignedInt, 0);

            // Draw shotgun
            GL.BindVertexArray(vao3);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.DrawElements(PrimitiveType.Triangles, indices3.Length, DrawElementsType.UnsignedInt, 0);

            

            // Debug bounding boxes if desired
            if (showCollisions)
            {
                GL.UseProgram(debugShader);
                int debugColorLoc = GL.GetUniformLocation(debugShader, "color");
                GL.Uniform4(debugColorLoc, new Vector4(1f, 1f, 1f, 0.5f));
                // White w/ 50% alpha
                GL.Uniform4(colorLoc, new Vector4(1f, 1f, 1f, 0.5f));

                GL.BindVertexArray(debugVao);

                // Player bounding box
                (float pLeft, float pRight, float pBottom, float pTop) = GetPlayerBoundingBox();
                DrawBox(pLeft, pBottom, pRight, pTop);

                // Square bounding boxes
                foreach (var sq in squares)
                {
                    float sqLeft = sq.CenterX - sq.HalfW;
                    float sqRight = sq.CenterX + sq.HalfW;
                    float sqBottom = sq.CenterY - sq.HalfH;
                    float sqTop = sq.CenterY + sq.HalfH;
                    DrawBox(sqLeft, sqBottom, sqRight, sqTop);
                }
            }

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);

            if (!blendWasEnabled) GL.Disable(EnableCap.Blend);
            if (depthTestWasEnabled) GL.Enable(EnableCap.DepthTest);

            SwapBuffers();
        }



        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            //No lighting check
            if (KeyboardState.IsKeyPressed(Keys.L))
            {
                lightingEnabled = !lightingEnabled;
                Console.WriteLine($"Lighting enabled = {lightingEnabled}");
            }

            //Player bounding box with offsets + scaling
            (float playerLeft, float playerRight, float playerBottom, float playerTop) = GetPlayerBoundingBox();

            KeyboardState k = KeyboardState;
            MouseState mouse = MouseState;
            Vector2 mousePos = mouse.Position;
            float normalizedX = (mousePos.X / Size.X) * 2.0f - 1.0f;
            float normalizedY = 1.0f - (mousePos.Y / Size.Y) * 2.0f;

            float step = 0.00005f;
            Vector2 movement = Vector2.Zero;

            //Animation handling

            if (isReloading)
            {
                reloadTimer += args.Time;
                if (reloadTimer >= reloadAnimSpeed)
                {
                    reloadFrame++;
                    reloadTimer = 0;

                    if (reloadFrame >= reloadTextures.Count)
                    {
                        isReloading = false;
                        reloadFrame = 0;
                        Console.WriteLine("Reload complete");
                    }
                    else
                    {
                        playerTexture = reloadTextures[reloadFrame];
                    }
                }
                return; 
            }


            bool isIdle = !k.IsKeyDown(Keys.W) && !k.IsKeyDown(Keys.A) && !k.IsKeyDown(Keys.S) && !k.IsKeyDown(Keys.D);

            bool isMoving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.A) || k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.D);
            List<int> currentFrames = isMoving ? walkTextures : idleTextures;

            animTimer += args.Time;
            double currentSpeed = isMoving ? walkAnimSpeed : idleAnimSpeed;

            if (animTimer >= currentSpeed)
            {
                currentAnimFrame = (currentAnimFrame + 1) % currentFrames.Count;
                playerTexture = currentFrames[currentAnimFrame];
                animTimer = 0;
            }

            if (!editMode && KeyboardState.IsKeyPressed(Keys.R) && !isReloading)
            {
                isReloading = true;
                reloadFrame = 0;
                reloadTimer = 0;
                Console.WriteLine("Started reload animation");
            }

            //Edit mode handling

            if (k.IsKeyPressed(Keys.F1))
            {
                editMode = !editMode;
                Console.WriteLine($"Edit mode: {editMode}");

                if (editMode)
                {
                    highlightSquare = highlightTemplate;
                }
                else
                {
                    highlightSquare = null;
                }
            }

            if(k.IsKeyPressed (Keys.F9))
            {
                SaveMap();
            }


            if (editMode)
            {
                highlightedTileX = (int)((normalizedX - startX + tileWidth * 0.5f) / tileWidth);
                highlightedTileY = (int)((normalizedY - startY + tileHeight * 0.5f) / tileHeight);


                if (highlightInitialized && highlightedTileX >= 0 && highlightedTileX < cols && highlightedTileY >= 0 && highlightedTileY < rows)
                {
                    string[] pos = positionArray[highlightedTileY][highlightedTileX].Split(',');
                    float hx = float.Parse(pos[0]);
                    float hy = float.Parse(pos[1]);

                    highlightTemplate.CenterX = hx;
                    highlightTemplate.CenterY = hy;
                    UpdateSquareBuffer(ref highlightTemplate);
                }

                // Right click = select block to place
                if (MouseState.IsButtonPressed(MouseButton.Right))
                {
                    selectingObject = true;
                    Console.WriteLine("=== Map Object Selection ===");
                    for (int i = 0; i < availableObjects.Length; i++)
                    {
                        Console.WriteLine($"[{i + 1}] {availableObjects[i]}");
                    }
                    Console.WriteLine("Press 1-9 to choose object");
                }

                // Detect number key input for object selection
                if (selectingObject)
                {
                    for (int i = 0; i < availableObjects.Length && i < 9; i++)
                    {
                        if (KeyboardState.IsKeyPressed(Keys.KeyPad1 + i))
                        {
                            selectedEditObject = availableObjects[i];
                            selectingObject = false;
                            Console.WriteLine($"Selected: {selectedEditObject}");
                        }
                    }
                }

                // Left click = place selected object
                if (MouseState.IsButtonPressed(MouseButton.Left) && highlightedTileX >= 0 && highlightedTileX < cols && highlightedTileY >= 0 && highlightedTileY < rows)
                {
                    string[] pos = positionArray[highlightedTileY][highlightedTileX].Split(',');
                    float placeX = float.Parse(pos[0]);
                    float placeY = float.Parse(pos[1]);

                    PlaceObjectAt(selectedEditObject, placeX, placeY);
                }

                if (MouseState.IsButtonPressed(MouseButton.Middle) && highlightedTileX >= 0 && highlightedTileX < cols && highlightedTileY >= 0 && highlightedTileY < rows)
                {
                    mapArray[highlightedTileY][highlightedTileX] = "*";
                    CreateMap();
                }


            }




            //Movement Handling
            if (!editMode)
            {
                if (k.IsKeyDown(Keys.W))
                {
                    if (CanMove(playerLeft, playerRight, playerTop, playerBottom, 0f, step)) // TOP
                    {
                        movement.Y += step;
                    }
                }
                if (k.IsKeyDown(Keys.S))
                {
                    if (CanMove(playerLeft, playerRight, playerTop, playerBottom, 0f, -step)) // BOTTOM
                    {
                        movement.Y -= step;
                    }
                }
                if (k.IsKeyDown(Keys.A))
                {
                    if (CanMove(playerLeft, playerRight, playerTop, playerBottom, -step, 0f)) // LEFT
                    {
                        movement.X -= step;
                    }
                }
                if (k.IsKeyDown(Keys.D))
                {
                    if (CanMove(playerLeft, playerRight, playerTop, playerBottom, step, 0f)) // RIGHT
                    {
                        movement.X += step;
                    }
                }


                //Apply movement to the internal bounding geometry
                for (int i = 0; i < _vertices.Length; i += 3)
                {
                    _vertices[i] += movement.X;
                    _vertices[i + 1] += movement.Y;
                }

                //Recompute center + rotation
                _playerCenterX = (_vertices[0] + _vertices[3]) * 0.5f;
                _playerCenterY = (_vertices[1] + _vertices[4]) * 0.5f;
                Vector2 direction = new Vector2(normalizedX - _playerCenterX, normalizedY - _playerCenterY);
                float playerRotation = MathF.Atan2(direction.Y, direction.X);

                float playerWidth = 0.20f;
                float playerHeight = 0.20f;
                float cosA = MathF.Cos(playerRotation);
                float sinA = MathF.Sin(playerRotation);

                // top-left
                playerVertices[0] = _playerCenterX + (-playerWidth / 2) * cosA - (playerHeight / 2) * sinA;
                playerVertices[1] = _playerCenterY + (-playerWidth / 2) * sinA + (playerHeight / 2) * cosA;
                // top-right
                playerVertices[4] = _playerCenterX + (playerWidth / 2) * cosA - (playerHeight / 2) * sinA;
                playerVertices[5] = _playerCenterY + (playerWidth / 2) * sinA + (playerHeight / 2) * cosA;
                // bottom-right
                playerVertices[8] = _playerCenterX + (playerWidth / 2) * cosA - (-playerHeight / 2) * sinA;
                playerVertices[9] = _playerCenterY + (playerWidth / 2) * sinA + (-playerHeight / 2) * cosA;
                // bottom-left
                playerVertices[12] = _playerCenterX + (-playerWidth / 2) * cosA - (-playerHeight / 2) * sinA;
                playerVertices[13] = _playerCenterY + (-playerWidth / 2) * sinA + (-playerHeight / 2) * cosA;

                //Gun positioning
                float gunWidthScale = 0.18f;
                float gunHeightScale = 0.18f;
                float gunRotationScale = 1.0f;
                float gunXOffset = 0.13f;
                float gunYOffset = 0.0f;
                float halfGunWidth = 0.5f * gunWidthScale;
                float halfGunHeight = 0.5f * gunHeightScale;
                float gunOffsetX = gunXOffset * cosA - gunYOffset * sinA;
                float gunOffsetY = gunXOffset * sinA + gunYOffset * cosA;
                float gunCenterX = _playerCenterX + gunOffsetX;
                float gunCenterY = _playerCenterY + gunOffsetY;
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
            }

            // Update GL buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, playerVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, playerVertices.Length * sizeof(float), playerVertices);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo3);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices3.Length * sizeof(float), vertices3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, backgroundVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, backgroundVertices.Length * sizeof(float), backgroundVertices);



        }

        private void PlaceObjectAt(string objectName, float x, float y)
        {
            for (int row = 0; row < positionArray.Length; row++)
            {
                for (int col = 0; col < positionArray[row].Length; col++)
                {
                    string[] pos = positionArray[row][col].Split(',');
                    float tileX = float.Parse(pos[0]);
                    float tileY = float.Parse(pos[1]);

                    if (Math.Abs(tileX - x) < 0.001f && Math.Abs(tileY - y) < 0.001f)
                    {
                        if (mapArray[row][col] != "*" && mapArray[row][col] != "")
                        {
                            Console.WriteLine($"Can't place {objectName} at {x}, {y} — already occupied.");
                            return;
                        }

                        if (objectName == "Box") mapArray[row][col] = "S";

                        Console.WriteLine($"Placed {objectName} at {x}, {y}");

                        if (objectName == "Box")
                        {
                            boxSquare = CreateSquare(x, y, 0.12f, 0.15f, boxTextureLoc,true);
                            squares.Add(boxSquare);
                        }
                        if (objectName == "Dirt_GrassBottomLeft")
                        {
                            var dirtSquare = CreateSquare(x, y, 0.12f, 0.15f, Dirt_GrassBottomLeft, true);
                            squares.Add(dirtSquare);
                        }


                        return;
                    }
                }
            }

            Console.WriteLine($"Warning: Position ({x},{y}) not found in map grid.");
        }



        private (float left, float right, float bottom, float top) GetPlayerBoundingBox()
        {
            float rawLeft = _vertices[0];
            float rawRight = _vertices[3];
            float rawTop = _vertices[1];
            float rawBottom = _vertices[7];

            // Center & half-size
            float centerX = (rawLeft + rawRight) * 0.5f;
            float centerY = (rawTop + rawBottom) * 0.5f;

            float halfW = (rawRight - rawLeft) * 0.5f;
            float halfH = (rawTop - rawBottom) * 0.5f;

            // Scale
            halfW *= playerCollisionScaleX;
            halfH *= playerCollisionScaleY;

            // Rebuild
            float left = centerX - halfW;
            float right = centerX + halfW;
            float top = centerY + halfH;
            float bottom = centerY - halfH;

            // Offsets
            left += playerCollisionOffsetLeft;
            right -= playerCollisionOffsetRight;
            top -= playerCollisionOffsetTop;
            bottom += playerCollisionOffsetBottom;

            return (left, right, bottom, top);
        }


        private bool CanMove(float playerLeft, float playerRight, float playerTop, float playerBottom, float dx, float dy)
        {
            float newLeft = playerLeft + dx;
            float newRight = playerRight + dx;
            float newBottom = playerBottom + dy;
            float newTop = playerTop + dy;

            // Left edge
            if (newLeft < -1f)
                return false;

            if (newRight > 1f)
                return false;

            if (newBottom < -1f)
                return false;

            if (newTop > 1f)
            {
                if (currentMap == "Main")
                {
                    _playerCenterY = -1f + playerCollisionScaleY * 0.04f; // tp player to the bottom
                    _vertices[1] = _playerCenterY + 0.04f;
                    _vertices[4] = _playerCenterY + 0.04f;
                    _vertices[7] = _playerCenterY - 0.04f;
                    _vertices[10] = _playerCenterY - 0.04f;
                    LoadMap(Map1);
                    currentMap = "Map1";
                    Console.WriteLine($"Current map is {currentMap}");
                }
                else
                {
                    return false;
                }
            }


            foreach (var sq in squares)
            {
                if (!sq.HasCollision)
                    continue;

                float sqLeft = sq.CenterX - sq.HalfW;
                float sqRight = sq.CenterX + sq.HalfW;
                float sqBottom = sq.CenterY - sq.HalfH;
                float sqTop = sq.CenterY + sq.HalfH;

                bool overlapH = (newRight > sqLeft && newLeft < sqRight);
                bool overlapV = (newTop > sqBottom && newBottom < sqTop);
                if (overlapH && overlapV)
                {
                    return false;
                }
            }

            return true;
        }

        private void DrawBox(float left, float bottom, float right, float top)
        {
            float[] verts = new float[]
            {
                left,  bottom,
                right, bottom,
                right, top,
                left,  top
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, debugVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);

            // Triangle fan for 4 vertices => 2 triangles
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

          
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(ebo);

            GL.DeleteProgram(_shaderProgram);

            GL.DeleteBuffer(playerVbo);
            GL.DeleteVertexArray(playerVao);
            GL.DeleteBuffer(playerEbo);

            GL.DeleteBuffer(backgroundVbo);
            GL.DeleteVertexArray(backgroundVao);
            GL.DeleteBuffer(backgroundEbo);

            GL.DeleteProgram(shaderProgram);

            foreach (var sq in squares)
            {
                GL.DeleteBuffer(sq.Vbo);
                GL.DeleteBuffer(sq.Ebo);
                GL.DeleteVertexArray(sq.Vao);
            }

            // Debug resources
            GL.DeleteBuffer(debugVbo);
            GL.DeleteVertexArray(debugVao);
            GL.DeleteProgram(debugShader);

            // The shotgun buffers
            GL.DeleteBuffer(vbo3);
            GL.DeleteBuffer(ebo3);
            GL.DeleteVertexArray(vao3);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Compute aspect ratio
            float aspect = ClientSize.X / (float)ClientSize.Y;

            // Example: we want to keep height from -1 to +1,
            // but expand or shrink the X range to maintain ratio
            // so that the game region is 2.0 units tall, but 2.0 * aspect wide.
            //
            // left   = -aspect
            // right  = +aspect
            // bottom = -1
            // top    = +1
            // 
            // Then any geometry drawn in [-1..1, -1..1] will get pillarboxed if wide,
            // or letterboxed if tall, but will not stretch.

            float left = -aspect;
            float right = +aspect;
            float bottom = -1f;
            float top = +1f;

            // If you had a projection matrix, you’d recalculate it here:
            // projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar);
            // Then pass it to your shader or do the transform as appropriate.

            // In a pure -1..1 approach, you can also do nothing except draw your scene in the range [-aspect..+aspect, -1..+1],
            // so that you effectively see the same shape with black bars on the sides or top.
        }


        // A struct storing data for each “square” obstacle
        public struct SquareData
        {
            public int Vao;
            public int Vbo;
            public int Ebo;
            public int Texture;

            public float CenterX;
            public float CenterY;
            public float HalfW;
            public float HalfH;

            public bool HasCollision; 
        }


        public SquareData CreateSquare(float centerX, float centerY, float width, float height, string texturePath, bool hasCollision)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            float[] vertices =
            {
                centerX - halfW, centerY - halfH,  0.0f, 0.0f,
                centerX + halfW, centerY - halfH,  1.0f, 0.0f,
                centerX + halfW, centerY + halfH,  1.0f, 1.0f,
                centerX - halfW, centerY + halfH,  0.0f, 1.0f
            };  

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            int texture = LoadTexture(texturePath);

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            int posLocation = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(posLocation);
            GL.VertexAttribPointer(posLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            int texLocation = GL.GetAttribLocation(shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(texLocation);
            GL.VertexAttribPointer(texLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);

            return new SquareData
            {
                Vao = vao,
                Vbo = vbo,
                Ebo = ebo,
                Texture = texture,
                CenterX = centerX,
                CenterY = centerY,
                HalfW = halfW,
                HalfH = halfH,
                HasCollision = hasCollision
            };
        }

        public void LoadMap(string mapPath)
        {
            if (!File.Exists(mapPath))
            {
                Console.WriteLine($"Map file not found: {mapPath}");
                return;
            }

            string[] lines = File.ReadAllLines(mapPath);

            if (lines.Length != rows)
            {
                Console.WriteLine("Map file format mismatch: incorrect row count.");
                return;
            }

            for (int y = 0; y < rows; y++)
            {
                string[] tokens = lines[y].Split(' ');
                if (tokens.Length != cols)
                {
                    Console.WriteLine($"Map file format mismatch at row {y}: incorrect column count.");
                    return;
                }

                for (int x = 0; x < cols; x++)
                {
                    float worldX = startX + x * tileWidth;
                    float worldY = startY + y * tileHeight;

                    mapArray[y][x] = tokens[x];
                    positionArray[y][x] = $"{worldX:F2},{worldY:F2}";
                }
            }

            CreateMap();
            Console.WriteLine($"Successfully loaded map from {mapPath}");
        }


        private string LoadShaderSource(string filePath)
        {
            // A “Shaders” folder next to your .exe:
            string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
            string fullPath = Path.Combine(shaderDir, filePath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Shader file not found: {fullPath}");
                return "";
            }
            return File.ReadAllText(fullPath);
        }


        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: Texture file not found at {path}. Loading error texture.");
                path = Path.Combine(Path.GetDirectoryName(path), "error.png");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"ERROR: Error texture file not found at {path}. Cannot load texture.");
                    return 0;
                }
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
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                              OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            return id;
        }

        private int CreateDebugShader()
        {
            string vs = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
            }";

            string fs = @"
            #version 330 core
            out vec4 FragColor;
            uniform vec4 color;
            void main()
            {
                FragColor = color;
            }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vs);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fs);
            GL.CompileShader(fragmentShader);

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vertexShader);
            GL.AttachShader(prog, fragmentShader);
            GL.LinkProgram(prog);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return prog;
        }
        private int CreateLightingShader()
        {
            string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            out vec2 TexCoord;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
            ";
                        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                        GL.ShaderSource(vertexShader, vertexShaderSource);
                        GL.CompileShader(vertexShader);
                        string fragmentShaderSource = @"
            #version 330 core
            in vec2 TexCoord;
            out vec4 FragColor;
            uniform sampler2D texture1;
            uniform vec2 u_LightPos;
            uniform float u_LightRadius;
            uniform vec3 u_LightColor;
            uniform vec2 u_WindowSize;
            void main()
            {
                vec4 baseColor = texture(texture1, TexCoord);
                float ndcX = (gl_FragCoord.x / u_WindowSize.x) * 2.0 - 1.0;
                float ndcY = (gl_FragCoord.y / u_WindowSize.y) * 2.0 - 1.0;
                vec2 fragPosNDC = vec2(ndcX, ndcY);
                float dist = length(fragPosNDC - u_LightPos);
                float intensity = 1.0 - (dist * dist) / (u_LightRadius * u_LightRadius);
                intensity = clamp(intensity, 0.0, 1.0);
                vec3 finalColor = baseColor.rgb * (u_LightColor * intensity);
                FragColor = vec4(finalColor, baseColor.a);
            }
            ";
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        private int noLightingShaderProgram;

        private int CreateNoLightingShader()
        {
            string vs = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            out vec2 TexCoord;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
            ";

                    string fs = @"
            #version 330 core
            in vec2 TexCoord;
            out vec4 FragColor;
            uniform sampler2D texture1;
            void main()
            {
                FragColor = texture(texture1, TexCoord);
            }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vs);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fs);
            GL.CompileShader(fragmentShader);

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vertexShader);
            GL.AttachShader(prog, fragmentShader);
            GL.LinkProgram(prog);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return prog;
        }

        string path = null;
        private void SaveMap()
        {
            if (currentMap == "Main")
            {
                path = MainMap;
            }
            else if(currentMap == "Map1")
            {
                path = Map1;
            }
            else
            {
                throw new Exception($"Map saving error most likely somewhere in the map path. Curr path {path}");
            }
            
            try
            {

                using (StreamWriter writer = new StreamWriter(path))
                {
                    for (int row = 0; row < mapArray.Length; row++)
                    {
                        for (int col = 0; col < mapArray[row].Length; col++)
                        {
                            writer.Write(mapArray[row][col]);
                            if (col < mapArray[row].Length - 1)
                                writer.Write(" ");
                        }
                        writer.WriteLine();
                    }
                }

                Console.WriteLine("Map saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save map: " + ex.Message);
            }
        }

    }


}




//═══◆ KRST™ ENGINE ◆═══
//Built by Kryskata. Powered by OpenTK.
//© 2025 All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Landscape
{
    public struct VertexPositionNormalColored
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static int SizeInBytes = 7 * 4;
        /*public static VertexElement[] VertexElements = new VertexElement[]
        {
            new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position,0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float)*4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        };*/
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0));
    }

    public struct VertexMultiTextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = 14 * sizeof(float);
        public static readonly VertexElement[] VertexElements = new VertexElement[]{
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate,1)};
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;

        int terrainWidth;
        int terrainLength;
        float[,] heightData;

        VertexBuffer terrainVertexBuffer;
        IndexBuffer terrainIndexBuffer;
        //VertexDeclaration terrainVertexDeclaration;

        VertexBuffer waterVertexBuffer;
        //VertexDeclaration waterVertexDeclaration;

        VertexBuffer treeVertexBuffer;

        Effect effect;
        Effect bbEffect;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix reflectionViewMatrix;

        Vector3 cameraPosition = new Vector3(130, 30, -50);
        float leftRightRot = MathHelper.PiOver2;
        float upDownRot = -MathHelper.Pi / 10.0f;
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 30.0f;

        MouseState originalMouseState;

        Texture2D grassTexture;
        Texture2D rockTexture;
        Texture2D sandTexture;
        Texture2D snowTexture;
        Texture2D cloudMap;
        Texture2D waterBumpMap;//each rgb components of a pixel matches the normal of this pixel
        Texture2D treeTexture;
        Texture2D treeMap;

        Model skyDome;

        const float waterHeight = 5.0f;
        RenderTarget2D refractionRenderTarget;
        Texture2D refractionMap;

        RenderTarget2D reflectionRenderTarget;
        Texture2D reflectionMap;

        Vector3 windDirection = new Vector3(1, 0, 0);

        RenderTarget2D cloudsRenderTarget;
        Texture2D cloudStaticMap;
        VertexPositionTexture[] fullScreenVertices;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1500;
            graphics.PreferredBackBufferHeight = 1000;
            graphics.ApplyChanges();
            Window.Title = "Landscape";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = GraphicsDevice;

            effect = Content.Load<Effect>("HLSL");
            bbEffect = Content.Load<Effect>("bbEffect");
            UpdateViewMatrix();
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.3f, 1000.0f);

            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            skyDome = Content.Load<Model>("skydome");
            skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();

            PresentationParameters pp = device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, true, pp.BackBufferFormat, pp.DepthStencilFormat);
            reflectionRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, true, pp.BackBufferFormat, pp.DepthStencilFormat);

            cloudsRenderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, true, pp.BackBufferFormat, pp.DepthStencilFormat);

            LoadVertices();
            LoadTextures();
        }

        private void LoadTextures()
        {
            grassTexture = Content.Load<Texture2D>("grass");
            rockTexture = Content.Load<Texture2D>("rock");
            sandTexture = Content.Load<Texture2D>("sand");
            snowTexture = Content.Load<Texture2D>("snow");
            cloudMap = Content.Load<Texture2D>("cloudMap");
            waterBumpMap = Content.Load<Texture2D>("waterbump");
            treeTexture = Content.Load<Texture2D>("tree");
            cloudStaticMap = CreateStaticMap(32);
        }

        private void LoadVertices()
        {
            Texture2D heightMap = Content.Load<Texture2D>("heightmap");
            LoadHeightData(heightMap);
            VertexMultiTextured[] terrainVertices = SetUpTerrainVertices();
            int[] terrainIndices = SetUpTerrainIndices();
            terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
            CopyToTerrainBuffers(terrainVertices, terrainIndices);
            //terrainVertexDeclaration = new VertexDeclaration(terrainVertices);
            SetUpWaterVertices();
            //waterVertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());
            treeMap = Content.Load<Texture2D>("treeMap");
            List<Vector3> treeList = GenerateTreePositions(treeMap, terrainVertices);
            CreateBillboardVerticesFromList(treeList);
            fullScreenVertices = SetUpFullscreenVertices();
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            terrainWidth = heightMap.Width;
            terrainLength = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
        }

        private VertexMultiTextured[] SetUpTerrainVertices()
        {
            VertexMultiTextured[] terrainVertices = new VertexMultiTextured[terrainWidth * terrainLength];

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    //terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)(x / 30.0f);
                    //terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)(y / 30.0f);
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 10.0f;
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 10.0f;

                    terrainVertices[x + y * terrainWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 8.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 12) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 20) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 30) / 6.0f, 0, 1);

                    float total = terrainVertices[x + y * terrainWidth].TexWeights.X;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Y;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Z;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.W;

                    terrainVertices[x + y * terrainWidth].TexWeights.X /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Y /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Z /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.W /= total;
                }
            return terrainVertices;
        }

        private int[] SetUpTerrainIndices()
        {
            int[] indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainLength - 1; y++)
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            return indices;
        }

        private VertexMultiTextured[] CalculateNormals(VertexMultiTextured[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }

        private void CopyToTerrainBuffers(VertexMultiTextured[] vertices, int[] indices)
        {
            VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexMultiTextured.VertexElements);

            terrainVertexBuffer = new VertexBuffer(device, vertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }

        private void SetUpWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(0, waterHeight, -terrainLength), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, 0), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(terrainWidth, waterHeight, -terrainLength), new Vector2(1, 0));

            //VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexMultiTextured.VertexElements);
            waterVertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, waterVertices.Count(), BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
        }

        private void CreateBillboardVerticesFromList(List<Vector3> treeList)
        {
            VertexPositionTexture[] billboardVertices = new VertexPositionTexture[treeList.Count * 6];
            int i = 0;
            foreach (Vector3 currentV3 in treeList)
            {
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));

                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 1));
            }

            treeVertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, billboardVertices.Length * VertexPositionTexture.VertexDeclaration.VertexStride, BufferUsage.WriteOnly);
            treeVertexBuffer.SetData(billboardVertices);
        }

        private List<Vector3> GenerateTreePositions(Texture2D treeMap, VertexMultiTextured[] terrainVertices)
        {
            Color[] treeMapColors = new Color[treeMap.Width * treeMap.Height];
            treeMap.GetData(treeMapColors);

            int[,] noiseData = new int[treeMap.Width, treeMap.Height];
            for (int x = 0; x < treeMap.Width; x++)
                for (int y = 0; y < treeMap.Height; y++)
                    noiseData[x, y] = treeMapColors[y + x * treeMap.Height].R;


            List<Vector3> treeList = new List<Vector3>(); Random random = new Random();

            for (int x = 2; x < terrainWidth - 2; x++)
            {
                for (int y = 2; y < terrainLength - 2; y++)
                {
                    float terrainHeight = heightData[x, y];
                    if ((terrainHeight > 8) && (terrainHeight < 14))
                    {
                        float flatness = Vector3.Dot(terrainVertices[x + y * terrainWidth].Normal, new Vector3(0, 1, 0));
                        float minFlatness = (float)Math.Cos(MathHelper.ToRadians(15));
                        if (flatness > minFlatness)
                        {
                            float relx = (float)x / (float)terrainWidth;
                            float rely = (float)y / (float)terrainLength;

                            float noiseValueAtCurrentPosition = noiseData[(int)(relx * treeMap.Width), (int)(rely * treeMap.Height)];
                            float treeDensity;
                            if (noiseValueAtCurrentPosition > 200)
                                treeDensity = 5;
                            else if (noiseValueAtCurrentPosition > 150)
                                treeDensity = 4;
                            else if (noiseValueAtCurrentPosition > 100)
                                treeDensity = 3;
                            else
                                treeDensity = 0;

                            for (int currDetail = 0; currDetail < treeDensity; currDetail++)
                            {
                                float rand1 = (float)random.Next(1000) / 1000.0f;
                                float rand2 = (float)random.Next(1000) / 1000.0f;
                                Vector3 treePos = new Vector3((float)x - rand1, 0, -(float)y - rand2);
                                treePos.Y = heightData[x, y];
                                treeList.Add(treePos);
                            }
                        }
                    }
                }
            }

            return treeList;
        }

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[y + x * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(device, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);

            base.Update(gameTime);
        }

        private void ProcessInput(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Space))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.E))
                moveVector += new Vector3(0, -1, 0);
            AddToCameraPosition(moveVector * amount);

            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftRightRot -= rotationSpeed * xDifference * amount;
                upDownRot -= rotationSpeed * yDifference * amount;
                Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                UpdateViewMatrix();
            }
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);

            Vector3 reflCameraPosition = cameraPosition;
            reflCameraPosition.Y = -cameraPosition.Y + waterHeight * 2;

            Vector3 reflTargetPos = cameraFinalTarget;
            reflTargetPos.Y = -cameraFinalTarget.Y + waterHeight * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPos - reflCameraPosition);

            reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            //device.RasterizerState = RasterizerState.CullNone;
            /*RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            device.RasterizerState = rs;*/

            DrawRefractionMap();
            DrawReflectionMap();
            GeneratePerlinNoise(time);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            DrawSkyDome(viewMatrix);
            DrawTerrain(viewMatrix);
            DrawWater(time);
            DrawBillboard(viewMatrix);

            SpriteBatch spriteBatch = new SpriteBatch(device);
            spriteBatch.Begin();
            Vector2 pos = new Vector2(graphics.PreferredBackBufferWidth - (graphics.PreferredBackBufferWidth / 10), 0);
            spriteBatch.Draw(refractionMap, pos, null, Color.White, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);
            Vector2 pos2 = new Vector2(graphics.PreferredBackBufferWidth - (graphics.PreferredBackBufferWidth / 10), graphics.PreferredBackBufferHeight / 10);
            spriteBatch.Draw(reflectionMap, pos2, null, Color.White, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawTerrain(Matrix currentViewMatrix)
        {
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            Matrix worldMatrix = Matrix.Identity;

            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(terrainVertexBuffer);
                device.Indices = terrainIndexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainVertexBuffer.VertexCount, 0, terrainIndexBuffer.IndexCount / 3);
            }
        }

        private void DrawSkyDome(Matrix currentViewMatrix)
        {
            device.DepthStencilState = DepthStencilState.None;
            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(200) * Matrix.CreateTranslation(cameraPosition);

            device.BlendState = BlendState.AlphaBlend;
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xSkyDomeTexture"].SetValue(cloudMap);
                }
                //device.RasterizerState = RasterizerState.CullNone;
                mesh.Draw();
            }

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }

        private Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
                planeCoeffs *= -1;

            /*Matrix worldViewProjection = currentViewMatrix * projectionMatrix;//=cameraMatrix
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);*/
            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }

        private void DrawRefractionMap()
        {
            Plane refractionPlane = CreatePlane(waterHeight + 1.5f, new Vector3(0, -1, 0), viewMatrix, false);
            effect.Parameters["ClipPlane0"].SetValue(new Vector4(refractionPlane.Normal, refractionPlane.D));
            effect.Parameters["Clipping"].SetValue(true);
            device.SetRenderTarget(refractionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawTerrain(viewMatrix);
            effect.Parameters["Clipping"].SetValue(false);
            device.SetRenderTarget(null);
            refractionMap = refractionRenderTarget;
        }

        private void DrawReflectionMap()
        {
            Plane reflectionPlane = CreatePlane(waterHeight - 0.5f, new Vector3(0, -1, 0), reflectionViewMatrix, true);
            effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));
            effect.Parameters["Clipping"].SetValue(true);
            device.SetRenderTarget(reflectionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkBlue, 1.0f, 0);
            DrawSkyDome(reflectionViewMatrix);
            DrawTerrain(reflectionViewMatrix);
            DrawBillboard(reflectionViewMatrix);
            effect.Parameters["Clipping"].SetValue(false);
            device.SetRenderTarget(null);
            reflectionMap = reflectionRenderTarget;

            /*System.IO.Stream ss = System.IO.File.OpenWrite("lol.jpeg");
            reflectionRenderTarget.SaveAsJpeg(ss, 1000, 1000);
            ss.Close();*/
        }

        private void DrawWater(float time)
        {
            effect.CurrentTechnique = effect.Techniques["Water"];
            Matrix worldMatrix = Matrix.Identity;

            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xReflectionView"].SetValue(reflectionViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            effect.Parameters["xRefractionMap"].SetValue(refractionMap);
            effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            effect.Parameters["xWaveLength"].SetValue(0.1f);
            effect.Parameters["xWaveHeight"].SetValue(0.1f);
            effect.Parameters["xCamPos"].SetValue(cameraPosition);
            effect.Parameters["xWindDirection"].SetValue(windDirection);
            effect.Parameters["xWindForce"].SetValue(0.0005f);
            effect.Parameters["xTime"].SetValue(time);

            effect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffer(waterVertexBuffer);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, waterVertexBuffer.VertexCount / 3);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(waterVertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, waterVertexBuffer.VertexCount / 3);
            }
        }

        private void DrawBillboard(Matrix currentViewMatrix)
        {
            bbEffect.CurrentTechnique = bbEffect.Techniques["CylBillboard"];
            bbEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            bbEffect.Parameters["xView"].SetValue(currentViewMatrix);
            bbEffect.Parameters["xProjection"].SetValue(projectionMatrix);
            bbEffect.Parameters["xCamPos"].SetValue(cameraPosition);
            bbEffect.Parameters["xAllowedRotDir"].SetValue(new Vector3(0, 1, 0));
            bbEffect.Parameters["xBillboardTexture"].SetValue(treeTexture);

            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in bbEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(treeVertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, treeVertexBuffer.VertexCount / 3);
            }
            device.BlendState = BlendState.Opaque;
        }

        private void GeneratePerlinNoise(float time)
        {
            device.SetRenderTarget(cloudsRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkBlue, 1.0f, 0);

            effect.CurrentTechnique = effect.Techniques["PerlinNoise"];
            effect.Parameters["xTexture"].SetValue(cloudStaticMap);
            effect.Parameters["xOvercast"].SetValue(1.1f);
            effect.Parameters["xTime"].SetValue(time / 1000.0f);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }

            device.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;

            /*System.IO.Stream ss = System.IO.File.OpenWrite("lol.jpeg");
            cloudsRenderTarget.SaveAsJpeg(ss, 1000, 1000);
            ss.Close();*/
        }
    }
}

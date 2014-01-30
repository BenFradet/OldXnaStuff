using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using GameStateManagement;

namespace Platformer
{
    class Level: IDisposable
    {
        Tile[,] tiles;
        //Layer[] layers;
        Layer layer;

        //const int EntityLayer = 2;

        public Player Player { get; private set; }

        public List<Gem> gems = new List<Gem>();
        public List<Enemy> enemies = new List<Enemy>();

        public string Name { get; set; }

        static readonly Point InvalidPosition = new Point(-1, -1);
        Vector2 start;
        Point exit = InvalidPosition;

        Random random = new Random(358479);

        float cameraPosition;

        public static int Score;

        public bool ReachedExit { get; private set; }

        private SoundEffect exitReachedSound;

        public TimeSpan TimeRemaining { get; private set; }

        const int PointsPerSecond = 5;

        public ContentManager Content { get; private set; }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            Content = new ContentManager(serviceProvider, "Content");

            TimeRemaining = TimeSpan.FromMinutes(3.0);

            LoadTiles(fileStream);

            layer = new Layer(Content, "Backgrounds/bg", 0.5f);

            /*layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);*/

            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        private void LoadTiles(Stream stream)
        {
            int width;
            List<string> lines = new List<string>();
            using(StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                width = line.Length;

                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different", lines.Count));
                    line = reader.ReadLine();
                }
            }

            tiles = new Tile[width, lines.Count];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            if (Player == null)
                throw new NotSupportedException("Starting point needed");
            if (exit == InvalidPosition)
                throw new NotSupportedException("Ending point needed");

        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                case '.':
                    return new Tile(null, TileCollision.Passable);
                case 'X':
                    return LoadExitTile(x, y);
                case 'G':
                    return LoadGemTile(x, y, false);
                case 'P':
                    return LoadGemTile(x, y, true);
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA");
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");
                case 'S':
                    return LoadStartTile(x, y);
                case '#':
                    return LoadTile("regular", TileCollision.Impassable);
                case '?':
                    return LoadTile("qm", TileCollision.Question);
                case '1':
                    return LoadTile("tl", TileCollision.Impassable);
                case '2':
                    return LoadTile("tm", TileCollision.Impassable);
                case '3':
                    return LoadTile("tr", TileCollision.Impassable);
                case '4':
                    return LoadTile("ml", TileCollision.Impassable);
                case '5':
                    return LoadTile("mm", TileCollision.Impassable);
                case '6':
                    return LoadTile("mr", TileCollision.Impassable);
                case '7':
                    return LoadTile("bl", TileCollision.Impassable);
                case '8':
                    return LoadTile("bm", TileCollision.Impassable);
                case '9':
                    return LoadTile("br", TileCollision.Impassable);
                default:
                    throw new NotSupportedException("Invalid character in the .txt file");
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("Only one exit");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        private Tile LoadGemTile(int x, int y, bool isPowerUp)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y), isPowerUp));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadEnemyTile(int x, int y, string monster)
        {
            Vector2 position = RectangleExtended.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, monster));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("Only one start");

            start = RectangleExtended.GetBottomCenter(GetBounds(x, y));
            Player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public TileCollision GetCollision(int x, int y)
        {
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;

            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public void SetCollision(int x, int y)
        {
            if(GetCollision(x, y) == TileCollision.Question)
                tiles[x, y].Collision = TileCollision.Impassable;
        }

        public void Update(GameTime gameTime, InputState inputState)
        {
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
                Player.ApplyPhysics(gameTime);
            else if (ReachedExit)
            {
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                TimeRemaining -= TimeSpan.FromSeconds(seconds);
                Score += seconds * PointsPerSecond;
            }
            else
            {
                TimeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, inputState);

                UpdateGems(gameTime);

                //top.y = 0
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                if (Player.IsAlive && Player.IsOnGround && Player.BoundingRectangle.Contains(exit))
                    OnExitReached();
            }
            if (TimeRemaining < TimeSpan.Zero)
                TimeRemaining = TimeSpan.Zero;
        }

        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; i++)
            {
                Gem gem = gems[i];
                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i);
                    OnGemCollected(gem, Player);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy e in enemies)
            {
                e.Update(gameTime);

                if (e.IsAlive && e.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (Player.IsPoweredUp)
                        OnEnemyKilled(e, Player);
                    else
                        OnPlayerKilled(e);
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Player player)
        {
            enemy.OnKilled(player);
        }

        private void OnGemCollected(Gem gem, Player player)
        {
            Score += gem.PointValue;
            gem.OnCollected(player);
        }

        private void OnPlayerKilled(Enemy enemy)
        {
            Player.OnKilled(enemy);
        }

        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            ReachedExit = true;
        }

        public void StartNewLife()
        {
            Player.Resurect(start);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            layer.Draw(spriteBatch, cameraPosition);
            /*for (int i = 0; i <= EntityLayer; i++)
                layers[i].Draw(spriteBatch, cameraPosition);*/
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            //care
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Gem g in gems)
                g.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy e in enemies)
                e.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            /*spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; i++)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();*/
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);
            for (int y = 0; y < Height; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }
    }
}

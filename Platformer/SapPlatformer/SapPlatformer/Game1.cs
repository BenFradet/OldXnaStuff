using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GameStateManagement;
using Platformer.Libz;
using Platformer.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Platformer
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        ScreenFactory screenFactory;

        public Game1()
        {
            //IsMouseVisible = true;
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            screenFactory = new ScreenFactory();
            Services.AddService(typeof(IScreenFactory), screenFactory);
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            AddInitialScreen();
        }

        private void AddInitialScreen()
        {
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }

        /*GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D dieOverlay;

        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed = false;

        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private KeyboardState keyboardState;

        private const int numberOfLevels = 3;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            dieOverlay = Content.Load<Texture2D>("Overlays/you_died");

            LoadNextLevel();
        }

        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            level.Update(gameTime, keyboardState);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            bool continuePressed = keyboardState.IsKeyDown(Keys.Space);

            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                    level.StartNewLife();
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }
            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            levelIndex = (levelIndex + 1) % numberOfLevels;

            if (level != null)
                level.Dispose();

            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        public void ReloadCurrentLevel()
        {
            levelIndex--;
            LoadNextLevel();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            level.Draw(gameTime, spriteBatch);
            DrawHUD();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f, titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;

            if (level.TimeRemaining > WarningTime || level.ReachedExit || (int)level.TimeRemaining.TotalSeconds % 2 == 0)
                timeColor = Color.Yellow;
            else
                timeColor = Color.Red;
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                    status = winOverlay;
                else
                    status = loseOverlay;
            }
            else if (!level.Player.IsAlive)
                status = dieOverlay;

            if (status != null)
            {
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, text, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, text, position, color);
        }*/
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace GameStateManagement
{
    public class ScreenManager: DrawableGameComponent
    {
        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> tmpScreensList = new List<GameScreen>();

        InputState input = new InputState();

        SpriteBatch spriteBatch;
        SpriteFont fontHUD;
        SpriteFont fontQ;
        Texture2D blankTexture;

        bool isInitialized;
        bool traceEnabled;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        public SpriteFont FontQ
        {
            get { return fontQ; }
        }

        public SpriteFont FontHUD
        {
            get { return fontHUD; }
        }

        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }

        public Texture2D BlankTexture
        {
            get { return blankTexture; }
        }

        public ScreenManager(Game game) : base(game) { }

        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        protected override void LoadContent()
        {
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontHUD = content.Load<SpriteFont>("Fonts/Hud");
            fontQ = content.Load<SpriteFont>("Fonts/Question");
            blankTexture = content.Load<Texture2D>("Backgrounds/blank");

            foreach (GameScreen screen in screens)
                screen.Activate(false);
        }

        protected override void UnloadContent()
        {
            foreach (GameScreen screen in screens)
                screen.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            input.Update();

            tmpScreensList.Clear();

            foreach (GameScreen screen in screens)
                tmpScreensList.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            while (tmpScreensList.Count > 0)
            {
                GameScreen screen = tmpScreensList[tmpScreensList.Count - 1];
                tmpScreensList.RemoveAt(tmpScreensList.Count - 1);

                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
                {
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, input);
                        otherScreenHasFocus = true;
                    }
                    if (!screen.IsPopUp)
                        coveredByOtherScreen = true;
                }
            }
            if (traceEnabled)
                TraceScreens();
        }

        void TraceScreens()
        {
            List<string> screenNames = new List<string>();
            foreach (GameScreen screen in screens)
                screenNames.Add(screens.GetType().Name);
            Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;
                screen.Draw(gameTime);
            }
        }

        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.IsExiting = false;

            if (isInitialized)
                screen.Activate(false);
            screens.Add(screen);
        }

        public void RemoveScreen(GameScreen screen)
        {
            if (isInitialized)
                screen.Unload();
            screens.Remove(screen);
            tmpScreensList.Remove(screen);
        }

        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }

        public void FadeBackBufferToBlack(float alpha)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(blankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
            spriteBatch.End();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer.Screens
{
    class BackgroundScreen : GameScreen
    {
        ContentManager content;
        Texture2D background;

        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
                background = content.Load<Texture2D>("Backgrounds/bg");
            }
        }

        public override void Deactivate()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.Game.GraphicsDevice.Viewport;
            Rectangle fullScreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(background, fullScreen, Color.White);
            ScreenManager.SpriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string text, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, text, position + new Vector2(1.0f, 1.0f), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, text, position, color);
        }
    }
}

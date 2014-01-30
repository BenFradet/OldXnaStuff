using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Platformer.Screens
{
    class MessageBoxScreen : GameScreen
    {
        public string Message { get; set; }
        public int NrLines { get; set; }
        Texture2D overlay;

        public EventHandler<LinkEventArgs> Link;
        public EventHandler<EventArgs> Accepted;
        public EventHandler<EventArgs> Cancelled;

        public MessageBoxScreen(string message) : this(message, true) { }

        public MessageBoxScreen(string message, bool includeUsageText)
        {
            const string usageText = "\nSpace, Enter = ok" + "\nEsc = Cancel";

            if (includeUsageText)
                Message = message + usageText;
            else
                Message = message;

            IsPopUp = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            NrLines = 1;
        }

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                overlay = ScreenManager.Game.Content.Load<Texture2D>("Overlays/blankOverlay");
            }
        }

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            KeyboardState keyboardState = inputState.CurrentKeyboardState;
            KeyboardState lastKeyboardState = inputState.LastKeyboardState;

            if ((keyboardState.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space)) || (keyboardState.IsKeyDown(Keys.Enter) && !lastKeyboardState.IsKeyDown(Keys.Enter)))
            {
                if (Accepted != null)
                    Accepted(this, new EventArgs());
                if(Link != null)
                    Link(this, new LinkEventArgs("www.google.com"));
                ExitScreen();
            }
            else if (keyboardState.IsKeyDown(Keys.Escape) && !lastKeyboardState.IsKeyDown(Keys.Escape))
            {
                if (Cancelled != null)
                    Cancelled(this, new EventArgs());
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            Viewport viewport = ScreenManager.Game.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            string messageToDraw = ParseText(Message, ScreenManager.FontHUD);
            Vector2 textSize = ScreenManager.FontHUD.MeasureString(messageToDraw);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad, (int)textPosition.Y - vPad, (int)textSize.X + hPad * 2, (int)textSize.Y + vPad * 2);
            Color color = Color.White * TransitionAlpha;

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(overlay, backgroundRectangle, color);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.FontHUD, messageToDraw, textPosition, color);
            ScreenManager.SpriteBatch.End();
        }

        private string ParseText(string text, SpriteFont font)
        {
            string line = String.Empty;
            string returnString = String.Empty;
            string[] wordArray = text.Split(' ');
            NrLines = 1;

            foreach (String word in wordArray)
            {
                if (font.MeasureString(line + word).Length() > 600)
                {
                    returnString = returnString + line + '\n';
                    NrLines++;
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Platformer.Screens;

namespace Platformer.Screens
{
    class MenuEntry
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        float selectionFade;
        public int NrLines { get; private set; }

        public MenuEntry(string text)
        {
            Text = text;
            NrLines = 1;
        }

        public event EventHandler<SelectedIndexEventArgs> Selected;

        protected internal virtual void OnSelectEntry(int selectedIndex)
        {
            if(Selected != null)
                Selected(this, new SelectedIndexEventArgs(selectedIndex));
        }

        public virtual void Update(GameTime gameTime, MenuScreen menuScreen, bool isSelected)
        {
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }

        public virtual void Draw(GameTime gameTime, MenuScreen menuScreen, bool isSelected, int linesSoFar)
        {
            Color color = isSelected ? Color.Yellow : Color.Black;
            double time = gameTime.TotalGameTime.TotalSeconds;
            float pulsate = (float)Math.Sin(time * 6) + 1;
            float scale = 1 + pulsate * 0.01f * selectionFade;
            color *= menuScreen.TransitionAlpha;

            ScreenManager screenManager = menuScreen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.FontHUD;

            string textToDraw = ParseText(Text, font);
            Vector2 origin = new Vector2(0, font.LineSpacing / 20 * linesSoFar);

            spriteBatch.DrawString(font, textToDraw, Position, color, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
        }

        public virtual int GetHeight(MenuScreen menuScreen)
        {
            ParseText(Text, menuScreen.ScreenManager.FontHUD);
            return menuScreen.ScreenManager.FontHUD.LineSpacing * NrLines;
        }

        public virtual int GetWidth(MenuScreen menuScreen)
        {
            return (int)menuScreen.ScreenManager.FontHUD.MeasureString(ParseText(Text, menuScreen.ScreenManager.FontHUD)).X;
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

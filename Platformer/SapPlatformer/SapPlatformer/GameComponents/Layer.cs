using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    class Layer
    {
        //public Texture2D[] Textures { get; private set; }
        public Texture2D Texture { get; private set; }
        public float ScrollRate { get; private set; }

        private const int NumberOflayers = 3;

        public Layer(ContentManager content, string basePath, float scrollRate)
        {
            /*Textures = new Texture2D[NumberOflayers];

            for (int i = 0; i < NumberOflayers; i++)
                Textures[i] = content.Load<Texture2D>(basePath + "_" + i);*/

            Texture = content.Load<Texture2D>(basePath);

            ScrollRate = scrollRate;
        }

        public void Draw(SpriteBatch spriteBatch, float cameraPosition)
        {
            //int segmentWidth = Textures[0].Width;
            int segmentWidth = Texture.Width;

            float x = cameraPosition * ScrollRate;
            int leftSegment = (int)Math.Floor(x / segmentWidth);
            int rightSegment = leftSegment + 1;
            x = (x / segmentWidth - leftSegment) * -segmentWidth;

            spriteBatch.Draw(Texture, new Vector2(x, 0.0f), Color.White);
            spriteBatch.Draw(Texture, new Vector2(x + segmentWidth, 0.0f), Color.White);
            /*spriteBatch.Draw(Textures[leftSegment % Textures.Length], new Vector2(x, 0.0f), Color.White);
            spriteBatch.Draw(Textures[rightSegment % Textures.Length], new Vector2(x + segmentWidth, 0.0f), Color.White);*/
        }
    }
}

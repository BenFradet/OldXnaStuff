using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Platformer.Libz;

namespace Platformer
{
    class Gem
    {
        Texture2D texture;
        Vector2 origin;
        SoundEffect collectedSound;

        public readonly int PointValue = 30;
        public bool IsPowerUp { get; private set; }
        public readonly Color Color;

        public Vector2 basePosition;
        float bounce;

        public Level Level { get; private set; }

        public Vector2 Position
        {
            get { return basePosition + new Vector2(0.0f, bounce); }
        }

        public Circle BoundingCircle
        {
            get { return new Circle(Position, Tile.Width / 3.0f); }
        }

        public Gem(Level level, Vector2 position, bool isPowerUp)
        {
            IsPowerUp = isPowerUp;
            Level = level;
            this.basePosition = position;

            if (IsPowerUp)
            {
                PointValue = 100;
                Color = Color.Red;
            }
            else
            {
                PointValue = 30;
                Color = Color.Yellow;
            }

            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        public void Update(GameTime gameTime)
        {
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
            if (IsPowerUp)
                collectedBy.PowerUp();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    class AnimationPlayer
    {
        public Animation Animation 
        {
            get { return animation; } 
        }
        Animation animation;
        public int FrameIndex { get; private set; }

        public float Time { get; private set; }

        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        public void PlayAnimation(Animation animation)
        {
            if (Animation == animation)
                return;

            this.animation = animation;
            FrameIndex = 0;
            Time = 0.0f;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation playing");

            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (Time > Animation.FrameTime)
            {
                Time -= Animation.FrameTime;

                if (Animation.IsLooping)
                    FrameIndex = (FrameIndex + 1) % Animation.FrameCount;
                else
                    FrameIndex = Math.Min(FrameIndex + 1, Animation.FrameCount - 1);
            }

            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

            spriteBatch.Draw(Animation.Texture, position, source, color, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);  
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            Draw(gameTime, spriteBatch, position, spriteEffects, Color.White);
        }
    }
}

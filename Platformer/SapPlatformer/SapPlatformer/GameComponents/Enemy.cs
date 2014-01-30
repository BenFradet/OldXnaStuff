using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    enum FaceDirection
    {
        Left = -1,
        Right = 1
    }

    class Enemy
    {
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private AnimationPlayer sprite;

        public Level Level { get; private set; }

        public bool IsAlive { get; private set; }

        public Vector2 Position { get; private set; }

        private Rectangle localBounds;

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        private FaceDirection direction = FaceDirection.Left;

        private float waitTime;
        private const float MaxWaitTime = 0.5f;

        private const float MoveSpeed = 64.0f;

        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            Level = level;
            Position = position;
            IsAlive = true;
            sprite = new AnimationPlayer();
            LoadContent(spriteSet);
        }

        public void LoadContent(string spriteSet)
        {
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            sprite.PlayAnimation(idleAnimation);

            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsAlive)
                return;

            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                    direction = (FaceDirection)(-(int)direction);
            }
            else
            {
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable || Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                    waitTime = MaxWaitTime;
                else
                {
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    Position = Position + velocity;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsAlive)
                sprite.PlayAnimation(dieAnimation);
            else if (!Level.Player.IsAlive || Level.ReachedExit || Level.TimeRemaining == TimeSpan.Zero || waitTime > 0)
                sprite.PlayAnimation(idleAnimation);
            else
                sprite.PlayAnimation(runAnimation);

            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip); 
        }

        public void OnKilled(Player killedBy)
        {
            IsAlive = false;
        }
    }
}

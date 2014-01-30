using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;

namespace Platformer
{
    class Player
    {
        Animation idleAnimation;
        Animation runAnimation;
        Animation dieAnimation;
        Animation jumpAnimation;
        Animation celebrateAnimation;

        SpriteEffects flip = SpriteEffects.None;

        //private AnimationPlayer Sprite;
        public AnimationPlayer Sprite { get; private set; }

        public Level Level { get; private set; }

        public bool IsAlive { get; private set; }

        const float MaxPowerUpTime = 6.0f;
        float powerUpTime;
        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }
        readonly Color[] poweredUpColors = { Color.Red, Color.Blue, Color.Orange, Color.Yellow };
        SoundEffect powerUpSound;

        public bool Question { get; set; }

        Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        float previousBottom;

        Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        const float MoveAcceleration = 13000.0f;
        const float MaxMoveSpeed = 1750.0f;
        const float GroundDragFactor = 0.48f;
        const float AirDragFactor = 0.58f;

        const float MaxJumpTime = 0.35f;
        const float JumpLaunchVelocity = -3500.0f;
        const float GravityAcceleration = 3200f;
        const float MaxFallSpeed = 550.0f;
        const float JumpControlPower = 0.14f;

        const float MoveStickScale = 1.0f;

        public bool IsOnGround { get; private set; }

        float movement;

        bool isJumping;
        bool wasJumping;
        float jumpTime;

        Rectangle localBounds;

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - Sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - Sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public Player(Level level, Vector2 position)
        {
            Level = level;
            Sprite = new AnimationPlayer();
            LoadContent();
            Resurect(position);
            Question = false;
        }

        public void Resurect(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            IsAlive = true;
            powerUpTime = 0.0f;
            Sprite.PlayAnimation(idleAnimation);
        }

        public void LoadContent()
        {
            string root = "Sprites/Player/";
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(root + "Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>(root + "Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>(root + "Run"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(root + "Idle"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>(root + "Run"), 0.1f, false);

            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/Powerup");

            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Update(GameTime gameTime, InputState inputState)
        {
            Question = false;

            GetInput(inputState);
            ApplyPhysics(gameTime);                

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                    Sprite.PlayAnimation(runAnimation);
                else
                    Sprite.PlayAnimation(idleAnimation);
            }

            movement = 0.0f;
            isJumping = false;
        }

        public void GetInput(InputState inputState)
        {
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            if (inputState.CurrentKeyboardState.IsKeyDown(Keys.A) || inputState.CurrentKeyboardState.IsKeyDown(Keys.Left))
                movement = -1.0f;
            else if (inputState.CurrentKeyboardState.IsKeyDown(Keys.D) || inputState.CurrentKeyboardState.IsKeyDown(Keys.Right))
                movement = 1.0f;

            isJumping = inputState.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputState.CurrentKeyboardState.IsKeyDown(Keys.W) 
                || inputState.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputState.IsLeftMouseButtonPressed();
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 previousPosition = Position;
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(Velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(Velocity.Y, gameTime);

            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            HandleCollisions();

            if (Position.X == previousPosition.X)
                velocity.X = 0;
            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            if (isJumping)
            {
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Sprite.PlayAnimation(jumpAnimation);
                }

                if (jumpTime > 0.0f && jumpTime <= MaxJumpTime)
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                else
                    jumpTime = 0.0f;
            }
            else
                jumpTime = 0.0f;

            wasJumping = isJumping;

            return velocityY;
        }

        private void HandleCollisions()
        {
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Height) - 1;

            IsOnGround = false;

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtended.GetIntersectionDepth(bounds, tileBounds);

                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                if (previousBottom <= tileBounds.Top)
                                    IsOnGround = true;

                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable)
                            {
                                Position = new Vector2(Position.X + depth.X, Position.Y);
                                bounds = BoundingRectangle;
                            }
                            else if (collision == TileCollision.Question)
                            {
                                Question = true;
                                Level.SetCollision(x, y);
                            }
                        }
                    }
                }
            }
            previousBottom = bounds.Bottom;
        }

        public void OnKilled(Enemy killedBy)
        {
            IsAlive = false;

            Sprite.PlayAnimation(dieAnimation);
        }

        public void OnReachedExit()
        {
            Sprite.PlayAnimation(celebrateAnimation);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Velocity.X > 0)
                flip = SpriteEffects.None;
            else if (Velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;

            Color color;
            if (IsPoweredUp)
            {
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
                int colorIndex = (int)t % poweredUpColors.Length;
                color = poweredUpColors[colorIndex];
            }
            else
                color = Color.White;

            Sprite.Draw(gameTime, spriteBatch, Position, flip, color);
        }

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }
    }
}

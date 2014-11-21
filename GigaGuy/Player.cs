using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GigaGuy
{
    class Player
    {
        private Texture2D renderTexture;
        private Texture2D idleTexture;
        private Texture2D duckTexture;

        private KeyboardState keyboardState;
        private KeyboardState lastState;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public RectangleF Hitbox
        {
            get
            {
                return new RectangleF(
                    Position.X, Position.Y,
                    playerWidth, playerHeight);
            }
        }

        private int playerWidth = 32;
        private int playerHeight = 64;

        private float speed = 1.5f;
        private float jumpSpeed = 10f;
        private float terminalSpeed;
        private const float terminalSpeedDefault = 0.75f;

        private float slideTimer;
        private float slideTimerDefault = 0.1f;
        private float jumpTimer;
        private float jumpTimerDefault = 0.4f;

        private const float xMaxSpeed = 10f;
        private const float yMaxSpeed = 17.5f;

        private bool isMoving;
        public bool IsDucking { get; private set; }
        public bool IsOnGround { get; set; }

        public bool IsOnWall { get; set; }
        public bool IsOnRightWall { get; set; }
        public bool IsJumping { get; set; }
        private bool wasOnWall;
        

        public Player()
        {
            Position = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);
            terminalSpeed = terminalSpeedDefault;
            slideTimer = slideTimerDefault;
            jumpTimer = jumpTimerDefault;
        }

        public void LoadContent(ContentManager Content)
        {
            idleTexture = Content.Load<Texture2D>("player");
            duckTexture = Content.Load<Texture2D>("playerDuck");
            renderTexture = idleTexture;
        }

        public void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            HandleMovement();
            HandleDucking();
            HandleJumping(gameTime);
            HandleWallSliding(gameTime);
            HandlePhysics(gameTime);
            lastState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offSet)
        {
            spriteBatch.Draw(renderTexture, Position + offSet, Color.White);
        }

        private void HandlePhysics(GameTime gameTime)
        {
            if (!isMoving)
            {
                if (Velocity.X > 0)
                    Velocity -= Vector2.UnitX * speed;
                else if (Velocity.X < 0)
                    Velocity += Vector2.UnitX * speed;
                if (Math.Abs(Velocity.X) < speed)
                    Velocity = new Vector2(0, Velocity.Y);
            }
            float xClamp = MathHelper.Clamp(Velocity.X, -xMaxSpeed, xMaxSpeed);
            float yClamp = MathHelper.Clamp(Velocity.Y + terminalSpeed, -yMaxSpeed, yMaxSpeed);
            Velocity = new Vector2(xClamp, yClamp);
            Position += Velocity;
        }

        private void HandleMovement()
        {
            isMoving = false;

            if (keyboardState.IsKeyDown(Keys.D))
            {
                Velocity += Vector2.UnitX * speed;
                isMoving = !isMoving;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                Velocity -= Vector2.UnitX * speed;
                isMoving = !isMoving;
            }
        }

        private void HandleJumping(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W))
            {
                if (IsOnGround)
                {
                    IsJumping = true;
                    jumpTimer = jumpTimerDefault;
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);                  
                }
                else if (IsOnWall)
                {
                    IsJumping = true;
                    jumpTimer = jumpTimerDefault;

                    if (IsOnRightWall)
                        Velocity = new Vector2(-jumpSpeed, -jumpSpeed);
                    else
                        Velocity = new Vector2(jumpSpeed, -jumpSpeed);
                }
            }                
            else if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyDown(Keys.W) && jumpTimer > 0 && IsJumping)
            {
                if (jumpTimer < 0.1f)
                    Velocity = new Vector2(Velocity.X, -jumpSpeed / 2);
                else
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);
                jumpTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (((keyboardState.IsKeyUp(Keys.W) && lastState.IsKeyDown(Keys.W)) || jumpTimer < 0) && IsJumping)
            {
                Velocity = new Vector2(Velocity.X, 0);
                jumpTimer = jumpTimerDefault;
                IsJumping = false;
            }
        }

        private void HandleDucking()
        {
            if (keyboardState.IsKeyDown(Keys.S))
                IsDucking = true;
            else
                IsDucking = false;

            if (IsDucking)
            {
                Velocity = new Vector2(Velocity.X / 2, Velocity.Y);
                renderTexture = duckTexture;
                playerHeight = 32;

                if (keyboardState.IsKeyDown(Keys.S) && lastState.IsKeyUp(Keys.S))
                    Position = new Vector2(Position.X, Position.Y + 32);
            }
            else
            {
                if (lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyUp(Keys.S))
                {
                    Position = new Vector2(Position.X, Position.Y - 32);
                    renderTexture = idleTexture;
                    playerHeight = 64;
                }
            }
        }

        private void HandleWallSliding(GameTime gameTime)
        {
            if (IsOnWall && !IsOnGround && !IsJumping)
            {
                if ((slideTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
                {
                    terminalSpeed = 0;
                    Velocity = new Vector2(Velocity.X, 0);
                }
                else
                    terminalSpeed = terminalSpeedDefault / 6;
            }
            else
            {
                terminalSpeed = terminalSpeedDefault;
                slideTimer = slideTimerDefault;
            }
            wasOnWall = IsOnWall;
        }
    }
}
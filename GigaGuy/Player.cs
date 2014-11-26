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
        private const float slideTimerDefault = 0.1f;
        private float stickTimer;
        private const float stickTimerDefault = 1f;
        private float jumpTimer;
        private const float jumpTimerDefault = 0.4f;

        private float xMaxSpeed;
        private const float xMaxSpeedDefault = 7.5f;
        private float yMaxSpeed;
        private const float yMaxSpeedDefault = 17.5f;

        private bool isMoving;
        private bool isJumping;

        public bool IsDucking { get; private set; }
        public bool IsOnGround { get; set; }

        public bool IsOnWall { get; set; }
        public bool IsOnRightWall { get; set; }     
        private bool wasOnWall;
        private bool letGoOfWall;
        

        public Player()
        {
            Position = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);
            xMaxSpeed = xMaxSpeedDefault;
            yMaxSpeed = yMaxSpeedDefault;
            terminalSpeed = terminalSpeedDefault;
            slideTimer = slideTimerDefault;
            stickTimer = stickTimerDefault;
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
            HandleInput();
            HandleJumping(gameTime);
            HandleWallSliding(gameTime);
            HandlePhysics(gameTime);
            IsOnWall = false;
            IsOnGround = false;
            lastState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offSet)
        {
            spriteBatch.Draw(renderTexture, Position + offSet, Color.White);
        }

        private void HandlePhysics(GameTime gameTime)
        {
            if (!isMoving && !letGoOfWall)
            {
                if (Velocity.X > 0)
                    Velocity -= Vector2.UnitX * 0.5f * speed;
                else if (Velocity.X < 0)
                    Velocity += Vector2.UnitX * 0.5f * speed;
                if (Math.Abs(Velocity.X) < 0.5f * speed)
                    Velocity = new Vector2(0, Velocity.Y);
            }
            float xClamp = MathHelper.Clamp(Velocity.X, -xMaxSpeed, xMaxSpeed);
            float yClamp = MathHelper.Clamp(Velocity.Y + terminalSpeed, -yMaxSpeed, yMaxSpeed);
            Velocity = new Vector2(xClamp, yClamp);
            Position += Velocity;
        }

        private void HandleInput()
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

            if (keyboardState.IsKeyDown(Keys.S) || (lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyUp(Keys.S)))
            {
                HandleDucking();
            }

        }

        private void HandleJumping(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W))
            {
                if (IsOnGround)
                {
                    isJumping = true;
                    jumpTimer = jumpTimerDefault;
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);                  
                }
                else if (IsOnWall)
                {
                    isJumping = true;
                    jumpTimer = jumpTimerDefault;

                    if (IsOnRightWall)
                        Velocity = new Vector2(-jumpSpeed, -jumpSpeed);
                    else
                        Velocity = new Vector2(jumpSpeed, -jumpSpeed);
                }
            }                
            else if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyDown(Keys.W) && jumpTimer > 0 && isJumping)
            {
                if (jumpTimer < 0.1f)
                    Velocity = new Vector2(Velocity.X, -jumpSpeed / 2);
                else
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);
                jumpTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (((keyboardState.IsKeyUp(Keys.W) && lastState.IsKeyDown(Keys.W)) || jumpTimer < 0) && isJumping)
            {
                Velocity = new Vector2(Velocity.X, 0);
                jumpTimer = jumpTimerDefault;
                TerminateJump();
            }
        }

        private void HandleDucking()
        {
            if (keyboardState.IsKeyDown(Keys.S))
            {
                IsDucking = true;
                Velocity = new Vector2(Velocity.X / 1.5f, Velocity.Y);
                renderTexture = duckTexture;
                playerHeight = 32;

                if (keyboardState.IsKeyDown(Keys.S) && lastState.IsKeyUp(Keys.S))
                    Position = new Vector2(Position.X, Position.Y + 32);
            }
            else
            {
                if (lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyUp(Keys.S))
                {
                    IsDucking = false;
                    Position = new Vector2(Position.X, Position.Y - 32);
                    renderTexture = idleTexture;
                    playerHeight = 64;
                }
            }
        }

        private void HandleWallSliding(GameTime gameTime)
        {
            if (letGoOfWall && !isJumping)
            {
                if ((stickTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
                {
                    StickToWall();
                }
                else
                    letGoOfWall = false;
            }
            else if (IsOnWall && !IsOnGround && !isJumping)
            {
                if ((slideTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
                {
                    terminalSpeed = 0;
                    Velocity = new Vector2(Velocity.X, 0);
                }
                else
                    terminalSpeed = terminalSpeedDefault / 6;
            }      
            else if (wasOnWall && !IsOnWall && !isJumping && !letGoOfWall)
            {
                letGoOfWall = true;
                StickToWall();
                return;
            }
            else
            {
                terminalSpeed = terminalSpeedDefault;
                slideTimer = slideTimerDefault;
                stickTimer = stickTimerDefault;
                letGoOfWall = false;
            }
            wasOnWall = IsOnWall;
        }

        public void TerminateJump()
        {
            isJumping = false;
        }

        private void StickToWall()
        {
            if (IsOnWall && IsOnRightWall)
                Velocity = new Vector2(speed, Velocity.Y);
            else if (IsOnWall && !IsOnRightWall)
                Velocity = new Vector2(-speed, Velocity.Y);
        }
    }
}
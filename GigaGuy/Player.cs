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
      
        private Vector2 acceleration;

        private int playerWidth = 32;
        private int playerHeight = 64;

        private float speed = 90f;
        private float terminalSpeed = 45f;
        private float slideTimer = 0.5f;

        private float xMaxSpeed;
        private const float xMaxSpeedDefault = 600f;
        private float yMaxSpeed;
        private const float yMaxSpeedDefault = 1050f;

        private bool notMoving;
        public bool IsDucking { get; private set; }
        public bool IsOnGround { get; set; }

        public bool IsOnWall { get; set; }
        public bool IsOnRightWall { get; set; }
        private bool wasOnWall;
        private bool isJumping;

        public Player()
        {
            Position = new Vector2(0, 0);
            acceleration = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);
            xMaxSpeed = xMaxSpeedDefault;
            yMaxSpeed = yMaxSpeedDefault;
        }

        public void LoadContent(ContentManager Content)
        {
            idleTexture = Content.Load<Texture2D>("player");
            duckTexture = Content.Load<Texture2D>("playerDuck");
            renderTexture = idleTexture;
        }

        public void Update(GameTime gameTime)
        {
            HandleInput();
            HandleDucking();           
            HandlePhysics(gameTime);

            lastState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offSet)
        {
            spriteBatch.Draw(renderTexture, Position + offSet, Color.White);
        }

        private void HandlePhysics(GameTime gameTime)
        {
            HandleWallSliding(gameTime);

            if (notMoving)
            {
                if (Velocity.X > 0)
                    acceleration -= Vector2.UnitX * speed;
                else if (Velocity.X < 0)
                    acceleration += Vector2.UnitX * speed;
                if (Math.Abs(Velocity.X) < speed)
                    Velocity = new Vector2(0, Velocity.Y);
            }
            Velocity += acceleration;
            Velocity = new Vector2(
                MathHelper.Clamp(Velocity.X, -xMaxSpeed, xMaxSpeed),
                MathHelper.Clamp(Velocity.Y, -yMaxSpeed, yMaxSpeed));
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            acceleration = new Vector2(0, terminalSpeed);
        }

        private void HandleInput()
        {
            keyboardState = Keyboard.GetState();
            notMoving = true;

            if (keyboardState.IsKeyDown(Keys.D))
            {
                acceleration += Vector2.UnitX * speed;
                notMoving = false;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                acceleration -= Vector2.UnitX * speed;
                notMoving = false;
            }

            if (keyboardState.IsKeyDown(Keys.S))
                IsDucking = true;
            else
                IsDucking = false;

            if (keyboardState.IsKeyDown(Keys.W))
                Jump();
        }

        private void HandleDucking()
        {
            if (IsDucking)
            {
                xMaxSpeed = xMaxSpeedDefault / 2;
                renderTexture = duckTexture;
                playerHeight = 32;

                if (keyboardState.IsKeyDown(Keys.S) && lastState.IsKeyUp(Keys.S))
                    Position = new Vector2(Position.X, Position.Y + 32);
            }
            else
            {
                if (lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyUp(Keys.S))
                {
                    xMaxSpeed = xMaxSpeedDefault;
                    Position = new Vector2(Position.X, Position.Y - 32);
                    renderTexture = idleTexture;
                    playerHeight = 64;
                }
            }
        }

        private void Jump()
        {
            if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W))
            {
                if (IsOnGround)
                {
                        Velocity = new Vector2(Velocity.X, -yMaxSpeed);
                }
                else if (IsOnWall)
                {
                    yMaxSpeed = yMaxSpeedDefault;

                    if (IsOnRightWall)
                        Velocity = new Vector2(-xMaxSpeed, -yMaxSpeed);
                    else
                        Velocity = new Vector2(xMaxSpeed, -yMaxSpeed);
                }
            }
        }

        private void HandleWallSliding(GameTime gameTime)
        {
            if (IsOnWall && !IsOnGround)
            {
                yMaxSpeed = yMaxSpeedDefault / 4;
                StickToWall(gameTime);

                if (IsOnRightWall)
                    Velocity = new Vector2(Velocity.X, Velocity.Y);
                else
                    Velocity = new Vector2(Velocity.X, Velocity.Y);
            }
            else
            {
                yMaxSpeed = yMaxSpeedDefault;
                slideTimer = 1f;
            }

            wasOnWall = IsOnWall;
        }

        // Refactor into OnWallHit, call from collision code.
        // Right now, wall sliding is dependent on continuous collision with the walls. Make it not so.
        private void StickToWall(GameTime gameTime) 
        {            
            if ((slideTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
            {
                yMaxSpeed = 0;
                Velocity = new Vector2(Velocity.X, yMaxSpeed);
            }
            else
                yMaxSpeed = yMaxSpeedDefault / 4;
        }
    }
}

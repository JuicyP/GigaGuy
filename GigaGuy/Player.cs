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

        private float speed;
        private const float speedDefault = 2.0f;
        private float jumpSpeed = 10.5f;
        private float terminalSpeed;
        private const float terminalSpeedDefault = 0.75f;

        private float slideTimer;
        private const float slideTimerDefault = 0.1f;
        private float stickTimer;
        private const float stickTimerDefault = 0.5f;
        private float jumpTimer;
        private const float jumpTimerDefault = 0.4f;
        private float groundTimer;  // Sets IsOnGround to true if left platform recently
        private const float groundTimerDefault = 0.05f;
        private float jumpOnCollisionTimer;
        private const float jumpOnCollisionTimerDefault = 0.05f;

        private float xMaxSpeed;
        private const float xMaxSpeedDefault = 7.5f;
        private float yMaxSpeed;
        private const float yMaxSpeedDefault = 17.5f;

        private bool isMoving;
        public bool IsJumping { get; set; }
        private bool isWallJumping = false;
        public bool IsDucking { get; private set; }
        public bool IsOnGround { get; set; }
        public bool IsOnWall { get; set; }
        public bool IsOnRightWall { get; set; }
        public bool IsArc { get; set; }
        private bool stuckToWall;
        public bool IsOnSlope { get; set; }
        public SlopeType SlopeType { get; set; }


        public Player()
        {
            Position = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);
            xMaxSpeed = xMaxSpeedDefault;
            yMaxSpeed = yMaxSpeedDefault;
            speed = speedDefault;
            terminalSpeed = terminalSpeedDefault;
            slideTimer = slideTimerDefault;
            jumpTimer = jumpTimerDefault;
            stickTimer = 0;
            groundTimer = 0;
            jumpOnCollisionTimer = 0;
        }

        public void LoadContent(ContentManager Content)
        {
            idleTexture = Content.Load<Texture2D>("Player/player");
            duckTexture = Content.Load<Texture2D>("Player/playerDuck");
            renderTexture = idleTexture;
        }

        public void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            PlatformDelay(gameTime);
            SetSpeed();
            HandleInput();
            HandleWallSliding(gameTime);
            HandleJumping(gameTime);
            HandlePhysics(gameTime);
            IsOnWall = false;
            IsOnGround = false;
            lastState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offSet)
        {
            spriteBatch.Draw(renderTexture, Position + offSet, Color.White);
        }

        /// <summary>
        /// Changes player position with player velocity.
        /// If the player isn't moving, velocity approaches 0.
        /// Clamps velocity based on max speed on the X and Y axes.
        /// If the player is wall sliding, player movement is limited towards the wall.
        /// If the player is on a slope, player Y position is adjusted based on X velocity and slope angle.
        /// </summary>
        private void HandlePhysics(GameTime gameTime)
        {
            if (!isMoving)
            {
                if (Velocity.X > 0)
                    Velocity -= Vector2.UnitX * 0.5f * speed;
                else if (Velocity.X < 0)
                    Velocity += Vector2.UnitX * 0.5f * speed;
                if (Math.Abs(Velocity.X) < 0.5f * speed)
                    Velocity = new Vector2(0, Velocity.Y);
            }
            float xClamp;
            float yClamp;
            if (IsOnWall)
            {
                if (IsOnRightWall)
                    xClamp = MathHelper.Clamp(Velocity.X, 0, xMaxSpeed);
                else
                    xClamp = MathHelper.Clamp(Velocity.X, -xMaxSpeed, 0);
                yClamp = MathHelper.Clamp(Velocity.Y + terminalSpeed, -yMaxSpeed, yMaxSpeed);
            }
            else
            {
                xClamp = MathHelper.Clamp(Velocity.X, -xMaxSpeed, xMaxSpeed);
                yClamp = MathHelper.Clamp(Velocity.Y + terminalSpeed, -yMaxSpeed, yMaxSpeed);
            }
            Velocity = new Vector2(xClamp, yClamp);

            if (IsOnSlope && !IsJumping)
            {
                float slopeDisplacement = 0;
                if (SlopeType == SlopeType._45R)
                    slopeDisplacement = -Velocity.X;
                else if (SlopeType == SlopeType._45L)
                    slopeDisplacement = Velocity.X;
                else if (SlopeType == SlopeType._2251R || SlopeType == SlopeType._2252R)
                    slopeDisplacement = -Velocity.X / 2;
                else if (SlopeType == SlopeType._2251L || SlopeType == SlopeType._2252L)
                    slopeDisplacement = Velocity.X / 2;
                else if (SlopeType == SlopeType._11251R || SlopeType == SlopeType._11252R || SlopeType == SlopeType._11253R || SlopeType == SlopeType._11254R)
                    slopeDisplacement = -Velocity.X / 4;
                else if (SlopeType == SlopeType._11251L || SlopeType == SlopeType._11252L || SlopeType == SlopeType._11253L || SlopeType == SlopeType._11254L)
                    slopeDisplacement = Velocity.X / 4;

                Position += Vector2.UnitY * slopeDisplacement;
            }
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

        /// <summary>
        /// Jumping is performed by adding minute amounts of Y velocity each frame the jump button is held down.
        /// When the jump is nearing its end, the added velocity is tapered down to make the top of the jump have an arc.
        /// Also stores jump action when pressing jump before hitting the ground.
        /// </summary>
        private void HandleJumping(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W) || jumpOnCollisionTimer > 0)
            {
                if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W))
                    jumpOnCollisionTimer = jumpOnCollisionTimerDefault; // Stores jump action

                jumpOnCollisionTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (IsOnGround)
                {
                    IsJumping = true;
                    isWallJumping = false;
                    jumpTimer = jumpTimerDefault;
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);
                    jumpOnCollisionTimer = 0;
                }
                else if (IsOnWall && keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W)) // Refactor everything
                {
                    IsJumping = true;
                    isWallJumping = true;
                    jumpTimer = jumpTimerDefault;
                    IsOnWall = false;
                    stuckToWall = false;
                    jumpOnCollisionTimer = 0;

                    if (IsOnRightWall)
                        Velocity = new Vector2(-jumpSpeed, -jumpSpeed);
                    else
                        Velocity = new Vector2(jumpSpeed, -jumpSpeed);
                }
            }
            else if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyDown(Keys.W) && jumpTimer > 0 && IsJumping)
            {
                if (jumpTimer > 0.375f && isWallJumping)
                {

                    if (IsOnRightWall)
                        Velocity += Vector2.UnitX * -jumpSpeed;
                    else
                        Velocity += Vector2.UnitX * jumpSpeed;
                }

                if (jumpTimer < 0.1f)
                {
                    IsArc = true;
                    Velocity = new Vector2(Velocity.X, -jumpSpeed / 2);
                }
                else
                    Velocity = new Vector2(Velocity.X, -jumpSpeed);
                jumpTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (((keyboardState.IsKeyUp(Keys.W) && lastState.IsKeyDown(Keys.W)) || jumpTimer < 0) && IsJumping)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y / 3);
                jumpTimer = jumpTimerDefault;
                IsJumping = false;
            }
            else
                IsArc = false;
        }

        // Slows down horizontal movement and makes the player shorter.
        private void HandleDucking()
        {
            if (keyboardState.IsKeyDown(Keys.S))
            {
                IsDucking = true;
                Velocity = new Vector2(Velocity.X / 1.5f, Velocity.Y);
                renderTexture = duckTexture;
                playerHeight = 32;

                // Position needs to be adjusted.
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

        /// <summary>
        /// Sticks the player to the wall, so that the player can only move away or fall down by wall jumping or waiting for the timer to expire.
        /// The sticking is executed by constantly bumping the player into the wall to cause a collision which sets the bool IsOnWall to true.
        /// </summary>
        private void HandleWallSliding(GameTime gameTime)
        {
            if (IsOnWall && !IsOnGround && !IsJumping)
            {
                if ((IsOnRightWall && (Velocity.X > 0)) || (!IsOnRightWall && (Velocity.X < 0)) || !stuckToWall)
                    stickTimer = stickTimerDefault;

                if ((stickTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
                {
                    stuckToWall = true;
                    if ((slideTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds) > 0)
                    {
                        terminalSpeed = 0;
                        Velocity = new Vector2(Velocity.X, 0);
                    }
                    else
                        terminalSpeed = terminalSpeedDefault / 3;
                    if (IsOnRightWall)
                        Position += Vector2.UnitX * 0.1f;
                    else
                        Position -= Vector2.UnitX * 0.1f;
                }
            }
            else
            {
                if (stuckToWall)
                {
                    stuckToWall = false;
                    if (IsOnRightWall)
                        Position -= Vector2.UnitX * 0.1f;
                    else if (!IsOnRightWall)
                        Position += Vector2.UnitX * 0.1f;
                }
                terminalSpeed = terminalSpeedDefault;
                slideTimer = slideTimerDefault;
                stickTimer = 0;
            }
        }

        // Sets IsOnGround to true for a few milliseconds after leaving a platform.
        private void PlatformDelay(GameTime gameTime)
        {
            if (IsOnGround)
            {
                groundTimer = groundTimerDefault;
            }
            else if (IsJumping)
            {
                IsOnGround = false;
                groundTimer = 0;
            }
            else if (groundTimer > 0)
            {
                IsOnGround = true;
                groundTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private void SetSpeed()
        {
            if (IsOnGround)
            {
                speed = speedDefault;
            }
            else
            {
                speed = speedDefault / 1.5f;
            }
        }
    }
}
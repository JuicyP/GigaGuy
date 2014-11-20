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

        private float speed = 1.5f;
        private float terminalSpeed = 0.75f;

        private float xMaxSpeed = 10f;
        private float yMaxSpeed = 17.5f;

        private bool notMoving;
        private bool isDucking;
        public bool IsOnGround { get; set; }

        public Player()
        {
            Position = new Vector2(0, 0);
            acceleration = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);            
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
            HandlePhysics();

            lastState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(renderTexture, Position, Color.White);
        }

        private void HandlePhysics()
        {
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
            Position += Velocity;
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
                isDucking = true;
            else
                isDucking = false;

            if (keyboardState.IsKeyDown(Keys.W))
                Jump();
        }

        private void HandleDucking()
        {
            if (isDucking)
            {
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

        private void Jump()
        {
            if (IsOnGround)
            {
                if (keyboardState.IsKeyDown(Keys.W) && lastState.IsKeyUp(Keys.W))
                {
                    Velocity = new Vector2(Velocity.X, Velocity.Y - 17.5f);
                }
            }
        }
    }
}

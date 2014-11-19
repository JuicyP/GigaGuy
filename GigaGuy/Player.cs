using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GigaGuy
{
    class Player
    {
        private Texture2D texture;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle(
                    (int)Position.X, (int)Position.Y,
                    playerWidth, playerHeight);
            }
        }
      
        private Vector2 acceleration;
        private Vector2 friction;

        private int playerWidth = 32;
        private int playerHeight = 64;

        private float accelerationFactor = 0.75f;
        private float frictionFactor = 1.05f;

        public Player()
        {
            Position = new Vector2(0, 0);
            acceleration = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);            
            friction = new Vector2(frictionFactor, frictionFactor);
        }

        public void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("player");
        }

        public void Update(GameTime gameTime)
        {
            Move();    
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }

        private void Move()
        {
            Position += Velocity;
            Velocity += acceleration;
            Velocity /= friction;
            acceleration = Vector2.Zero;
        }

        public void MoveRight() { acceleration += Vector2.UnitX * accelerationFactor; }
        public void MoveLeft() { acceleration -= Vector2.UnitX * accelerationFactor; }
        public void MoveDown() { acceleration += Vector2.UnitY * accelerationFactor; }
        public void MoveUp() { acceleration -= Vector2.UnitY * accelerationFactor; }
    }
}

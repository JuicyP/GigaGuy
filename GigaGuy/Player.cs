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
        public Vector2 Acceleration { get; set; }

        private Vector2 velocity;
        private Vector2 friction;

        private float accelerationFactor = 0.75f;
        private float frictionFactor = 1.05f;

        public Player()
        {
            Position = new Vector2(0, 0);
            Acceleration = new Vector2(0, 0);
            velocity = new Vector2(0, 0);            
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
            Position += velocity;
            velocity += Acceleration;
            velocity /= friction;
            Acceleration = Vector2.Zero;
        }

        public void MoveRight() { Acceleration += Vector2.UnitX * accelerationFactor; }
        public void MoveLeft() { Acceleration -= Vector2.UnitX * accelerationFactor; }
        public void MoveDown() { Acceleration += Vector2.UnitY * accelerationFactor; }
        public void MoveUp() { Acceleration -= Vector2.UnitY * accelerationFactor; }
    }
}

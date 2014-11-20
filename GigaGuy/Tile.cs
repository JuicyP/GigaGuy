using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GigaGuy
{
    class Tile
    {
        public Texture2D Texture { get; set; }
        public RectangleF Hitbox { get; set; }
        public Tile(RectangleF hitbox)
        {
            Hitbox = hitbox;
        }

        public void Update()
        {
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offSet)
        {
            spriteBatch.Draw(Texture, new Vector2(Hitbox.X, Hitbox.Y) + offSet, Color.White);
        }
    }
}
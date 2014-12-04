using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GigaGuy
{
    class Camera
    {
        private int screenWidth = 1280; // TODO: Shouldn't hardcode. Will fix later
        private int screenHeight = 720;

        public Camera() { }

        /// <summary>
        /// Returns a Vector2 used for offsetting all drawing in the Level-class, based on the position of the player.
        /// </summary>
        public Vector2 CalculateOffSet(Player player)
        {
            if (player.IsDucking)
            {
                return new Vector2(
                    screenWidth / 2 - player.Hitbox.Width / 2 - player.Hitbox.X,
                    screenHeight / 2 - player.Hitbox.Y);
            }
            else
            {
                return new Vector2(
                    screenWidth / 2 - player.Hitbox.Width / 2 - player.Hitbox.X,
                    screenHeight / 2 - player.Hitbox.Height / 2 - player.Hitbox.Y);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GigaGuy
{
    public enum SlopeType { _45Right, _45Left };

    class Slope : Tile
    {
        public SlopeType slopeType { get; private set; }

        public Slope(Texture2D texture, RectangleF hitbox, SlopeType slopeType)
            : base(texture, hitbox)
        {
            this.slopeType = slopeType;
        }
    }
}

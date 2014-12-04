using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GigaGuy
{
    public enum SlopeType { _45R, _45L, _2251R, _2252R, _2251L, _2252L, _11251R, _11252R, _11253R, _11254R, _11251L, _11252L, _11253L, _11254L };

    class Slope : Tile
    {
        public SlopeType SlopeType { get; private set; }

        public Slope(Texture2D texture, RectangleF hitbox, SlopeType slopeType)
            : base(texture, hitbox)
        {
            this.SlopeType = slopeType;
        }
    }
}

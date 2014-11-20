using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GigaGuy
{
    struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float Right { get { return X + Width; } }
        public float Left { get { return X; } }
        public float Bottom { get { return Y + Height; } }
        public float Top { get { return Y; } }

        public RectangleF(float x, float y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(RectangleF rectangle)
        {
            if (rectangle.X + rectangle.Width > X && rectangle.X < X + Width)
                if (rectangle.Y + rectangle.Height > Y && rectangle.Y < Y + Height)
                    return true;

            return false;
        }
    }
}

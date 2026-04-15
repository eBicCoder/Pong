using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Core
{
    public class Player
    {
        public Player(int width, int height, float speed, int x, float y = 0)
        {
            Width = width;
            Height = height;
            Speed = speed;
            X = x;
            Y = y;
        }

        public int Width { get; init; }
        public int Height { get; init; }
        public float Speed { get; set; }
        public int X { get; init; }
        public float Y { get; set; }
        public (float, float) ULCorner => (X - Width / 2f, Y + Height / 2f);
        public (float, float) URCorner => (X + Width / 2f, Y + Height / 2f);
        public (float, float) BLCorner => (X - Width / 2f, Y - Height / 2f);
        public (float, float) BRCorner => (X + Width / 2f, Y - Height / 2f);
    }
}

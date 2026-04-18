using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Core
{
    public class Point
    {
        public Point (float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
    }
}

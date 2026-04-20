using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Core
{
    public class Button
    {
        public Button()
        {}
        public float X {  get; set; }
        public float Y { get; set; }
        public Point Location => new Point(X, Y);
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visible { get; set; }
    }
}

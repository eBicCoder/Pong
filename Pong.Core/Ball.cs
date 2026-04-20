namespace Pong.Core
{
    public class Ball
    {
        public Ball(int size, float direction, float initialSpeed = 500)
        {
            Size = size;
            Direction = direction;
            InitialSpeed = initialSpeed;
            Speed = initialSpeed;
        }

        public int Size { get; init; }
        public float Direction { get; set; }
        public float InitialSpeed { get; init; }
        public float Speed { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public Point ULCorner => new Point(X - Size / 2f, Y + Size / 2f);
        public Point URCorner => new Point(X + Size / 2f, Y + Size / 2f);
        public Point BLCorner => new Point(X - Size / 2f, Y - Size / 2f);
        public Point BRCorner => new Point(X + Size / 2f, Y - Size / 2f);
    }
}

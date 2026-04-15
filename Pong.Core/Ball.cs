namespace Pong.Core
{
    public class Ball
    {
        public Ball(int size, float direction, float initialSpeed = 500, int x = 0, int y = 0)
        {
            Size = size;
            Direction = direction;
            InitialSpeed = initialSpeed;
            Speed = initialSpeed;
            X = x;
            Y = y;
        }

        public int Size { get; init; }
        public float Direction { get; set; }
        public float InitialSpeed { get; init; }
        public float Speed { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public (float, float) ULCorner => (X - Size / 2f, Y + Size / 2f);
        public (float, float) URCorner => (X + Size / 2f, Y + Size / 2f);
        public (float, float) BLCorner => (X - Size / 2f, Y - Size / 2f);
        public (float, float) BRCorner => (X + Size / 2f, Y - Size / 2f);
    }
}

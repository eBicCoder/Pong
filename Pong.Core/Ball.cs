namespace Pong.Core
{
    public class Ball
    {
        public Ball(uint size, float direction, float initialSpeed, float maxSpeed, uint acceleration, float maxAngle)
        {
            Size = size;
            Direction = direction;
            InitialSpeed = initialSpeed;
            Speed = initialSpeed;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            MaxAngle = maxAngle;
        }

        public uint Size { get; init; }
        public float Direction { get; set; }
        public float InitialSpeed { get; init; }
        public float MaxSpeed { get; init; }
        public uint Acceleration { get; init; }
        public float MaxAngle { get; init; }
        public float Speed { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public Point ULCorner => new Point(X - Size / 2f, Y + Size / 2f);
        public Point URCorner => new Point(X + Size / 2f, Y + Size / 2f);
        public Point BLCorner => new Point(X - Size / 2f, Y - Size / 2f);
        public Point BRCorner => new Point(X + Size / 2f, Y - Size / 2f);

        /// <summary>
        /// Resets balls position to the center of the board and its speed to initial speed (used after a point is scored)
        /// </summary>
        internal void ResetBall()
        {
            X = 0;
            Y = 0;
            Speed = InitialSpeed;
        }
        /// <summary>
        /// Picks a random direction at the start of the round that is going towards the player that just got scored on
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public float NewRoundBallDirection(Player player)
        {
            float direction;
            if (player.X < 0)
            {
                direction = MathF.PI + MaxAngle - RandomBallDirection();
            }

            else if (player.X > 0)
            {
                direction = MaxAngle - RandomBallDirection();
            }

            else
                throw new Exception("Player isnt on the board");

            return direction;
        }
        /// <summary>
        /// Sets random ball direction between 0 and max angle
        /// </summary>
        /// <returns></returns>
        public float RandomBallDirection()
        {
            return 2 * Board.RND.NextSingle() * MaxAngle;
        }
    }
}

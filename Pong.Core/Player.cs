using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Pong.Core.Board;

namespace Pong.Core
{
    public class Player
    {
        public Player(string name, int width, int height, PlayerType playerType, float speed, int x, float y = 0)
        {
            Name = name;
            Width = width;
            Height = height;
            Type = playerType;
            Speed = speed;
            X = x;
            Y = y;
        }

        public string Name { get; private set; }
        public int Width { get; init; }
        public int Height { get; init; }
        public PlayerType Type { get; private set; }
        public float Speed { get; set; }
        public int X { get; init; }
        public float Y { get; set; }
        public Point ULCorner => new Point(X - Width / 2f, Y + Height / 2f);
        public Point URCorner => new Point(X + Width / 2f, Y + Height / 2f);
        public Point BLCorner => new Point(X - Width / 2f, Y - Height / 2f);
        public Point BRCorner => new Point(X + Width / 2f, Y - Height / 2f);
        public int Score = 0;

        internal static bool canMove = true; // stops players from moving
        public void ChangePos(Board board, double deltaTime, Direction direction)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Board.BoardState state = board.State;

            if ((state != BoardState.Playing && 
                state != BoardState.TempPause) ||
                !canMove)
                return;
            switch (direction)
            {
                case Direction.Up:
                    float yUp = (float)(Y + Speed * deltaTime);
                    if (yUp + Height / 2 >= top)
                        Y = top - Height / 2;
                    else
                        Y = yUp;
                    break;

                case Direction.Down:
                    float yDown = (float)(Y - Speed * deltaTime);
                    if (yDown - Height / 2 <= bottom)
                        Y = bottom + Height / 2;
                    else
                        Y = yDown;
                    break;

                case Direction.None:
                    break;

                default:
                    throw new Exception("Unhandled enum Direction");
            }
        }
        
        private double TimeSinceChangeDirection = 0;
        public enum Direction
        { Up, Down, None }
        private Direction LastDirection;

        public enum PlayerType
        { Human, Easy , Normal, Hard, Impossible }
        // easy - tries to match current ball Y
        // normal - normal bot 
        // hard - tries to predict where the ball will be
        // computes where the ball will be

        public void BotPlay(Board board, bool pointScored, double deltaTime)
        {
            switch (Type)
            {
                case PlayerType.Human:
                    throw new Exception("Human player cant call bot");

                case PlayerType.Easy:
                    EasyBot(board, pointScored, deltaTime);
                    break;

                case PlayerType.Normal:
                    NormalBot(board, pointScored, deltaTime);
                    break;

                case PlayerType.Hard:
                    HardBot(board, pointScored, deltaTime);
                    break;

                case PlayerType.Impossible:
                    ImpossibleBot(board, pointScored, deltaTime);
                    break;

                default:
                    throw new Exception("Unhandled enum PlayerType");
            }
        }

        private readonly double EasyReactionTime = 0.3;
        private void EasyBot(Board board, bool pointScored, double deltaTime)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Ball ball = board.Ball;

            TimeSinceChangeDirection += deltaTime;

            float vx = ball.Speed * MathF.Cos(ball.Direction);
            float vy = ball.Speed * MathF.Sin(ball.Direction);

            float t = (X - ball.X) / vx;

            float targetY = ball.Y;

            float stopZone = Height * 0.2f;
            float startZone = Height * 0.75f;

            float diff = targetY - Y;

            Direction direction = LastDirection;

            if (Math.Sign(vx) == Math.Sign(X) &&
                TimeSinceChangeDirection >= EasyReactionTime &&
                !pointScored)
            {
                if (diff > startZone)
                {
                    direction = Direction.Up;
                    TimeSinceChangeDirection = 0;
                }
                else if (diff < -startZone)
                {
                    direction = Direction.Down;
                    TimeSinceChangeDirection = 0;
                }

                
            }
            else if (pointScored)
            {
                if (Y > stopZone)
                    direction = Direction.Down;
                else if (Y < -stopZone)
                    direction = Direction.Up;
                else
                    direction = Direction.None;
            }
            else
            {
                if (MathF.Abs(diff) < stopZone)
                {
                    direction = Direction.None;
                }
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }

        private readonly double NormalReactionTime = 0.2;
        private void NormalBot(Board board, bool pointScored, double deltaTime)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Ball ball = board.Ball;

            TimeSinceChangeDirection += deltaTime;

            float vx = ball.Speed * MathF.Cos(ball.Direction);
            float vy = ball.Speed * MathF.Sin(ball.Direction);

            float t = 0.2f;

            float targetY = ball.Y + vy * t;

            float stopZone = Height * 0.2f;
            float startZone = Height * 0.75f;

            Direction direction = LastDirection;

            if (Math.Sign(vx) == Math.Sign(X) &&
                TimeSinceChangeDirection >= NormalReactionTime &&
                !pointScored)
            {
                float diff = targetY - Y;


                if (diff > startZone)
                {
                    direction = Direction.Up;
                    TimeSinceChangeDirection = 0;
                }
                else if (diff < -startZone)
                {
                    direction = Direction.Down;
                    TimeSinceChangeDirection = 0;
                }

                else
                {
                    if (MathF.Abs(diff) < stopZone)
                    {
                        direction = Direction.None;
                    }
                }
            }
            else if (pointScored)
            {
                if (Y > stopZone)
                    direction = Direction.Down;
                else if (Y < -stopZone)
                    direction = Direction.Up;
                else
                    direction = Direction.None;
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }

        private readonly double HardReactionTime = 0.1;
        private void HardBot(Board board, bool pointScored, double deltaTime)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Ball ball = board.Ball;

            TimeSinceChangeDirection += deltaTime;

            float vx = ball.Speed * MathF.Cos(ball.Direction);
            float vy = ball.Speed * MathF.Sin(ball.Direction);

            

            float stopZone = Height * 0.2f;
            float startZone = Height * 0.45f;

            Direction direction = LastDirection;

            if (Math.Sign(vx) == Math.Sign(X) &&
                TimeSinceChangeDirection >= HardReactionTime &&
                !pointScored)
            {
                float t = (X - ball.X) / vx;

                float xIntersect = MathF.Abs((X - ball.X)) / MathF.Abs(vx) * ((top - ball.Y) / MathF.Abs(vy));

                do
                {
                    vy = -vy;
                    xIntersect = MathF.Abs((X - xIntersect)) / MathF.Abs(vx) * ((board.Height) / MathF.Abs(vy));
                } while (MathF.Abs(xIntersect) < MathF.Abs(X));

                float targetY = ball.Y + vy * t;

                float diff = targetY - Y;


                if (diff > startZone)
                {
                    direction = Direction.Up;
                    TimeSinceChangeDirection = 0;
                }
                else if (diff < -startZone)
                {
                    direction = Direction.Down;
                    TimeSinceChangeDirection = 0;
                }

                else
                {
                    if (MathF.Abs(diff) < stopZone)
                    {
                        direction = Direction.None;
                    }
                }
            }
            else if (Math.Sign(vx) != Math.Sign(X) || pointScored)
            {
                if (Y > stopZone)
                    direction = Direction.Down;
                else if (Y < -stopZone)
                    direction = Direction.Up;
                else
                    direction = Direction.None;
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }

        private void ImpossibleBot(Board board, bool pointScored, double deltaTime)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Ball ball = board.Ball;

            TimeSinceChangeDirection += deltaTime;

            float vx = ball.Speed * MathF.Cos(ball.Direction);
            float vy = ball.Speed * MathF.Sin(ball.Direction);

            float t = (X - ball.X) / vx;

            float targetY = ball.Y + vy * t;

            float stopZone = Height * 0.2f;
            float startZone = Height * 0.45f;

            Direction direction = LastDirection;

            if (Math.Sign(vx) == Math.Sign(X) &&
                TimeSinceChangeDirection >= HardReactionTime &&
                !pointScored)
            {
                float diff = targetY - Y;


                if (diff > startZone)
                {
                    direction = Direction.Up;
                    TimeSinceChangeDirection = 0;
                }
                else if (diff < -startZone)
                {
                    direction = Direction.Down;
                    TimeSinceChangeDirection = 0;
                }

                else
                {
                    if (MathF.Abs(diff) < stopZone)
                    {
                        direction = Direction.None;
                    }
                }
            }
            else if (Math.Sign(vx) != Math.Sign(X) || pointScored)
            {
                if (Y > stopZone)
                    direction = Direction.Down;
                else if (Y < -stopZone)
                    direction = Direction.Up;
                else
                    direction = Direction.None;
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }
    }
}

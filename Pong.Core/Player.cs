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
        public Player(string name, int width, int height, PlayerType playerType, uint speed, int x, float y = 0)
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
        public uint Speed { get; set; }
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
                !canMove) // only allow movement during playing and temp pause (after scoring)
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

        public enum PlayerType
        { Human, Easy , Normal, Hard, Impossible }
        // easy - tries to match current ball Y
        // normal - normal bot 
        // hard - tries to predict where the ball will be
        // impossible - computes where the ball will be

        private Direction LastDirection;
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
        // easy bot just tries to match the current Y of the ball
        // it has a reaction time and a stop zone where it won't move
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
        // normal bot tries to predict where the ball will be in 0.2 seconds
        // it has a reaction time and a stop zone where it won't move
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
            else if (Math.Sign(vx) != Math.Sign(X))
            {
                direction = Direction.None;
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }

        private readonly double HardReactionTime = 0.1;
        // hard bot tries to predict where the ball will be, but cant predict board boundaries
        // it has a reaction time and a stop zone where it won't move
        // it also tries to move to the center when the ball is moving away or a point is scored
        private void HardBot(Board board, bool pointScored, double deltaTime)
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

        private int ImpossibleReactionTime = 0;
        private bool DecidedOffset = false;
        private float RandomOffset = 0;
        // impossible bot can predict perfectly where the ball will be
        // it has a  no reaction time and a stop zone where it won't move
        // it also tries to move to the center when the ball is moving away or a point is scored
        // he has random offset so he makes more interesting shots
        // he should truly live up to his name
        private void ImpossibleBot(Board board, bool pointScored, double deltaTime)
        {
            float top = board.Top;
            float bottom = board.Bottom;
            Ball ball = board.Ball;

            TimeSinceChangeDirection += deltaTime;

            float vx = ball.Speed * MathF.Cos(ball.Direction);
            float vy = ball.Speed * MathF.Sin(ball.Direction);

            float stopZone = Height * 0.2f;
            float startZone = Height * 0.1f;

            Direction direction = LastDirection;

            if (Math.Sign(vx) == Math.Sign(X) &&
                TimeSinceChangeDirection >= ImpossibleReactionTime &&
                !pointScored)
            {
                float boardHeight = board.Height;

                var t = (X - ball.X) / vx; // computes how much time the ball takes to reach the bot's X coordinate

                float predictedY = ball.Y + vy * t; // computes where the ball will be in that time if it travelled in a straight line

                float period = 2 * boardHeight; // the ball bounces between top and bottom
                                                // so we can imagine that it travels in a straight line in a space where the top and bottom are connected
                                                // so the period of the ball's movement is 2 times the board height
                float y = (predictedY - bottom) % period;

                if (y < 0) 
                    y += period; // if the predictedY is below the bottom, we need to add the period to get the correct position in the connected space

                if (y > boardHeight) // if the predictedY is above the top, we need to subtract the period to get the correct position in the connected space
                    y = period - y;

                float targetY = bottom + y;
                if (!DecidedOffset) // we need to decide random offset only once to stop bot from shaking
                {
                    RandomOffset = Height * 0.9f * (0.5f - Board.RND.NextSingle());
                    DecidedOffset = true;
                }

                float diff = targetY - Y + RandomOffset;

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
                    direction = Direction.None; // stops moving when it reaches the predicted point with offset, so it doesn't shake
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

                DecidedOffset = false; // if the ball is moving away or a point is scored we unlock randomOffset
            }

            ChangePos(board, deltaTime, direction);
            LastDirection = direction;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Core
{
    public class Board
    {
        public Random RND = new Random();
        public Board(int height, Player player1, Player player2, Ball ball)
        {
            Height = height;
            Player1 = player1;
            Player2 = player2;
            Ball = ball;
            BorderLength = MathF.Abs(player1.X - player2.X) * 1.2f;
            State = GameState.GoalPause;
        }

        public int Height { get; init; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Ball Ball { get; set; }
        public float BorderLength { get; private set; }
        public GameState State { get; set; }

        public enum Direction
        { Up, Down }

        public enum GameState
        { Playing, GoalPause, Paused, MainMenu }

        private bool _canMove = true;
        public void ChangePlayerPos(Player player, double deltaTime, Direction direction)
        {
            if (State == GameState.Paused ||
                State == GameState.MainMenu ||
                !_canMove)
                return;

            switch (direction)
            {
                case Direction.Up:
                    float yUp = (float)(player.Y + player.Speed * deltaTime);
                    if (yUp + player.Height / 2 >= Height / 2)
                        player.Y = (Height - player.Height) / 2;
                    else
                        player.Y = yUp;
                    break;

                case Direction.Down:
                    float yDown = (float)(player.Y - player.Speed * deltaTime);
                    if (yDown - player.Height / 2 <= -Height / 2)
                        player.Y = (-Height + player.Height) / 2;
                    else
                        player.Y = yDown;
                    break;

                default:
                    throw new Exception("Unhandled direction");
            }
        }
        private GameState _previousState;
        public void ChangeState()
        {
            if (State == Board.GameState.Playing ||
                State == Board.GameState.GoalPause)
            {
                _previousState = State;
                State = Board.GameState.Paused;
            }

            else if (State == Board.GameState.Paused)
            {
                goalPauseElapsed = 0;
                State = Board.GameState.GoalPause;

                if (_previousState == GameState.Playing)
                    _canMove = false;

                else if (_previousState == GameState.GoalPause)
                    _canMove = true;

                _previousState = State;
            }
        }

        public void ResizePause()
        {
            if (State == Board.GameState.Playing ||
                State == Board.GameState.GoalPause)
            {
                _previousState = State;
                State = Board.GameState.Paused;
            }
        }

        public readonly int goalPause = 3;
        public double goalPauseElapsed = 0;

        public readonly int MaxBallSpeed = 2000;
        public bool Tick(double deltaTime)
        {
            //Debug.WriteLineIf(deltaTime > 0.0167f, deltaTime);
            switch (State)
            {
                case GameState.MainMenu:
                    return false;

                case GameState.Paused:
                    return false;

                case GameState.GoalPause:
                    goalPauseElapsed += deltaTime;
                    if (goalPauseElapsed >= goalPause)
                    {
                        State = GameState.Playing;
                        goalPauseElapsed = 0;
                        _canMove = true;
                    }
                    return false;

                case GameState.Playing:
                    bool pointScored = false;

                    float vX = (float)(MathF.Cos(Ball.Direction) * Ball.Speed * deltaTime);
                    float vY = (float)(MathF.Sin(Ball.Direction) * Ball.Speed * deltaTime);

                    float ballX = Ball.X + vX;
                    float ballY = Ball.Y + vY;

                    if (ballX - Ball.Size / 2 <= Player1.X + Player1.Width / 2 &&
                        vX < 0 &&
                        ballX >= Player1.X)
                    {
                        if (ballY - Ball.Size / 2 <= Player1.URCorner.Y &&
                            ballY + Ball.Size / 2 >= Player1.BRCorner.Y)
                        {
                            if (Ball.Speed < MaxBallSpeed)
                                Ball.Speed = (float)(Ball.Speed + 100);

                            ballX = Player1.X + Player1.Width / 2 + Ball.Size / 2;

                            float offset = (ballY - Player1.Y) / (Player1.Height * 0.5f); // gets how far from centre the ball hit
                            offset = Math.Clamp(offset, -1f, 1f); // we clamp it just to be sure (can be a little bit more than 1)

                            Ball.Direction = MathF.Sign(offset) * MathF.Pow(offset, 2) * MathF.PI / 3; // offset squared because i like it more
                        }                                                                               // the ball feels more predictable
                    }
                    //else if (vX < 0 &&
                    //         ballX < Player1.X &&
                    //         ballX >= Player1.X - Player1.Width / 2)
                    //{
                    //    Ball.Direction = -Ball.Direction;
                    //}

                    if (ballX + Ball.Size / 2 >= Player2.X - Player2.Width / 2 &&
                        vX > 0 &&
                        ballX <= Player2.X)
                    {
                        if (ballY - Ball.Size / 2 <= Player2.URCorner.Y &&
                            ballY + Ball.Size / 2 >= Player2.BRCorner.Y)
                        {
                            if (Ball.Speed < MaxBallSpeed)
                                Ball.Speed = (float)(Ball.Speed + 100);

                            ballX = Player2.X - Player2.Width / 2 - Ball.Size / 2;

                            float offset = (ballY - Player2.Y) / (Player2.Height * 0.5f);
                            offset = Math.Clamp(offset, -1f, 1f);

                            Ball.Direction = MathF.PI - MathF.Sign(offset) * MathF.Pow(offset, 2) * MathF.PI / 3;
                        }
                    }
                    //else if (vX > 0 &&
                    //        ballX > Player2.X &&
                    //        ballX <= Player2.X + Player2.Width &&
                    //        ballY - Ball.Size <= Player2.ULCorner.Y &&
                    //        ballY + Ball.Size >= Player2.BLCorner.Y)
                    //{
                    //    Ball.Direction = -Ball.Direction;
                    //    if (ballY > Player1.Y)
                    //        ballY = Player2.Y + Player2.Height / 2 + Ball.Size / 2;
                    //    else if (ballY < Player2.Y)
                    //        ballY = Player2.Y - Player2.Height / 2 - Ball.Size / 2;
                    //}

                    if ((ballY + Ball.Size / 2 >= Height / 2) &&
                        (ballX + Ball.Size / 2 >= -BorderLength / 2 && ballX - Ball.Size / 2 <= BorderLength / 2))
                    {
                        Ball.Direction = -Ball.Direction;
                        ballY = Height / 2 - Ball.Size / 2;
                    }
                    if ((ballY - Ball.Size / 2 <= -Height / 2) &&
                        (ballX + Ball.Size / 2 >= -BorderLength / 2 && ballX - Ball.Size / 2 <= BorderLength / 2))
                    {
                        Ball.Direction = -Ball.Direction;
                        ballY = -Height / 2 + Ball.Size / 2;
                    }

                    //float ballAcceleration = 30;
                    //Ball.Speed = MathF.Min((float)(Ball.Speed + ballAcceleration * deltaTime), maxSpeed); // we cap max speed of the ball so it doesnt get too fast
                    //Debug.WriteLine(Ball.Speed);
                    Ball.X = ballX;
                    Ball.Y = ballY;

                    if (ballX < -BorderLength / 2 * 1.1f)
                    {
                        NewRoundBallDirection(Player1);
                        ResetBall();
                        Player2.Score++;
                        pointScored = true;
                        State = GameState.GoalPause;
                    }
                    else if (ballX > BorderLength / 2 * 1.1f)
                    {
                        NewRoundBallDirection(Player2);
                        ResetBall();
                        Player1.Score++;
                        pointScored = true;
                        State = GameState.GoalPause;
                    }

                    return pointScored;

                default:
                    throw new NotImplementedException("Not implemented enum GameState");
            }
        }

        private void ResetBall()
        {
            Ball.X = 0;
            Ball.Y = 0;
            Ball.Speed = Ball.InitialSpeed;
        }

        public void NewRoundBallDirection(Player player)
        {
            if (player.X == Player1.X)
            {
                Ball.Direction = MathF.PI + MaxAngle - RandomBallDirection();
            }

            else if (player.X == Player2.X)
            {
                Ball.Direction = MaxAngle - RandomBallDirection();
            }
        }
        public readonly float MaxAngle = MathF.PI / 3;
        public float RandomBallDirection()
        {
            return 2 * RND.NextSingle() * MaxAngle;
        }
    }
}

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
            State = GameState.TempPause;
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
        { Playing, TempPause, Paused, MainMenu }

        public void ChangePlayerPos(Player player, double deltaTime, Direction direction)
        {
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
        public double goalPause = 0;
        public bool Tick(double deltaTime)
        {
            switch (State)
            {
                case GameState.MainMenu:
                    return false;

                case GameState.Paused:
                    return false;

                case GameState.TempPause:
                    goalPause += deltaTime;
                    int pause = 3;
                    if (goalPause >= pause)
                    {
                        State = GameState.Playing;
                        goalPause = 0;
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
                        ballX >= Player1.X - Player1.Width / 2)
                    {
                        if (ballY - Ball.Size / 2 <= Player1.URCorner.Item2 &&
                            ballY + Ball.Size / 2 >= Player1.BRCorner.Item2)
                        {
                            ballX = Player1.X + Player1.Width / 2 + Ball.Size / 2;

                            float offset = (ballY - Player1.Y) / (Player1.Height * 0.5f); // gets how far from centre the ball hit
                            offset = Math.Clamp(offset, -1f, 1f); // we clamp it just to be sure (can be a little bit more than 1)

                            Ball.Direction = MathF.Sign(offset) * MathF.Pow(offset, 2) * MathF.PI / 3; // offset squared because i like it more
                        }                                                                               // the ball feels more predictable
                    }

                    if (ballX + Ball.Size / 2 >= Player2.X - Player2.Width / 2 &&
                        vX > 0 &&
                        ballX < Player2.X + Player2.Width / 2)
                    {
                        if (ballY - Ball.Size / 2 <= Player2.URCorner.Item2 &&
                            ballY + Ball.Size / 2 >= Player2.BRCorner.Item2)
                        {
                            ballX = Player2.X - Player2.Width / 2 - Ball.Size / 2;

                            float offset = (ballY - Player2.Y) / (Player2.Height * 0.5f);
                            offset = Math.Clamp(offset, -1f, 1f);

                            Ball.Direction = MathF.PI - MathF.Sign(offset) * MathF.Pow(offset, 2) * MathF.PI / 3;
                        }
                    }

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

                    float ballAcceleration = 10;
                    int maxSpeed = 2000;
                    Ball.Speed = MathF.Min((float)(Ball.Speed + ballAcceleration * deltaTime), maxSpeed); // we cap max speed of the ball so it doesnt get too fast
                    Debug.WriteLine(Ball.Speed);
                    Ball.X = ballX;
                    Ball.Y = ballY;

                    if (ballX < -BorderLength / 2 || ballX > BorderLength / 2)
                    {
                        PointScored(ballX);
                        ResetBall();
                        pointScored = true;
                        State = GameState.TempPause;
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

        private void PointScored(float x)
        {
            if (x < -BorderLength / 2)
            {
                Ball.Direction = MathF.PI + MathF.PI / 3 - 2 * RND.NextSingle() * MathF.PI / 3;
                Player2.Score++;
            }

            else
            {
                Ball.Direction = MathF.PI / 3 - 2 * RND.NextSingle() * MathF.PI / 3;
                Player1.Score++;
            }
        }
    }
}

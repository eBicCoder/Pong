using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Core
{
    public class Board
    {
        public Board(int height, Player player1, Player player2, Ball ball)
        {
            Height = height;
            Player1 = player1;
            Player2 = player2;
            Ball = ball;
            BorderLength = MathF.Abs(player1.X - player2.X) * 1.2f;
        }

        public int Height { get; init; }
        public Player Player1 { get; init; }
        public Player Player2 { get; init; }
        public Ball Ball { get; init; }
        public float BorderLength { get; private set; }

        public enum Direction
        { Up, Down }

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

        public void Tick(double deltaTime) // opravit fyziku odrážení od hráče
        {
            float ballX = Ball.X + (float)(MathF.Cos(Ball.Direction) * Ball.Speed * deltaTime);
            float ballY = Ball.Y + (float)(MathF.Sin(Ball.Direction) * Ball.Speed * deltaTime);

            if (ballX - Ball.Size / 2 <= Player1.X + Player1.Width / 2 && ballX >= Player1.X)
            {
                if (ballY - Ball.Size / 2 <= Player1.URCorner.Item2 &&
                    ballY + Ball.Size / 2 >= Player1.BRCorner.Item2)
                {
                    ballX = Player1.X + Player1.Width / 2 + Ball.Size / 2;

                    //float direction = Math.Clamp(MathF.PI - (Ball.Direction - (ballY - Player1.Y) / (Player1.Height / 2), -1f, +1f));
                    Ball.Direction = Math.Clamp(MathF.PI - (Ball.Direction - (ballY - Player1.Y) / (Player1.Height / 2) * MathF.PI / 2), -1f, +1f);
                }
            }

            if (ballX + Ball.Size / 2 >= Player2.X - Player2.Width / 2 && ballX < Player2.X)
            {
                if (ballY - Ball.Size / 2 <= Player2.URCorner.Item2 &&
                    ballY + Ball.Size / 2 >= Player2.BRCorner.Item2)
                {
                    ballX = Player2.X - Player2.Width / 2 - Ball.Size / 2;

                    Ball.Direction = Math.Clamp(MathF.PI - (Ball.Direction - (ballY - Player2.Y) / (Player2.Height / 2) * MathF.PI / 2), MathF.PI - 1f, MathF.PI + 1f);
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
            Ball.Speed += (float)(ballAcceleration * deltaTime);

            Ball.X = ballX;
            Ball.Y = ballY;
        }
    }
}

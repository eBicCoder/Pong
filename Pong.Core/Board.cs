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
        public Board(int height, Player player1, Player player2, Ball ball, int maxScore)
        {
            Height = height;
            Player1 = player1;
            Player2 = player2;
            Ball = ball;
            BorderLength = MathF.Abs(player1.X - player2.X) * 1.2f;
            State = BoardState.TempPause;
            MaxScore = maxScore;
        }

        public int Height { get; init; }
        public float Top => Height / 2f;
        public float Bottom => -Height / 2f;
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Ball Ball { get; set; }
        public float BorderLength { get; private set; }
        public BoardState State = BoardState.TempPause;
        public int MaxScore { get; private set; }
        public Player? Winner { get; set; }

        public enum BoardState
        { Playing, TempPause, Paused, GameEnded }

        private BoardState _previousState;
        public void ChangeState()
        {
            if (State == Board.BoardState.Playing ||
                State == Board.BoardState.TempPause ||
                State == Board.BoardState.GameEnded)
            {
                _previousState = State;
                State = Board.BoardState.Paused;
            }

            else if (State == Board.BoardState.Paused)
            {
                goalPauseElapsed = 0;
                State = Board.BoardState.TempPause;

                if (_previousState == BoardState.Playing)
                    Player.canMove = false;

                else if (_previousState == BoardState.TempPause)
                    Player.canMove = true;

                _previousState = State;
            }
        }

        public void ResizePause()
        {
            if (State == Board.BoardState.Playing ||
                State == Board.BoardState.TempPause)
            {
                _previousState = State;
                State = Board.BoardState.Paused;
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
                case BoardState.Paused:
                    return false;

                case BoardState.TempPause:
                    goalPauseElapsed += deltaTime;
                    if (goalPauseElapsed >= goalPause)
                    {
                        State = BoardState.Playing;
                        goalPauseElapsed = 0;
                        Player.canMove = true;
                    }
                    return false;

                case BoardState.Playing:
                    bool pointScored = false;

                    float vX = (float)(MathF.Cos(Ball.Direction) * Ball.Speed * deltaTime);
                    float vY = (float)(MathF.Sin(Ball.Direction) * Ball.Speed * deltaTime);

                    float ballX = Ball.X + vX;
                    float ballY = Ball.Y + vY;

                    if (ballX - Ball.Size / 2 <= Player1.X + Player1.Width / 2 &&
                        vX < 0 &&
                        ballX >= Player1.X - Player1.Width / 2)
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

                    if (ballX + Ball.Size / 2 >= Player2.X - Player2.Width / 2 &&
                        vX > 0 &&
                        ballX <= Player2.X + Player2.Width / 2)
                    {
                        if (ballY - Ball.Size / 2 <= Player2.URCorner.Y &&
                            ballY + Ball.Size / 2 >= Player2.BRCorner.Y)
                        {
                            if (Ball.Speed < MaxBallSpeed)
                                Ball.Speed = Ball.Speed + 100;

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
                        Ball.Direction = NewRoundBallDirection(Player1);
                        ResetBall();
                        Player2.Score++;
                        pointScored = true;
                        State = BoardState.TempPause;
                    }
                    else if (ballX > BorderLength / 2 * 1.1f)
                    {
                        Ball.Direction = NewRoundBallDirection(Player2);
                        ResetBall();
                        Player1.Score++;
                        pointScored = true;
                        State = BoardState.TempPause;
                    }

                    if (pointScored)
                    {
                        if (Player1.Score == MaxScore)
                        {
                            Winner = Player1;
                            State = BoardState.GameEnded;
                        }
                        else if (Player2.Score == MaxScore)
                        {
                            Winner = Player2;
                            State = BoardState.GameEnded;
                        }
                    }
                    return pointScored;

                case BoardState.GameEnded:
                    return false;

                default:
                    throw new NotImplementedException("Not implemented enum GameState");
            }
        }

        // HOTOVO hodit PlayerChangePos do Player a tam dát i tu bot logiku, ale to se mi nechce teď řešit
        // HOTOVA (FURT TRASH) udělat cooldown na změnu směru pohybu, aby se tak necukalo, ale to se mi taky nechce řešit teď
        // HOTOVO udělat nějakou lepší logiku pro boty, aby se třeba snažili odhadnout kam bude míč a ne jenom sledovali jeho pozici, ale to se mi taky nechce řešit teď
        // STAČÍ DODĚLAT IMPOSSIBLE udělat různé obtížnosti botů, ale to se mi taky nechce řešit teď
        
        private void ResetBall()
        {
            Ball.X = 0;
            Ball.Y = 0;
            Ball.Speed = Ball.InitialSpeed;
        }
        
        public float NewRoundBallDirection(Player player)
        {
            float direction;
            if (player == Player1)
            {
                direction = MathF.PI + MaxAngle - RandomBallDirection();
            }

            else if (player == Player2)
            {
                direction = MaxAngle - RandomBallDirection();
            }

            else
                throw new Exception("Player isnt on the board");

            return direction;
        }
        public readonly float MaxAngle = MathF.PI / 3;
        public float RandomBallDirection()
        {
            return 2 * RND.NextSingle() * MaxAngle;
        }
    }
}

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
        public static Random RND = new Random();
        public Board(uint height, Player player1, Player player2, Ball ball, uint maxScore)
        {
            Height = height;
            Player1 = player1;
            if (player1.X >= 0)
                throw new ArgumentException("Player 1 must be on the left side of the board");
            Player2 = player2;
            if (player2.X <= 0)
                throw new ArgumentException("Player 2 must be on the right side of the board");
            Ball = ball;
            BorderLength = MathF.Abs(player1.X - player2.X) * 1.2f;
            State = BoardState.TempPause;
            MaxScore = maxScore;
        }

        public uint Height { get; init; }
        public float Top => Height / 2f;
        public float Bottom => -Height / 2f;
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Ball Ball { get; set; }
        public float BorderLength { get; private set; }
        public BoardState State = BoardState.TempPause;
        public uint MaxScore { get; private set; }
        public Player? Winner { get; set; }

        public enum BoardState
        { Playing, TempPause, Paused, GameEnded }

        private BoardState _previousState;
        /// <summary>
        /// Method for changing board state
        /// </summary>
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
                PauseElapsed = 0;
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

        public readonly int PauseLength = 3;
        public double PauseElapsed = 0;

        /// <summary>
        /// Method that updates the board
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Tick(double deltaTime)
        {
            switch (State)
            {
                case BoardState.Paused:
                    return false;

                case BoardState.TempPause:
                    PauseElapsed += deltaTime;
                    if (PauseElapsed >= PauseLength)
                    {
                        State = BoardState.Playing; 
                        PauseElapsed = 0; // resets timer
                        Player.canMove = true; // enables movement when the game unpauses
                    }
                    return false;

                case BoardState.Playing:
                    bool pointScored = false;

                    float vX = (float)(MathF.Cos(Ball.Direction) * Ball.Speed * deltaTime);
                    float vY = (float)(MathF.Sin(Ball.Direction) * Ball.Speed * deltaTime);

                    float ballX = Ball.X + vX;
                    float ballY = Ball.Y + vY;

                    if (ballX - Ball.Size / 2 <= Player1.X + Player1.Width / 2 && // checks if the ball is colliding with the player and if its moving towards the player
                        vX < 0 &&                                                 // seperated with y checks to make it more efficient 
                        ballX >= Player1.X - Player1.Width / 2)                   // we check only if the ball is in small rectangle around the player
                    {
                        if (ballY - Ball.Size / 2 <= Player1.URCorner.Y &&  // if it is inside rectangle
                            ballY + Ball.Size / 2 >= Player1.BRCorner.Y)    // we check if the ball is colliding with the player
                        {
                            if (Ball.Speed < Ball.MaxSpeed) // if the ball hasnt reached max speed we increase its speed
                                Ball.Speed = Ball.Speed + Ball.Acceleration;

                            ballX = Player1.X + Player1.Width / 2 + Ball.Size / 2; // we set the ball position to the edge of the player so it doesnt get stuck in the player

                            float offset = (ballY - Player1.Y) / (Player1.Height * 0.5f); // gets how far from centre the ball hit
                            offset = Math.Clamp(offset, -1f, 1f); // we clamp it just to be sure (can be a little bit more than 1)
                                                                  // because we only check centre of the ball it can be a little bit more than 1 if the ball hits with its edge
                            Ball.Direction = MathF.Sign(offset) * MathF.Pow(offset, 2) * Ball.MaxAngle; // offset squared because i like it more
                        }                                                                              // the ball feels more predictable
                    }

                    if (ballX + Ball.Size / 2 >= Player2.X - Player2.Width / 2 && // same for player 2
                        vX > 0 &&
                        ballX <= Player2.X + Player2.Width / 2)
                    {
                        if (ballY - Ball.Size / 2 <= Player2.URCorner.Y &&
                            ballY + Ball.Size / 2 >= Player2.BRCorner.Y)
                        {
                            if (Ball.Speed < Ball.MaxSpeed)
                                Ball.Speed = Ball.Speed + Ball.Acceleration;

                            ballX = Player2.X - Player2.Width / 2 - Ball.Size / 2;

                            float offset = (ballY - Player2.Y) / (Player2.Height * 0.5f);
                            offset = Math.Clamp(offset, -1f, 1f);

                            Ball.Direction = MathF.PI - MathF.Sign(offset) * MathF.Pow(offset, 2) * Ball.MaxAngle;
                        }
                    }
                    // checks if the ball is colliding with the top and bottom wall
                    if ((ballY + Ball.Size / 2 >= Top) &&
                        (ballX + Ball.Size / 2 >= -BorderLength / 2 && ballX - Ball.Size / 2 <= BorderLength / 2))
                    {
                        Ball.Direction = -Ball.Direction; // inverts y direction of the ball
                        ballY = Top - Ball.Size / 2;      // works because sin is odd function
                    }
                    else if ((ballY - Ball.Size / 2 <= Bottom) &&
                        (ballX + Ball.Size / 2 >= -BorderLength / 2 && ballX - Ball.Size / 2 <= BorderLength / 2))
                    {
                        Ball.Direction = -Ball.Direction;
                        ballY = Bottom + Ball.Size / 2;
                    }

                    Ball.X = ballX; // we set the new position of the ball after all collisin checks
                    Ball.Y = ballY; // but before checking if the point is scored (better for reseting ball position)

                    if (ballX < -BorderLength / 2 * 1.1f) // checks if the ball is out of bounds and which player scored
                    {
                        Ball.Direction = Ball.NewRoundBallDirection(Player1);
                        Ball.ResetBall();
                        Player2.Score++;
                        pointScored = true;
                        State = BoardState.TempPause;
                    }
                    else if (ballX > BorderLength / 2 * 1.1f)
                    {
                        Ball.Direction = Ball.NewRoundBallDirection(Player2);
                        Ball.ResetBall();
                        Player1.Score++;
                        pointScored = true;
                        State = BoardState.TempPause;
                    }

                    if (pointScored && MaxScore != 0) // if maxscore is 0 the game never ends
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
    }
}

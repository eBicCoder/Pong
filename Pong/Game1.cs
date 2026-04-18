using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pong.Core;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Diagnostics;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _border;
        private Texture2D _ball;
        private Texture2D _player;

        private SpriteFont _font;

        private float SpriteScale;

        private Board Board { get; set; }
        private int PĺayerSpeed = 1000;

        private Button PauseButton;

        private bool IsFullScreen;

        private KeyboardState _oldKeyBoardState { get; set; }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            Window.Title = "Pong 6.7";

            this.IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Player player1 = new Player(32, 256, PĺayerSpeed, -900);
            Player player2 = new Player(32, 256, PĺayerSpeed, 900);
            
            Ball ball = new Ball(32, 0);
            Board = new Board(1200, player1, player2, ball);

            Player[] players = { player1, player2 };
            Player targetPlayer = players[Board.RND.Next(players.Length)];
            Board.NewRoundBallDirection(targetPlayer);


            Debug.WriteLine($"{Board.Ball.Direction}");
            Board.State = Board.GameState.GoalPause;
            
            ChangeSpriteScale();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _border = this.Content.Load<Texture2D>("Sprites/Border");
            _ball = this.Content.Load<Texture2D>("Sprites/Ball");
            _player = this.Content.Load<Texture2D>("Sprites/Player");

            _font = this.Content.Load<SpriteFont>("SpriteFont/Font");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();
             
            if (keyboardState.IsKeyDown(Keys.W))
                Board.ChangePlayerPos(Board.Player1, deltaTime, Board.Direction.Up);
            if (keyboardState.IsKeyDown(Keys.S))
                Board.ChangePlayerPos(Board.Player1, deltaTime, Board.Direction.Down);

            if (keyboardState.IsKeyDown(Keys.Up))
                Board.ChangePlayerPos(Board.Player2, deltaTime, Board.Direction.Up);
            if (keyboardState.IsKeyDown(Keys.Down))
                Board.ChangePlayerPos(Board.Player2, deltaTime, Board.Direction.Down);

            if (IsKeyPressed(keyboardState, Keys.P))
            {
                Board.ChangeState();  
            }

            _oldKeyBoardState = keyboardState;

            bool pointScored = Board.Tick(deltaTime);

            if (Board.State == Board.GameState.Playing)
                RoundTime += deltaTime;

            if (pointScored)
                RoundTime = 0;

            base.Update(gameTime);
        }

        public void OnResize(object sender, EventArgs e)
        {
            ChangeSpriteScale();
            Board.ResizePause();
        }
        private int ScoreBarHeight = 150;
        private (float, float) BoardToMonitorPos(float x, float y)
        {
            float X = Window.ClientBounds.Width / 2f + x * SpriteScale;
            float Y = (Window.ClientBounds.Height + ScoreBarHeight * SpriteScale) / 2f - y * SpriteScale;

            return (X, Y);
        }

        private void ChangeSpriteScale()
        {
            if ((Window.ClientBounds.Height - ScoreBarHeight) * Board.BorderLength < Window.ClientBounds.Width * Board.Height)
                SpriteScale = (float)(Window.ClientBounds.Height) / (float)(Board.Height + ScoreBarHeight) * 0.9f;

            else
                SpriteScale = (float)(Window.ClientBounds.Width) / (float)(Board.BorderLength) * 0.9f;
        }

        private double RoundTime;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //Debug.WriteLine(gameTime.ElapsedGameTime.TotalSeconds);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if (Board.State == Board.GameState.MainMenu)
            {
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            int borderWidth = 32;

            Vector2 textAboveBoard = new Vector2(Window.ClientBounds.Width / 2f, (Window.ClientBounds.Height + ScoreBarHeight * SpriteScale) / 2f - (Board.Height / 2f + borderWidth) * SpriteScale);
            // draws round time
            int roundTime = (int)Math.Floor(RoundTime);
            string roundTimeString = roundTime.ToString();
            Vector2 roundTimeStringSize = _font.MeasureString(roundTimeString) * SpriteScale;
            _spriteBatch.DrawString(_font,
                                    roundTimeString,
                                    new Vector2(textAboveBoard.X - roundTimeStringSize.X / 2,
                                                textAboveBoard.Y - roundTimeStringSize.Y), 
                                    Color.White, 
                                    0f, 
                                    Vector2.Zero,
                                    SpriteScale,
                                    SpriteEffects.None,
                                    0f);

            // draws the ball
            (float xBallULC, float yBallULC) = BoardToMonitorPos(Board.Ball.ULCorner.X, Board.Ball.ULCorner.Y);
            _spriteBatch.Draw(
                _ball,
                new Vector2
                    (xBallULC,
                    yBallULC),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale / (64 / Board.Ball.Size),
                SpriteEffects.None,
                0);

            // draws player1 (left)
            (float xPlayer1ULC, float yPlayer1ULC) = BoardToMonitorPos(Board.Player1.ULCorner.X, Board.Player1.ULCorner.Y);
            _spriteBatch.Draw(
                _player,
                new Vector2
                    (xPlayer1ULC,
                    yPlayer1ULC),
                new Rectangle(0, 0, Board.Player1.Width, Board.Player1.Height),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);

            // draws player1 score
            (float xPlayer1, float yPlayer1) = BoardToMonitorPos(Board.Player1.X, Board.Player1.Y);
            string player1ScoreString = Board.Player1.Score.ToString();
            Vector2 player1ScoreStringSize = _font.MeasureString(player1ScoreString) * SpriteScale;
            _spriteBatch.DrawString(_font,
                                    player1ScoreString,
                                    new Vector2(xPlayer1 - player1ScoreStringSize.X / 2,
                                                textAboveBoard.Y - player1ScoreStringSize.Y),
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    SpriteScale,
                                    SpriteEffects.None,
                                    0f);

            // draws player2 (right)
            (float xPlayer2ULC, float yPlayer2ULC) = BoardToMonitorPos(Board.Player2.ULCorner.X, Board.Player2.ULCorner.Y);
            _spriteBatch.Draw(
                _player,
                new Vector2
                    (xPlayer2ULC,
                    yPlayer2ULC),
                new Rectangle(0, 0, Board.Player1.Width, Board.Player1.Height),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);

            // draws player2 score
            (float xPlayer2, float yPlayer2) = BoardToMonitorPos(Board.Player2.X, Board.Player2.Y);
            string player2ScoreString = Board.Player2.Score.ToString();
            Vector2 player2ScoreStringSize = _font.MeasureString(player2ScoreString) * SpriteScale;
            _spriteBatch.DrawString(_font,
                                    player2ScoreString,
                                    new Vector2(xPlayer2 - player2ScoreStringSize.X / 2,
                                                textAboveBoard.Y - player1ScoreStringSize.Y),
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    SpriteScale,
                                    SpriteEffects.None,
                                    0f);

            // draws top border
            _spriteBatch.Draw(
                _player,
                new Vector2
                    (Window.ClientBounds.Width / 2f - Board.BorderLength / 2 * SpriteScale,
                    (Window.ClientBounds.Height + ScoreBarHeight * SpriteScale) / 2f - (Board.Height / 2f + borderWidth) * SpriteScale),
                new Rectangle(0, 0, (int)(Board.BorderLength), borderWidth),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);
            // draws bottom border
            _spriteBatch.Draw(
                _player,
                new Vector2
                    (Window.ClientBounds.Width / 2f - Board.BorderLength / 2 * SpriteScale,
                    (Window.ClientBounds.Height + ScoreBarHeight * SpriteScale) / 2f - (-Board.Height / 2f) * SpriteScale),
                new Rectangle(0, 0, (int)(Board.BorderLength), borderWidth),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);

            if (Board.State == Board.GameState.Paused)
            {
                string paused = "Paused";
                Vector2 pauseSize = _font.MeasureString(paused) * SpriteScale;
                _spriteBatch.DrawString(_font, paused, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2) - pauseSize / 2, Color.White, 0f, Vector2.Zero, SpriteScale, SpriteEffects.None, 0f);

                string pauseHelp = "Press 'P' to unpause";
                Vector2 pauseHelpSize = _font.MeasureString(pauseHelp) * SpriteScale;
                _spriteBatch.DrawString(_font, pauseHelp, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2 + pauseSize.Y) - pauseHelpSize / 2, Color.White, 0f, Vector2.Zero, SpriteScale, SpriteEffects.None, 0f);
            }

            else if (Board.State == Board.GameState.GoalPause)
            {
                int tempPauseLeft = (int)Math.Ceiling(Board.goalPause - Board.goalPauseElapsed);
                string pauseLeftString = tempPauseLeft.ToString();
                Vector2 pauseLeftStringSize = _font.MeasureString(pauseLeftString) * SpriteScale;
                _spriteBatch.DrawString(_font, pauseLeftString, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2) - pauseLeftStringSize / 2, Color.White, 0f, Vector2.Zero, SpriteScale, SpriteEffects.None, 0f);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        

        private bool IsKeyPressed(KeyboardState keyboardState, Keys key)
        {
            bool isKeyPressed = false;

            if (keyboardState.IsKeyDown(key) &&
                !_oldKeyBoardState.IsKeyDown(key))
                isKeyPressed = true;

            return isKeyPressed;
        }
    }
}

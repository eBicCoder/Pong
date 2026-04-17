using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pong.Core;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
//using System.Windows.Forms;

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

        private KeyboardState _oldKeyBoardState { get; set; }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;
            Window.AllowUserResizing = true;
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
            Board.State = Board.GameState.MainMenu; 
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

            Board.Tick(deltaTime);

            if (Board.State == Board.GameState.Playing)
                RoundTime += deltaTime;

            if (Board.State == Board.GameState.GoalPause)
                RoundTime = 0;

            base.Update(gameTime);
        }

        public void OnResize(object sender, EventArgs e)
        {
            ChangeSpriteScale();
            Board.State = Board.GameState.Paused;
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

            //_spriteBatch.DrawString(_font, $"{(int)RoundTime}", new Vector2(), Color.LightGreen);

            // draws the ball
            (float xBallULC, float yBallULC) = BoardToMonitorPos(Board.Ball.ULCorner.Item1, Board.Ball.ULCorner.Item2);
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
            // draws left player
            (float xPlayer1ULC, float yPlayer1ULC) = BoardToMonitorPos(Board.Player1.ULCorner.Item1, Board.Player1.ULCorner.Item2);
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
            // draws right player
            (float xPlayer2ULC, float yPlayer2ULC) = BoardToMonitorPos(Board.Player2.ULCorner.Item1, Board.Player2.ULCorner.Item2);
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

            // draws top and bottom border
            int borderWidth = 32;
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

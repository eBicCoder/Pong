using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Pong.Core;
using System.Collections.Generic;

namespace Pong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _border;
        private Texture2D _ball;
        private Texture2D _player;
        private float SpriteScale;

        private Board Board { get; set; }
        private int PĺayerSpeed = 1000;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.ClientSizeChanged += OnResize;
            Window.AllowUserResizing = true;
            Window.Title = "Pong 6.7";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Player player1 = new Player(32, 256, PĺayerSpeed, -900);
            Player player2 = new Player(32, 256, PĺayerSpeed, 900);
            Ball ball = new Ball(32, 0);
            Board = new Board(1200, player1, player2, ball);

            ChangeSpriteScale();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _border = this.Content.Load<Texture2D>("Sprites/Border");
            _ball = this.Content.Load<Texture2D>("Sprites/Ball");
            _player = this.Content.Load<Texture2D>("Sprites/Player");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                Board.ChangePlayerPos(Board.Player1, deltaTime, Board.Direction.Up);
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                Board.ChangePlayerPos(Board.Player1, deltaTime, Board.Direction.Down);

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                Board.ChangePlayerPos(Board.Player2, deltaTime, Board.Direction.Up);
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                Board.ChangePlayerPos(Board.Player2, deltaTime, Board.Direction.Down);

            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
            {
                if (Board.State == Board.GameState.Playing ||
                    Board.State == Board.GameState.TempPause)
                    Board.State = Board.GameState.Paused;

                if (Board.State == Board.GameState.Paused)
                {
                    Board.goalPause = 0;
                    Board.State = Board.GameState.TempPause;
                }    
            }

            Board.Tick(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if (Board.State == Board.GameState.MainMenu)
            {
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

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

            // now we draw top and bottom border
            int borderWidth = 32;
            _spriteBatch.Draw(
                _player,
                new Vector2
                    (Window.ClientBounds.Width / 2f - Board.BorderLength / 2 * SpriteScale,
                    Window.ClientBounds.Height / 2f - (Board.Height / 2f + borderWidth) * SpriteScale),
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
                    Window.ClientBounds.Height / 2f - (-Board.Height / 2f) * SpriteScale),
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

        private (float, float) BoardToMonitorPos(float x, float y)
        {
            float X = Window.ClientBounds.Width / 2f + x * SpriteScale;
            float Y = Window.ClientBounds.Height / 2f - y * SpriteScale;

            return (X, Y);
        }

        public void OnResize(object sender, EventArgs e)
        {
            ChangeSpriteScale();
        }

        private void ChangeSpriteScale()
        {
            if (Window.ClientBounds.Height * Board.BorderLength < Window.ClientBounds.Width * Board.Height)
                SpriteScale = (float)(Window.ClientBounds.Height) / (float)(Board.Height) * 0.9f;

            else
                SpriteScale = (float)(Window.ClientBounds.Width) / (float)(Board.BorderLength) * 0.9f;
        }
    
    }
}

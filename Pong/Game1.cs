using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pong.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Pong.Core.Board;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Pong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _border;
        private Texture2D _ball;
        private Texture2D _player;
        public static Texture2D Pixel;

        private SpriteFont _font;

        public static float SpriteScale;

        private readonly uint BallSize = 32;
        private readonly float BallInitialSpeed = 500;
        private readonly float BallMaxSpeed = 2000;
        private readonly uint BallSpeedIncrease = 100;
        private readonly int PlayerWidth = 32;
        private readonly int PlayerHeight = 256;
        private readonly uint PlayerSpeed = 1000;
        private readonly int PlayerXPos = 900;
        private readonly uint FieldHeight = 1200;
        private readonly uint MaxScore = 10;
        private Board Board { get; set; }

        private bool PointScored;

        private List<Button> MainMenuButtons = new List<Button>();
        private List<Button> BotSelectionButtons = new List<Button>();
        private List<Button> GameEndedButtons = new List<Button>();
        public List<Button> PauseButtons = new List<Button>();

        private KeyboardState _oldKeyBoardState { get; set; }

        private GameState State = GameState.MainMenu;

        private enum GameState
        { MainMenu, BotSelection, Playing}
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
            SetupBoard("Bot", Player.PlayerType.Impossible,
                       "Bot", Player.PlayerType.Impossible,
                       0);

            Board.State = BoardState.TempPause;

            OnResize(this, EventArgs.Empty); // initializes the sprite scale and screen center for the first time

            base.Initialize();
        }
        /// <summary>
        /// Method that initializes the board with the given parameters, used for starting a new game with the right settings when the player clicks on the buttons in the menu
        /// </summary>
        /// <param name="player1Name"></param>
        /// <param name="player1Type"></param>
        /// <param name="player2Name"></param>
        /// <param name="player2Type"></param>
        /// <param name="maxScore"></param>
        private void SetupBoard (string player1Name, Player.PlayerType player1Type, 
                                 string player2Name, Player.PlayerType player2Type,
                                 uint maxScore)
        {
            Ball ball = new Ball(BallSize, 0f, BallInitialSpeed, BallMaxSpeed, BallSpeedIncrease, MathF.PI / 3);

            Player player1 = new Player(player1Name, PlayerWidth, PlayerHeight, player1Type, PlayerSpeed, -PlayerXPos);
            Player player2 = new Player(player2Name, PlayerWidth, PlayerHeight, player2Type, PlayerSpeed, PlayerXPos);

            Board = new Board(FieldHeight, player1, player2, ball, maxScore);

            Player[] players = { player1, player2 };
            Player targetPlayer = players[Board.RND.Next(players.Length)];
            Board.Ball.Direction = Board.Ball.NewRoundBallDirection(targetPlayer);

            RoundTime = 0;
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _border = this.Content.Load<Texture2D>("Sprites/Border");
            _ball = this.Content.Load<Texture2D>("Sprites/Ball");
            _player = this.Content.Load<Texture2D>("Sprites/Player");

            _font = this.Content.Load<SpriteFont>("SpriteFont/Font");
            // TODO: use this.Content to load your game content here

            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });

            // main menu buttons initialization
            MainMenuButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.55f),
                Size = new Vector2(500, 120),
                Text = "2 hráči",
                OnClick = () =>
                {
                    State = GameState.Playing;
                    SetupBoard("levý hráč", Player.PlayerType.Human,
                               "pravý hráč", Player.PlayerType.Human,
                               MaxScore);
                }
            });

            MainMenuButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.45f),
                Size = new Vector2(500, 120),
                Text = "1 hráč",
                OnClick = () => State = GameState.BotSelection
            });

            MainMenuButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.8f),
                Size = new Vector2(500,120),
                Text = "Konec",
                OnClick = () => Exit()
            });

            // bot menu buttons initialization
            BotSelectionButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.35f),
                Size = new Vector2(500, 120),
                Text = "Easy",
                OnClick = () =>
                {
                    State = GameState.Playing;
                    SetupBoard("lidský hráč", Player.PlayerType.Human,
                               "easy bot", Player.PlayerType.Easy,
                               MaxScore);
                }
            });

            BotSelectionButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.45f),
                Size = new Vector2(500, 120),
                Text = "Normal",
                OnClick = () =>
                {
                    State = GameState.Playing;
                    SetupBoard("lidský hráč", Player.PlayerType.Human,
                               "normal bot", Player.PlayerType.Normal,
                               MaxScore);
                }
            });

            BotSelectionButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.55f),
                Size = new Vector2(500, 120),
                Text = "Hard",
                OnClick = () =>
                {
                    State = GameState.Playing;
                    SetupBoard("lidský hráč", Player.PlayerType.Human,
                               "hard bot", Player.PlayerType.Hard,
                               MaxScore);
                }
            });

            BotSelectionButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.65f),
                Size = new Vector2(500, 120),
                Text = "Impossible",
                OnClick = () =>
                {
                    State = GameState.Playing;
                    SetupBoard("lidský hráč", Player.PlayerType.Human,
                               "impossible bot", Player.PlayerType.Impossible,
                               MaxScore);
                }
            });

            BotSelectionButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.8f),
                Size = new Vector2(500, 120),
                Text = "Zpět",
                OnClick = () =>
                {
                    State = GameState.MainMenu;
                }
            });

            // game ended menu buttons initialization
            GameEndedButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.8f),
                Size = new Vector2(500, 120),
                Text = "Do menu",
                OnClick = () =>
                {
                    State = GameState.MainMenu;
                    SetupBoard("Bot", Player.PlayerType.Impossible,
                       "Bot", Player.PlayerType.Impossible,
                       0);
                }
            });

            // pause buttons initialization
            PauseButtons.Add(new Button
            {
                RelativePosition = new Vector2(0.5f, 0.5f),
                Size = new Vector2(700, 120),
                Text = "Pokračovat",
                OnClick = () =>
                {
                    Board.ChangeState();
                }
            });
            PauseButtons.Add(new Button
             {
                 RelativePosition = new Vector2(0.5f, 0.8f),
                 Size = new Vector2(500, 120),
                 Text = "Do menu",
                 OnClick = () =>
                 {
                     State = GameState.MainMenu;
                     SetupBoard("Bot", Player.PlayerType.Impossible,
                        "Bot", Player.PlayerType.Impossible,
                        0);
                 }
             });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            float maxDeltaTime = 0.02f;
            if (deltaTime > maxDeltaTime)
            {
                Debug.WriteLine(deltaTime);
                deltaTime = maxDeltaTime; // caps delta time to prevent bugs when the game freezes for a moment (ball goes through player, etc.)
            }
            var mouseState = Mouse.GetState();
            var viewport = GraphicsDevice.Viewport;

            // controls if buttons are being hovered/clicked if game is in the right state
            if (State == GameState.MainMenu)
            {
                foreach (var b in MainMenuButtons)
                    b.Update(mouseState, viewport);
            }
            else if (State == GameState.BotSelection)
            {
                foreach (var b in BotSelectionButtons)
                    b.Update(mouseState, viewport);
            }
            else if (State == GameState.Playing &&
                Board.State == Board.BoardState.GameEnded)
            {
                foreach (var b in GameEndedButtons)
                    b.Update(mouseState, viewport);
            }
            else if (State == GameState.Playing &&
                Board.State == Board.BoardState.Paused)
            {
                foreach (var b in PauseButtons)
                    b.Update(mouseState, viewport);
            }


            KeyboardState keyboardState = Keyboard.GetState();
            if (Board.Player1.Type == Player.PlayerType.Human) // if player1 == human, check for input, else let the bot play
            {
                if (keyboardState.IsKeyDown(Keys.W))
                    Board.Player1.ChangePos(Board, deltaTime, Player.Direction.Up);
                if (keyboardState.IsKeyDown(Keys.S))
                    Board.Player1.ChangePos(Board, deltaTime, Player.Direction.Down);
            }
            else
            {
                Board.Player1.BotPlay(Board, PointScored, deltaTime);
            }

            if (Board.Player2.Type == Player.PlayerType.Human) // same for player2
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                    Board.Player2.ChangePos(Board, deltaTime, Player.Direction.Up);
                if (keyboardState.IsKeyDown(Keys.Down))
                    Board.Player2.ChangePos(Board, deltaTime, Player.Direction.Down);
            }
            else
            {
                Board.Player2.BotPlay(Board, PointScored, deltaTime);
            }
            if (IsKeyPressed(keyboardState, Keys.P) &&
                State == GameState.Playing)
            {
                Board.ChangeState();  
            }

            _oldKeyBoardState = keyboardState;

            PointScored = Board.Tick(deltaTime); // if a point is scored, the board will return true, which
                                                 // is used to reset the round time and for the bots to know when to reset their target position
            if (Board.State == Board.BoardState.Playing)
                RoundTime += deltaTime; // adds to the round time only when the game is actually being played, not when it's paused or ended

            if (PointScored)
                RoundTime = 0; // resets the round time when a point is scored

            base.Update(gameTime);
        }
        /// <summary>
        /// Handles resize event (changes sprite scale and pauses the game if it's being played to prevent bugs
        /// with the ball going through the players when the window is resized during gameplay)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnResize(object sender, EventArgs e)
        {
            ChangeSpriteScale();

            xScreenCenter = Window.ClientBounds.Width / 2f;
            yScreenCenter = (Window.ClientBounds.Height + ScoreBarHeight * SpriteScale) / 2f;

            if (State == GameState.Playing)
                Board.ResizePause();
        }
        private int ScoreBarHeight = 150;

        internal float xScreenCenter { get; private set; }
        internal float yScreenCenter { get; private set; }

        private (float, float) BoardToMonitorPos(float x, float y)
        {
            float X = xScreenCenter + x * SpriteScale;
            float Y = yScreenCenter - y * SpriteScale;

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
            if (State == GameState.Playing &&
                (Board.State == Board.BoardState.Playing || Board.State == Board.BoardState.TempPause)) 
                IsMouseVisible = false; // makes the cursor invisible during gameplay, but visible in the menus and when the game is paused/ended
            else
                IsMouseVisible = true;

            GraphicsDevice.Clear(Color.Black);
            //Debug.WriteLine(gameTime.ElapsedGameTime.TotalSeconds);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            int borderWidth = 32;

            Vector2 textAboveBoard = new Vector2(
                xScreenCenter,
                yScreenCenter - (Board.Height / 2f + borderWidth) * SpriteScale);
                

            // draws round time
            int roundTime = (int)Math.Floor(RoundTime);
            string roundTimeString = roundTime.ToString();
            Vector2 roundTimeStringSize = _font.MeasureString(roundTimeString) * SpriteScale;
            _spriteBatch.DrawString(
                _font,
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
            _spriteBatch.DrawString(
                _font,
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
            _spriteBatch.DrawString(
                _font,
                player2ScoreString,
                new Vector2(
                    xPlayer2 - player2ScoreStringSize.X / 2,
                    textAboveBoard.Y - player1ScoreStringSize.Y),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0f);

            // draws top border
            _spriteBatch.Draw(
                _border,
                new Vector2(
                    xScreenCenter - Board.BorderLength / 2 * SpriteScale,
                    yScreenCenter - (Board.Height / 2f + borderWidth) * SpriteScale),
                new Rectangle(0, 0, (int)(Board.BorderLength), borderWidth),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);
            // draws bottom border
            _spriteBatch.Draw(
                _border,
                new Vector2(
                    xScreenCenter - Board.BorderLength / 2 * SpriteScale,
                    yScreenCenter - (-Board.Height / 2f) * SpriteScale),
                new Rectangle(0, 0, (int)(Board.BorderLength), borderWidth),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteScale,
                SpriteEffects.None,
                0);
            // draws pause menu if the game is paused
            if (Board.State == Board.BoardState.Paused &&
                     State == GameState.Playing)
            {
                var viewport = GraphicsDevice.Viewport;

                foreach (var b in PauseButtons)
                    b.Draw(_spriteBatch, _font, viewport);
            }
            // draws temp pause time left if the game is in temp pause (between rounds and after unpausing)
            else if (Board.State == Board.BoardState.TempPause &&
                     State == GameState.Playing)
            {
                int tempPauseLeft = (int)Math.Ceiling(Board.PauseLength - Board.PauseElapsed);
                string pauseLeftString = tempPauseLeft.ToString();
                Vector2 pauseLeftStringSize = _font.MeasureString(pauseLeftString) * SpriteScale;
                _spriteBatch.DrawString(
                    _font, 
                    pauseLeftString, 
                    new Vector2(
                        Window.ClientBounds.Width / 2,
                        Window.ClientBounds.Height / 2) - pauseLeftStringSize / 2, 
                    Color.White, 0f, 
                    Vector2.Zero, 
                    SpriteScale, 
                    SpriteEffects.None, 
                    0f);
            }
            // draws game ended menu and winner if the game ended
            else if (Board.State == Board.BoardState.GameEnded &&
                     State == GameState.Playing)
            {
                string winnerString = $"Vyhrál {Board.Winner}";
                Vector2 winnerStringSize = _font.MeasureString(winnerString);
                Button winner = new Button
                {
                    RelativePosition = new Vector2(0.5f, 0.5f),
                    Size = new Vector2(winnerStringSize.X, 120),
                    Text = $"Vyhrál {Board.Winner.Name}",
                };

                var viewport = GraphicsDevice.Viewport;

                winner.Draw(_spriteBatch, _font, viewport);

                foreach (var b in GameEndedButtons)
                    b.Draw(_spriteBatch, _font, viewport);
            }
            // draws pause menu if the game is paused
            else if (Board.State == Board.BoardState.Paused &&
                     State == GameState.Playing)
            {
                var viewport = GraphicsDevice.Viewport;

                foreach (var b in PauseButtons)
                    b.Draw(_spriteBatch, _font, viewport);
            }
            // draws main menu
            else if (State == GameState.MainMenu)
            {
                var viewport = GraphicsDevice.Viewport;

                foreach (var b in MainMenuButtons)
                    b.Draw(_spriteBatch, _font, viewport);
            }
            // draws bot selection
            else if (State == GameState.BotSelection)
            {
                var viewport = GraphicsDevice.Viewport;

                foreach (var b in BotSelectionButtons)
                    b.Draw(_spriteBatch, _font, viewport);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        
        /// <summary>
        /// Method for detecting key press
        /// </summary>
        /// <param name="keyboardState"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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

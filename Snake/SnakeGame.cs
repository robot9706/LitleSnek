using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Snake
{
    public class SnakeGame : Game
    {
        public static string ExeFolder;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont _font;
        SpriteFont _font2;
        SpriteFont _font3;

        Map _map;

        Snake _player1;
        Snake _player2;

        Rectangle _sidepanel;

        float _gameTime = 0.0f;

        bool _gameOver = false;

        float _gameOverDelay = 0.0f;

        bool _countdown = true;
        float _coutndownTime = 3.0f;
        string _wonText = "";
        float _nextPowerup;

        int _currentMap = 0;
        string[] _maps;
        public SnakeGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 960;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = true;

            IsMouseVisible = true;

            Window.Title = "Little snek by \"It's ON\"";

            ExeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(Path.Combine(ExeFolder, "Map"));
            List<string> mapList = new List<string>();
            for (int x = 0; x < files.Length; x++)
            {
                if(Path.GetExtension(files[x]).ToLower() == ".png")
                {
                    mapList.Add(Path.GetFileName(files[x]));
                }
            }
            _maps = mapList.ToArray();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Art.Load(Content);

            StartGame();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("Font1");
            _font2 = Content.Load<SpriteFont>("Font2");
            _font3 = Content.Load<SpriteFont>("Font3");

            int w = 30 * 32;
            _sidepanel = new Rectangle(w, 0, graphics.PreferredBackBufferWidth - w, graphics.PreferredBackBufferHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if(_gameOverDelay > 0.0f)
            {
                _gameOverDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds; 
                if(_gameOverDelay <= 0.0f)
                {
                    StartGame();
                }
            }
            else if (_countdown)
            {
                _coutndownTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(_coutndownTime <= 0.0f)
                {
                    _countdown = false;
                }
            }
            else
            {
                if (!_gameOver)
                {
                    _gameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _nextPowerup -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(_nextPowerup <= 0.0f)
                    {
                        _nextPowerup = 30.0f;
                        RandomPowerup();
                    }

                    if(_gameTime >= 60.0f)
                    {
                        _player1.TimeSpeedMul = (float)(((int)_gameTime / 60.0f) + 1);
                        _player2.TimeSpeedMul = _player1.TimeSpeedMul;
                    }

                    if (_player1 != null)
                        _player1.Update(gameTime);
                    if (_player2 != null)
                        _player2.Update(gameTime);

                    if (_player1.IsDead && _player2.IsDead)
                    {
                        _gameOverDelay = 3.0f;
                        _wonText = "Draw";
                    }
                    else if (_player1.IsDead)
                    {
                        _gameOverDelay = 3.0f;
                        _wonText = "Player 2 wins!";

                        _player2.Score++;
                    }
                    else if (_player2.IsDead)
                    {
                        _gameOverDelay = 3.0f;
                        _wonText = "Player 1 wins!";

                        _player1.Score++;
                    }

                    if(_player1.GetHead() == _player2.GetHead())
                    {
                        _gameOver = true;
                        _gameOverDelay = 3.0f;
                        _wonText = "Draw";
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            spriteBatch.Begin();

            _map.Render(spriteBatch, gameTime);
            if (_player1 != null)
                _player1.Render(spriteBatch);
            if (_player2 != null)
                _player2.Render(spriteBatch);

            //Sidepanel
            spriteBatch.Draw(Art.CorkBoard, _sidepanel, Color.White);

            float centerY = (float)_sidepanel.Y + ((float)_sidepanel.Height / 2.0f);
            string timeText = "Time: " + FormatTime((int)_gameTime);
            Vector2 size = _font.MeasureString(timeText);

            DrawShadowText(_font, timeText, new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f) - (size.X / 2.0f), centerY - (size.Y / 2.0f)));

            DrawCenteredText(_font, "Player #1", new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), ((_sidepanel.Height / 16.0f))));
            DrawCenteredText(_font, "Score: " + _player1.Score.ToString(), new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), ((_sidepanel.Height / 16.0f) * 2.0f)));
            DrawCenteredText(_font, "Apples: " + _player1.ApplesInRound.ToString(), new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), ((_sidepanel.Height / 16.0f) * 2.5f)));

            spriteBatch.Draw(Art.Apple, new Rectangle(_sidepanel.X + (_sidepanel.Width / 2) - ((Art.AppleGs.Width * 2) / 2), (int)(((_sidepanel.Height / 16.0f) * 2.0f + ((Art.Apple.Height * 2)))), Art.AppleGs.Width * 2, Art.Apple.Height * 2), Color.White);
            DrawCenteredText(_font, Math.Ceiling(_player1.Food * 100.0f).ToString() + "%", new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), ((_sidepanel.Height / 16.0f) * 4.5f)));


            DrawCenteredText(_font, "Player #2", new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), _sidepanel.Height - (_sidepanel.Height / 16.0f)));
            DrawCenteredText(_font, "Score: " + _player2.Score.ToString(), new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), _sidepanel.Height - ((_sidepanel.Height / 16.0f) * 2.0f)));
            DrawCenteredText(_font, "Apples: " + _player2.ApplesInRound.ToString(), new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), _sidepanel.Height - ((_sidepanel.Height / 16.0f) * 2.5f)));

            spriteBatch.Draw(Art.Apple, new Rectangle((int)((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f) - (Art.Apple.Width)), (int)(_sidepanel.Height - ((_sidepanel.Height / 16.0f) * 5.5f)), Art.AppleGs.Width * 2, Art.Apple.Height * 2), Color.White);
            DrawCenteredText(_font, Math.Ceiling(_player2.Food * 100.0f).ToString() + "%", new Vector2((float)_sidepanel.X + ((float)_sidepanel.Width / 2.0f), _sidepanel.Height - ((_sidepanel.Height / 16.0f) * 4.0f)));

            //Countdown
            if (_countdown)
            {
                Vector2 center = new Vector2((_player1.GetHead().X + 0.5f) * 32, (_player1.GetHead().Y + 0.5f) * 32);
                DrawCenteredText(_font, "P1", center);

                center = new Vector2((_player2.GetHead().X + 0.5f) * 32, (_player2.GetHead().Y + 0.5f) * 32);
                DrawCenteredText(_font, "P2", center);

                string str = ((int)Math.Ceiling(_coutndownTime)).ToString();

                Vector2 cs = _font2.MeasureString(str);
                spriteBatch.DrawString(_font2, str, new Vector2(GraphicsDevice.Viewport.Width / 2.0f - (cs.X / 2.0f) + 20, (GraphicsDevice.Viewport.Height / 2.0f) - (cs.Y / 2.0f) + 20), Color.Lerp(Color.Black, Color.TransparentBlack, 0.75f));
                spriteBatch.DrawString(_font2, str, new Vector2(GraphicsDevice.Viewport.Width / 2.0f - (cs.X / 2.0f), (GraphicsDevice.Viewport.Height / 2.0f) - (cs.Y / 2.0f)), Color.White); 
            }

            if (_countdown || _gameOverDelay > 0.0f)
            {
                Vector2 cs = _font2.MeasureString("1");
                DrawCenteredText(_font, _wonText, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, (GraphicsDevice.Viewport.Height / 2.0f) + (cs.Y / 2.0f) * 1.1f));
            }

            string txt = "Alpha v0.1 pre-release";
            spriteBatch.DrawString(_font3, txt, new Vector2(0, GraphicsDevice.Viewport.Height - _font3.MeasureString(txt).Y), Color.Lerp(Color.White, Color.Transparent, 0.5f));

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawCenteredText(SpriteFont font, string text, Vector2 pos)
        {
            Vector2 size = font.MeasureString(text);
            DrawShadowText(font, text, new Vector2(pos.X - (size.X / 2.0f), pos.Y - (size.Y / 2.0f)));
        }

        private void DrawShadowText(SpriteFont font, string text, Vector2 pos)
        {
            spriteBatch.DrawString(font, text, new Vector2(pos.X + 5.0f, pos.Y + 5.0f), Color.Lerp(Color.Black, Color.TransparentBlack, 0.75f));
            spriteBatch.DrawString(font, text, pos, Color.White);
        }

        private void StartGame()
        {
            _map = new Map(this, new System.Drawing.Bitmap(Path.Combine(ExeFolder, "Map", _maps[_currentMap])));
            SpawnRandomApple();

            _gameTime = 0.0f;

            _currentMap++;
            if (_currentMap >= _maps.Length)
                _currentMap = 0;

            _countdown = true;
            _coutndownTime = 3.0f;
            _gameOver = false;

            _nextPowerup = 1.0f;

            int sc = 0;
            if (_player1 != null)
                sc = _player1.Score;
            _player1 = new Snake(_map, _map.P1Spawn, Keys.Left, Keys.Right);
            _player1.Score = sc;

            if (_player2 != null)
                sc = _player2.Score;
            _player2 = new Snake(_map, _map.P2Spawn, Keys.A, Keys.D);
            _player2.Score = sc;
        }

        private string FormatTime(int time)
        {
            int min = time / 60;
            int sec = time % 60;

            string minText = min.ToString();
            if (minText.Length < 2)
                minText = "0" + minText;

            string secText = sec.ToString();
            if (secText.Length < 2)
                secText = "0" + secText;

            return minText + ":" + secText;
        }

        public void SnakeDied(Snake snake)
        {
            _gameOver = true;

            snake.IsDead = true;
        }

        public void OnApplePickup()
        {
            SpawnRandomApple();

            if(_player1.ApplesInRound >= 10)
            {
                _gameOver = true;
                _player2.IsDead = true;
            }
            else if(_player2.ApplesInRound >= 10)
            {
                _gameOver = true;
                _player1.IsDead = true;
            }
        }

        public void SpawnRandomApple()
        {
            Random rnd = new Random();

            int ax;
            int ay;
            do
            {
                ax = rnd.Next(1, _map.MapWidth - 1);
                ay = rnd.Next(1, _map.MapHeight - 1);
            } while (_map.GetTileAt(ax, ay) != null  || _map.NoAppleSpawn[ax,ay]
            || (_player1 != null && _player1.GetBody().Contains(new Point(ax,ay)))
            || (_player2 != null && _player2.GetBody().Contains(new Point(ax, ay))));

            _map.SetTileAt(ax, ay, new AppleTile());
        }

        Type[] powerups = new Type[]
        {
            typeof(GoThroughWallsPowerup),
            typeof(GoThroughWallsPowerup),
            typeof(InputInvertPowerup),
            typeof(SpeedReducePowerup),
            typeof(SpeedBoostPowerup),
            typeof(SpeedBoostPowerup),
            typeof(SpeedBoostPowerup),
        };

        public void RandomPowerup()
        {
            Random rnd = new Random();

            int ax;
            int ay;
            do
            {
                ax = rnd.Next(1, _map.MapWidth - 1);
                ay = rnd.Next(1, _map.MapHeight - 1);
            } while (_map.GetTileAt(ax, ay) != null
            || (_player1 != null && _player1.GetBody().Contains(new Point(ax, ay)))
            || (_player2 != null && _player2.GetBody().Contains(new Point(ax, ay))));

            _map.SetTileAt(ax, ay, (Powerup)Activator.CreateInstance(powerups[rnd.Next(0, powerups.Length)]));
        }

        public Snake GetOtherSnake(Snake s)
        {
            if (s == _player1)
                return _player2;

            return _player1;
        }
    }
}

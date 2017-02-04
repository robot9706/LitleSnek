using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitmap = System.Drawing.Bitmap;
using Point = Microsoft.Xna.Framework.Point;
using GDIColor = System.Drawing.Color;
using Microsoft.Xna.Framework;

namespace Snake
{
    struct RGB
    {
        public byte R;
        public byte G;
        public byte B;

        public RGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    static class EXT
    {
        public static RGB Convert(this GDIColor clr)
        {
            return new RGB(clr.R, clr.G, clr.B);
        }
    }

    public class Map
    {
        private Dictionary<RGB, Type> _lookup = new Dictionary<RGB, Type>()
        {
            { new RGB(0,0,0), typeof(WallTile) },
            { new RGB(0,255,0), typeof(BushTile) },
            { new RGB(255,0,0), typeof(RageModePowerup) }
        };

        public int MapWidth
        {
            get { return _map.GetLength(0); }
        }
        public int MapHeight
        {
            get { return _map.GetLength(1); }
        }

        public Point P1Spawn;
        public Point P2Spawn;

        private Tile[,] _map;
        public bool[,] NoAppleSpawn;

        private static Point _tileSize = new Point(32, 32);

        public SnakeGame Game;

        public Map(SnakeGame game, Bitmap bmp)
        {
            Game = game;

            _map = new Tile[bmp.Width, bmp.Height];
            NoAppleSpawn = new bool[bmp.Width, bmp.Height];

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    NoAppleSpawn[x, y] = false;

                    RGB clr = bmp.GetPixel(x, y).Convert();
                    if(_lookup.ContainsKey(clr))
                    {
                        _map[x, y] = (Tile)Activator.CreateInstance(_lookup[clr]);
                    }
                    else if(clr.R == 0 && clr.G == 0 && clr.B == 255)
                    {
                        P1Spawn = new Point(x, y);
                    }
                    else if (clr.R == 0 && clr.G == 0 && clr.B == 254)
                    {
                        P2Spawn = new Point(x, y);
                    }
                    else if (clr.R == 255 && clr.G == 255 && clr.B == 0)
                    {
                        NoAppleSpawn[x, y] = true;
                    }
                }
            }
        }

        public static Rectangle GetTileRectanleAt(int tx, int ty)
        {
            return new Rectangle(_tileSize.X * tx, _tileSize.Y * ty, _tileSize.X, _tileSize.Y);
        }

        public void Render(SpriteBatch batch, GameTime time)
        {
            batch.End();

            batch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
            batch.Draw(Art.Sand, Vector2.Zero, new Rectangle(0, 0, _tileSize.X * MapWidth, _tileSize.Y * MapHeight), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            batch.End();

            batch.Begin();

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for(int y = 0;y<_map.GetLength(1);y++)
                {
                    if (_map[x, y] != null)
                    {
                        _map[x, y].Draw(batch, time, GetTileRectanleAt(x, y));
                    }
                }
            }
        }

        public Powerup GetPowerupTile(int x, int y)
        {
            Tile t = GetTileAt(x, y);
            if (t == null)
                return null;

            if (t is Powerup)
                return (Powerup)t;

            return null;
        }

        public void SetTileAt(int x, int y, Tile tile)
        {
            if (x < 0 || x >= _map.GetLength(0) || y < 0 || y >= _map.GetLength(1))
                return;

            _map[x, y] = tile;
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x < 0 || x >= _map.GetLength(0) || y < 0 || y >= _map.GetLength(1))
                return null;

            return _map[x, y];
        }
    }
    public abstract class Tile
    {
        public abstract void Draw(SpriteBatch sb, GameTime time, Rectangle target);
        public abstract bool KillsSnakes();
    }

    public class WallTile : Tile
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            sb.Draw(Art.WallTexture, target, Color.White);
        }

        public override bool KillsSnakes()
        {
            return true;
        }
    }

    public class BushTile : Tile
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            sb.Draw(Art.BushTexture, target, Color.White);
        }

        public override bool KillsSnakes()
        {
            return true;
        }
    }

    public class Powerup : Tile
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            sb.Draw(Art.WallTexture, target, Color.White);
        }

        public override bool KillsSnakes()
        {
            return false;
        }

        public virtual void ActivatePowerup(Snake snake, SnakeGame game)
        { }
    }

    public class SpeedBoostPowerup : Powerup
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.PowerBoost, target, Color.White);
        }

        public override void ActivatePowerup(Snake snake, SnakeGame game)
        { 
            snake.AddModifier(new SnakeSpeedModifier(0.5f));   
        }
    }

    class SpeedReducePowerup : Powerup
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.PowerSlow, target, Color.White);
        }

        public override void ActivatePowerup(Snake snake, SnakeGame game)
        {
            game.GetOtherSnake(snake).AddModifier(new SnakeSpeedModifier(1.5f));
        }
    }

    public class InputInvertPowerup : Powerup
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.PowerInvert, target, Color.White);
        }

        public override void ActivatePowerup(Snake snake, SnakeGame game)
        {
            game.GetOtherSnake(snake).AddModifier(new SnakeInputInvertModifier());
        }
    }

    public class RageModePowerup : Powerup
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.PowerUpRage, target, Color.White);
        }

        public override void ActivatePowerup(Snake snake, SnakeGame game)
        {
            snake.AddModifier(new SnakeRageModeModifier());
        }
    }

    public class AppleTile : Powerup
    {
        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.Apple, target, Color.White);
        }
    }

    public class GoThroughWallsPowerup : Powerup
    {
        public override void ActivatePowerup(Snake snake, SnakeGame game)
        {
            snake.AddModifier(new SnakeGoThroughWallsModifier());
        }

        public override void Draw(SpriteBatch sb, GameTime time, Rectangle target)
        {
            float scale = ((float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.0f) * 0.1f + 0.1f);

            int move = (int)(target.Width * scale);
            int move2 = (int)(target.Width * scale * 2);
            target = new Rectangle(target.X - move, target.Y - move, target.Width + move2, target.Height + move2);

            sb.Draw(Art.PowerWall, target, Color.White);
        }
    }
}

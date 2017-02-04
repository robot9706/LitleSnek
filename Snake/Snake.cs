using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public abstract class SnakeModifier
    {
        public float TimeLeft;

        public abstract float GetTime();

        public abstract void OnActivate(Snake snake);
        public abstract void OnDeactivate(Snake snake);
    }

    public class SnakeSpeedModifier : SnakeModifier
    {
        private float _mod;

        public SnakeSpeedModifier(float m)
        {
            _mod = m;
        }

        public override float GetTime()
        {
            return 10.0f;
        }

        public override void OnActivate(Snake snake)
        {
            snake.MoveSpeedMultiplier = _mod;
        }

        public override void OnDeactivate(Snake snake)
        {
            snake.MoveSpeedMultiplier = 1.0f;
        }
    }

    public class SnakeInputInvertModifier : SnakeModifier
    {
        public override float GetTime()
        {
            return 10.0f;
        }

        public override void OnActivate(Snake snake)
        {
            snake.InputInvert = true;
        }

        public override void OnDeactivate(Snake snake)
        {
            snake.InputInvert = false;
        }
    }

    public class SnakeRageModeModifier : SnakeModifier
    {
        public override float GetTime()
        {
            return 15.0f;
        }

        public override void OnActivate(Snake snake)
        {
            snake.RageMode = true;
            snake.MoveSpeedMultiplier = 0.75f;
        }

        public override void OnDeactivate(Snake snake)
        {
            snake.RageMode = false;
            snake.MoveSpeedMultiplier = 1.0f;
        }
    }

    public class SnakeGoThroughWallsModifier : SnakeModifier
    {
        public override float GetTime()
        {
            return 10.0f;
        }

        public override void OnActivate(Snake snake)
        {
            snake.CanGoThroughWalls = true;
        }

        public override void OnDeactivate(Snake snake)
        {
            snake.CanGoThroughWalls = false;
        }
    }

    public class Snake
    {
        private Map _map;

        private List<Point> _body;

        private float _headDirection = 0;
        private Point _moveDirection = new Point(0, 1);

        public Point MoveDirection
        {
            get { return _moveDirection; }
        }

        private float _targetHeadDirection = 0;
        private Point _targetDirection = new Point(0, 0);

        private float _moveTime = 0.0f;
        private bool _headAnimation = false;

        private Keys _leftKey;
        private Keys _rightKey;

        private bool _addBodyPart = false;

        private List<SnakeModifier> _modifiers = new List<SnakeModifier>();

        public float MoveSpeedMultiplier = 1.0f;
        public bool InputInvert = false;
        public bool RageMode = false;
        public bool CanGoThroughWalls = false;

        public bool IsDead = false;

        public float TimeSpeedMul = 1.0f;

        public int ApplesInRound = 0;
        public int Score = 0;
        public float Food = 1.0f;

        private Dictionary<Point, float> _directionLookup = new Dictionary<Point, float>()
        {
            { new Point(0, 1), MathHelper.Pi },
            { new Point(0, -1), 0.0f },
            { new Point(1, 0), MathHelper.PiOver2 },
            { new Point(-1, 0), MathHelper.PiOver2 + MathHelper.Pi }
        };

        public Snake(Map map, Point head, Keys leftKey, Keys rightKey)
        {
            _map = map;

            _leftKey = leftKey;
            _rightKey = rightKey;

            _targetHeadDirection = _headDirection;
            _targetDirection = _moveDirection;

            _body = new List<Point>();
            _body.Add(head);
            _body.Add(new Point(head.X, head.Y - 1));
            _body.Add(new Point(head.X, head.Y - 2));
        }

        public Point GetHead()
        {
            return _body[0];
        }

        public List<Point> GetBody()
        {
            return _body;
        }

        public void Update(GameTime time)
        {
            float dt = (float)time.ElapsedGameTime.TotalSeconds;

            Food = MathHelper.Clamp(Food - (dt * 0.01f), 0.0f, 1.0f);
            if (Food <= 0.0f)
            {
                _map.Game.SnakeDied(this);
                return;
            }

            float speedMod = 1.0f;
            if (Food <= 0.15f)
            {
                speedMod *= 0.75f;
            }

            _moveTime += dt;

            if (_moveTime >= 0.35f * MoveSpeedMultiplier * speedMod / TimeSpeedMul)
            {
                _headDirection = _targetHeadDirection;
                _moveDirection = _targetDirection;

                _moveTime = 0.0f;

                _headAnimation = !_headAnimation;

                MoveSnake();
            }

            if (Input.IsKeyPressed((InputInvert ? _rightKey : _leftKey)))
            {
                Vector2 v = new Vector2(_moveDirection.X, _moveDirection.Y);
                v = Vector2.TransformNormal(v, Matrix.CreateRotationZ(-MathHelper.PiOver2));

                _targetHeadDirection = _headDirection - MathHelper.PiOver2;
                _targetDirection = new Point((int)v.X, (int)v.Y);
            }
            else if (Input.IsKeyPressed((InputInvert ? _leftKey : _rightKey)))
            {
                Vector2 v = new Vector2(_moveDirection.X, _moveDirection.Y);
                v = Vector2.TransformNormal(v, Matrix.CreateRotationZ(MathHelper.PiOver2));

                _targetHeadDirection = _headDirection + MathHelper.PiOver2;
                _targetDirection = new Point((int)v.X, (int)v.Y);
            }

            if (_modifiers.Count > 0)
            {
                for (int x = _modifiers.Count - 1; x >= 0; x--)
                {
                    _modifiers[x].TimeLeft -= dt;
                    if(_modifiers[x].TimeLeft <= 0.0f)
                    {
                        _modifiers[x].OnDeactivate(this);
                        _modifiers.RemoveAt(x);
                    }
                }
            }
        }

        private void MoveSnake()
        {
            //Move body & parts
            Point head = (Point)_body[0];

            Point[] ar = _body.ToArray();

            if (!_addBodyPart)
            {
                for (int x = 1; x < _body.Count; x++)
                {
                    _body[x] = ar[x - 1];
                }
            }
            else
            {
                _addBodyPart = false;

                _body.Insert(1, head);
            }
            _body[0] = new Point(head.X + _moveDirection.X, head.Y + _moveDirection.Y);

            if(ar.Contains(_body[0]))
            {
                _map.Game.SnakeDied(this);
                return;
            }

            Snake other = _map.Game.GetOtherSnake(this);
            if(other.GetBody().Contains(_body[0]))
            {
                _map.Game.SnakeDied(this);
                return;
            }

            //Check head collision
            if (RageMode)
            {
                Tile t = _map.GetTileAt(_body[0].X, _body[0].Y);
                if(t is BushTile)
                {
                    _map.SetTileAt(_body[0].X, _body[0].Y, null);
                }
                else if(t is WallTile && !CanGoThroughWalls)
                {
                    _map.Game.SnakeDied(this);
                }
            }
            else
            {
                Tile t = _map.GetTileAt(_body[0].X, _body[0].Y);
                if (t != null)
                {
                    if (t is WallTile)
                    {
                        _map.Game.SnakeDied(this);
                    }
                    else if (t is BushTile)
                    {
                        if (!CanGoThroughWalls)
                        {
                            _map.Game.SnakeDied(this);
                        }
                    }
                    else if (t.KillsSnakes())
                    {
                        _map.Game.SnakeDied(this);
                    }
                }
            }

            Powerup p = _map.GetPowerupTile(_body[0].X, _body[0].Y);
            if(p != null)
            {
                p.ActivatePowerup(this, _map.Game);

                if(p is AppleTile)
                {
                    ApplesInRound++;
                    Food += 0.35f;
                    if (Food > 1.0)
                        Food = 1.0f;
                    _map.Game.OnApplePickup();
                    IncreaseSize();
                }

                _map.SetTileAt(_body[0].X, _body[0].Y, null);
            }
        }

        public void Render(SpriteBatch sb)
        {
            for(int x = 0;x<_body.Count;x++)
            {
                Texture2D tex = null;
                float partRotation = 0.0f;
                if (x == 0) //Head
                {
                    partRotation = _headDirection;
                    tex = (_headAnimation ? Art.SnakeHead2 : Art.SnakeHead);
                }
                else if (x == _body.Count - 1) //Tail
                {
                    tex = Art.SnakeBodyTail;

                    Point p = _body[_body.Count - 1] - _body[_body.Count - 2];

                    if (_directionLookup.ContainsKey(p))
                        partRotation = _directionLookup[p];
                }
                else
                {
                    tex = Art.SnakeBody;

                    Point a = _body[x] - _body[x + 1];
                    Point b = _body[x] - _body[x - 1];

                    if(a.X == -b.X && a.Y == b.Y)
                    {
                        partRotation = MathHelper.PiOver2;
                    }
                    else if (a.X == b.X && a.Y == -b.Y)
                    {
                        partRotation = 0.0f;
                    }
                    else
                    {
                        tex = Art.SnakeBodyL;

                        if ((a.Y < 0 && b.X > 0) || (b.Y < 0 && a.X > 0))
                        {
                            partRotation = MathHelper.Pi;
                        }
                        else if ((a.Y < 0 && b.X < 0) || (b.Y < 0 && a.X < 0))
                        {
                            partRotation = MathHelper.PiOver2;
                        }
                        else if ((a.Y > 0 && b.X < 0) || (b.Y > 0 && a.X < 0))
                        {
                            partRotation = 0.0f;
                        }
                        else if ((a.Y > 0 && b.X > 0) || (b.Y > 0 && a.X > 0))
                        {
                            partRotation = MathHelper.PiOver2 + MathHelper.Pi;
                        }
                    }
                }

                Rectangle target = Map.GetTileRectanleAt(_body[x].X, _body[x].Y);
                Vector2 center = new Vector2((float)Art.SnakeHead.Width / 2.0f, (float)Art.SnakeHead.Height / 2.0f);

                target.X += (int)center.X;
                target.Y += (int)center.Y;

                sb.Draw(tex, target, null, Color.White, partRotation, center, SpriteEffects.None, 0.0f);
            }
        }

        public void AddModifier(SnakeModifier m)
        {
            m.TimeLeft = m.GetTime();

            m.OnActivate(this);

            _modifiers.Add(m);
        }

        public void IncreaseSize()
        {
            _addBodyPart = true;
        }
    }
}

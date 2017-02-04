using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Art
    {
        public static Texture2D WallTexture;
        public static Texture2D BushTexture;
        public static Texture2D SnakeHead;
        public static Texture2D SnakeHead2;
        public static Texture2D SnakeBody;
        public static Texture2D SnakeBodyL;
        public static Texture2D SnakeBodyTail;
        public static Texture2D Apple;
        public static Texture2D AppleGs;
        public static Texture2D Sand;
        public static Texture2D PowerUpRage;
        public static Texture2D PowerInvert;
        public static Texture2D PowerWall;
        public static Texture2D PowerSlow;
        public static Texture2D PowerBoost;
        public static Texture2D CorkBoard;

        public static void Load(ContentManager content)
        {
            WallTexture = content.Load<Texture2D>("wall");
            BushTexture = content.Load<Texture2D>("bush");
            SnakeHead = content.Load<Texture2D>("snake_head");
            SnakeHead2 = content.Load<Texture2D>("snake_head2");
            SnakeBody = content.Load<Texture2D>("snake_body");
            SnakeBodyL = content.Load<Texture2D>("snake_body_l");
            SnakeBodyTail = content.Load<Texture2D>("snake_tail");
            Apple = content.Load<Texture2D>("apple");
            AppleGs = content.Load<Texture2D>("appleB");
            Sand = content.Load<Texture2D>("sand");
            PowerUpRage = content.Load<Texture2D>("rage");
            PowerWall = content.Load<Texture2D>("pu_wall");
            PowerSlow = content.Load<Texture2D>("pu_slow");
            PowerBoost = content.Load<Texture2D>("pu_boost");
            PowerInvert = content.Load<Texture2D>("pu_switch");
            CorkBoard = content.Load<Texture2D>("board");
        }
    }
}

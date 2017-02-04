using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Input
    {
        private static KeyboardState _keyboard;
        private static KeyboardState _lastKeyboard;

        public static void Update()
        {
            _lastKeyboard = _keyboard;
            _keyboard = Keyboard.GetState();
        }

        public static bool IsKeyPressed(Keys k)
        {
            return (_keyboard.IsKeyDown(k) && !_lastKeyboard.IsKeyDown(k));
        }

        public static bool IsKeyDown(Keys k)
        {
            return _keyboard.IsKeyDown(k);
        }
    }
}

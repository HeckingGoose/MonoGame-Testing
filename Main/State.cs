using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class State
    {
        public enum Level
        {
            _3D_Test_Loading,
            _3D_Test_Main
        }
        public enum Key
        {
            Released,
            Pressed,
            Held
        }
        public struct Mouse
        {
            // Variables
            private Point _position;
            private Key _leftState;
            private Key _rightState;
            private Key _middleState;

            // Constructor
            public Mouse()
            {
                _position = new Point();
                _leftState = Key.Released;
                _rightState = Key.Released;
                _middleState = Key.Released;
            }

            // Methods
            public Point Position
            {
                get { return _position; }
                set { _position = value; }
            }
            public Key LeftState
            {
                get { return _leftState; }
                set { _leftState = value; }
            }
            public Key RightState
            {
                get { return _rightState; }
                set { _rightState = value; }
            }
            public Key MiddleState
            {
                get { return _middleState; }
                set { _middleState = value; }
            }
        }
    }
}

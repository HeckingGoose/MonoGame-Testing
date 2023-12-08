using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class Default_Level
    {
        // Variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Constructors
        public Default_Level(
            ref GraphicsDeviceManager graphics,
            ref SpriteBatch spriteBatch
            )
        {
            // Set references to graphics device and spritebatch
            _graphics = graphics;
            _spriteBatch = spriteBatch;


        }

        // Methods
        public void RunLogic()
        {

        }
        public void RunGraphics()
        {

        }
    }
}

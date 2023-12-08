using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Main
{
    public class Wrapper : Game
    {
        // CONST

        private const int TARGET_WIDTH = 960;
        private const int TARGET_HEIGHT = 540;

        // ENDCONST

        // VAR

        // Builtin
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Overlay Definitions
        private Console console;

        // Level Definitions
        private Default_Level defaultLevel;

        // State Management
        private State.Level currentLevel;

        // Texture Tracking
        private Dictionary<string, Texture2D> textures;

        // Font Tracking
        private Dictionary<string, SpriteFont> fonts;

        // ENDVAR
        public Wrapper()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialise Values
            currentLevel = State.Level.Default_Loading;
            textures = new Dictionary<string, Texture2D>();
            fonts = new Dictionary<string, SpriteFont>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            fonts.Add("Arimo_12", Content.Load<SpriteFont>("Arimo_12"));

            // Load Textures
            textures.Add("ButtonSliced", Content.Load<Texture2D>("ButtonSliced"));

            // Initialise Console
            console = new Console(
                ref _graphics,
                ref _spriteBatch,
                fonts["Arimo_12"],
                textures["ButtonSliced"]
                );

            console.ShowConsole();
        }

        protected override void Update(GameTime gameTime)
        {
            // Run logic for current level
            switch (currentLevel)
            {
                // Default Level
                case State.Level.Default_Loading:
                    // Initialise Default Level
                    defaultLevel = new Default_Level(
                        ref _graphics,
                        ref _spriteBatch
                        );
                    // Switch to Default Level
                    currentLevel = State.Level.Default_Main;
                    break;
                case State.Level.Default_Main:
                    defaultLevel.RunLogic(
                        );
                    break;

                // Invalid Level
                default:
                    throw new Exception("Level does not exist!");
            }

            // Run logic for console window (if there is any to run)
            console.RunLogic(
                );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            // Draw current level
            switch (currentLevel)
            {
                // Default Level
                case State.Level.Default_Main:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    defaultLevel.RunGraphics(
                        );
                    break;
            }

            // Draw console window (or don't if it's hidden)
            console.RunGraphics(
                );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
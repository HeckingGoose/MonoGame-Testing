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
        private _3D_Test _3D_Test;

        // State Management
        private State.Level currentLevel;

        // Texture Tracking
        private Dictionary<string, Texture2D> textures;

        // Shader Tracking
        private BasicEffect defaultShader;
        private Dictionary<string, Effect> shaders;

        // Font Tracking
        private Dictionary<string, SpriteFont> fonts;

        // Keyboard Tracking
        private Dictionary<Keys, State.Key> keyMap;

        // Mouse Tracking
        private State.Mouse mouseState;

        // ENDVAR
        public Wrapper()
        {
            _graphics = new GraphicsDeviceManager(this)
            { 
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialise Values
            currentLevel = State.Level._3D_Test_Loading;
            textures = new Dictionary<string, Texture2D>();
            fonts = new Dictionary<string, SpriteFont>();
            shaders = new Dictionary<string, Effect>();
            mouseState = new State.Mouse();

            // Setup keymap
            keyMap = new Dictionary<Keys, State.Key>
            {
                { Keys.OemTilde, State.Key.Released }
            };

            // Set polygon drawing mode
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.WireFrame;
            _graphics.GraphicsDevice.RasterizerState = state;

            // Setup basic default shader
            defaultShader = new BasicEffect(GraphicsDevice);
            defaultShader.Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2, GraphicsDevice.Viewport.AspectRatio, 0.001f, 1000.0f);
            defaultShader.View = Matrix.CreateLookAt(new Vector3(0, 0, -5), Vector3.Forward, Vector3.Up);
            defaultShader.World = Matrix.Identity;
            defaultShader.VertexColorEnabled = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            fonts.Add("Candara_12", Content.Load<SpriteFont>(@"Fon\Candara_12"));

            // Load Textures
            textures.Add("ButtonSliced", Content.Load<Texture2D>(@"Tex\ButtonSliced"));

            // Load Shaders
            shaders.Add("TileTexture", Content.Load<Effect>(@"Shd\TileTexture"));
            shaders.Add("UnlitDefault", Content.Load<Effect>(@"Shd\UnlitDefault"));

            // Initialise Console
            console = new Console(
                ref _graphics,
                ref _spriteBatch,
                fonts["Candara_12"],
                textures["ButtonSliced"],
                new Rectangle(
                    60,
                    60,
                    _graphics.PreferredBackBufferWidth - 40,
                    _graphics.PreferredBackBufferHeight - 40
                    ),
                shaders["TileTexture"]
                );
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle keyboard input
            KeyboardState boardState = Keyboard.GetState();

            // Get mouse input
            MouseState mouseGet = Mouse.GetState();
            mouseState.Position = mouseGet.Position;

            // Loop through every key in the keymap
            foreach (KeyValuePair<Keys, State.Key> kvp in keyMap)
            {
                // If the key is pressed
                if (boardState.IsKeyDown(kvp.Key))
                {
                    if (keyMap[kvp.Key] != State.Key.Held)
                    {
                        keyMap[kvp.Key]++;
                    }
                }
                // If the key was released
                else
                {
                    keyMap[kvp.Key] = State.Key.Released;
                }
            }

            // Understand mouse input
            void HandleMouseButton(in ButtonState buttonState, ref State.Key mouseButton)
            {
                // If mouse is currently down
                if (buttonState == ButtonState.Pressed)
                {
                    switch (mouseButton)
                    {
                        // If mouse is not pressed yet
                        case State.Key.Released:
                            // Set mouse as pressed
                            mouseButton = State.Key.Pressed;
                            return;
                        case State.Key.Pressed:
                            // Set mouse as held
                            mouseButton = State.Key.Held;
                            return;
                    }
                }
                // If mouse is not down
                else
                {
                    mouseButton = State.Key.Released;
                    return;
                }
            }
            State.Key temp = mouseState.LeftState;
            HandleMouseButton(mouseGet.LeftButton, ref temp);
            mouseState.LeftState = temp;
            temp = mouseState.RightState;
            HandleMouseButton(mouseGet.RightButton, ref temp);
            mouseState.RightState = temp;
            temp = mouseState.MiddleState;
            HandleMouseButton(mouseGet.MiddleButton, ref temp);
            mouseState.MiddleState = temp;

            // Run logic for current level
            switch (currentLevel)
            {
                // Default Level
                case State.Level._3D_Test_Loading:
                    // Initialise Default Level
                    _3D_Test = new _3D_Test(
                        ref _graphics,
                        ref _spriteBatch,
                        defaultShader
                        );
                    // Switch to Default Level
                    currentLevel = State.Level._3D_Test_Main;
                    break;
                case State.Level._3D_Test_Main:
                    _3D_Test.RunLogic(
                        );
                    break;

                // Invalid Level
                default:
                    throw new Exception("Level does not exist!");
            }

            // Add message
            console.Write($"Hello, I'm an error message with one {FancyBox.PRELIMINARY_CHARACTER}text-colour{FancyBox.SPLITTING_CHARACTER}{FancyBox.CreateValidValueString(Color.Blue.ToVector4().ToString())}{FancyBox.POSTLIMINARY_CHARACTER}blue word!", Console.Type.Error);


            // Run logic for console window (if there is any to run)
            console.RunLogic(
                in keyMap,
                in mouseState
                );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Sanity call
            _spriteBatch.Begin();
            _spriteBatch.End();

            // Draw current level
            switch (currentLevel)
            {
                // Default Level
                case State.Level._3D_Test_Main:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _3D_Test.RunGraphics(
                        );
                    break;
            }

            // Draw console window (or don't if it's hidden)
            console.RunGraphics(
                );

            base.Draw(gameTime);
        }
    }
}
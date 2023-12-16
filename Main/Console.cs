using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class Console
    {
        // Classes
        private class ConsoleMessage
        {
            // Variables
            private string _message;
            private Type _messageType;

            // Constructors
            public ConsoleMessage(
                string message,
                Type messageType
                )
            {
                _message = message;
                _messageType = messageType;
            }

            // Methods
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }
            public Type MessageType
            {
                get { return _messageType; }
                set { _messageType = value; }
            }
        }
        public class BuiltIn
        {
            public string TextureNotLoaded(string textureName) { return $"Texture {textureName} is not currently loaded!"; }
        }

        // Constants
        public enum Type
        {
            Log,
            Warn,
            Error
        }

        // Variables
        // Builtin
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Font
        private SpriteFont _font;

        // Queue Tracking
        private Queue<ConsoleMessage> _messages;
        private string _messagesAsString;
        private uint _queueLength;
        private Vector2 _queueSize;

        // Window Tracking
        private Rectangle _window;
        private Rectangle _minMaxSize;

        // Console State Tracking
        private bool _shown;

        // Console Texture Management
        private Texture2D _baseTexture;
        private Texture2D _windowTexture;
        private Effect _tileTextureShader;

        // Cross rect management
        private Rectangle _closeButtonRect;

        // Constructors
        public Console(
            ref GraphicsDeviceManager graphics,
            ref SpriteBatch spriteBatch,
            SpriteFont font,
            Texture2D baseTexture,
            Rectangle minMaxSize,
            Effect tileTextureShader,
            int defaultX = 20,
            int defaultY = 20,
            Vector2 defaultSize = new Vector2()
            )
        {
            // Set references to graphics device and spritebatch
            _graphics = graphics;
            _spriteBatch = spriteBatch;

            // Store Font
            _font = font;

            // Store Texture
            _baseTexture = baseTexture;

            // Initialise values
            _messages = new Queue<ConsoleMessage>();
            _messagesAsString = string.Empty;
            _queueLength = (uint)_messages.Count;
            _shown = true;
            _minMaxSize = minMaxSize;
            _tileTextureShader = tileTextureShader;

            // Declare window
            if (defaultSize == Vector2.Zero)
            {
                _window = new Rectangle(
                    defaultX,
                    defaultY,
                    _graphics.PreferredBackBufferWidth / 2,
                    _graphics.PreferredBackBufferHeight / 2
                    );
            }
            else
            {
                _window = new Rectangle(
                    defaultX,
                    defaultY,
                    (int)defaultSize.X,
                    (int)defaultSize.Y
                    );
            }

            // Generate close button rect
            Vector2 temp = _font.MeasureString("X");
            _closeButtonRect = new Rectangle(
                _window.X + _window.Width - (int)temp.X - 10,
                _window.Y + 2,
                (int)temp.X,
                (int)temp.Y
                );

            // Size window texture
            _windowTexture = new Texture2D(_graphics.GraphicsDevice, _window.Width, _window.Height, false, SurfaceFormat.Color, ShaderAccess.ReadWrite);
            _tileTextureShader.Parameters["InputTexture"].SetValue(_baseTexture);
            _tileTextureShader.Parameters["OutputTexture"].SetValue(_windowTexture);
            foreach (var pass in _tileTextureShader.CurrentTechnique.Passes)
            {
                pass.ApplyCompute();
                _graphics.GraphicsDevice.DispatchCompute((int)Math.Ceiling((float)baseTexture.Width / 8), (int)Math.Ceiling((float)baseTexture.Height / 8), 1);
            }
        }

        // Methods
        public void RunLogic(
            in Dictionary<Keys, State.Key> keyMap,
            in State.Mouse mouseState
            )
        {
            // If the console needs to be toggled
            if (keyMap[Keys.OemTilde] == State.Key.Pressed)
            {
                // Toggle console
                _shown = !_shown;
            }

            // Console close button functionality
            if (
                _shown
                && _closeButtonRect.Intersects(new Rectangle(mouseState.Position, Point.Zero))
                && mouseState.LeftState == State.Key.Pressed
                )
            {
                _shown = false;
            }

            // If the number of messages has changed
            if (_queueLength != (uint)_messages.Count)
            {
                // Re-evaluate the value of messagesAsString
                foreach (ConsoleMessage message in _messages)
                {
                    _messagesAsString += message.Message + "\n";
                }

                // Raise queue re-measure
                _queueSize = _font.MeasureString(_messagesAsString);
            }
        }
        public void RunGraphics()
        {
            if (_shown)
            {
                // Draw console window
                _spriteBatch.Draw(_windowTexture, _window, Color.White);

                // Draw console title
                _spriteBatch.DrawString(_font, "Console", new Vector2(_window.X + 15, _window.Y + 2), new Color(1f, 1f, 1f, 0.75f));

                // Draw console X
                _spriteBatch.DrawString(_font, "X", new Vector2(_closeButtonRect.X, _closeButtonRect.Y), new Color(1f, 1f, 1f, 0.75f));
            }
        }

        public void Write(string message, Type type = Type.Log)
        {
            _messages.Enqueue(new ConsoleMessage(message, type));
        }

        #region Handle Console state
        /// <summary>
        /// Toggle whether the console is drawn or not.
        /// </summary>
        public void ToggleConsole()
        {
            _shown = !_shown;
        }
        /// <summary>
        /// Set the console to be drawn.
        /// </summary>
        public void ShowConsole()
        {
            _shown = true;
        }
        /// <summary>
        /// Set the console to not be drawn.
        /// </summary>
        public void HideConsole()
        {
            _shown = false;
        }
        #endregion
        private Texture2D GenerateTiledTexture(Texture2D inputTexture, int targetWidth, int targetHeight)
        {
            // Values that may be worth changing at a later date
            const int SUBDIVISIONS = 3;

            // Set values
            Texture2D outputTexture = new Texture2D(_graphics.GraphicsDevice, targetWidth, targetHeight);
            int subDivisionSize = inputTexture.Width / SUBDIVISIONS;

            // Store input texture data as array
            Color[] textureData = new Color[inputTexture.Height * inputTexture.Width];
            inputTexture.GetData(textureData);

            // Do top-left
            StretchRect(
                new Rectangle(
                    0,
                    0,
                    subDivisionSize,
                    subDivisionSize
                    ),
                new Rectangle(
                    0,
                    0,
                    subDivisionSize,
                    subDivisionSize
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do top-right
            StretchRect(
                new Rectangle(
                    inputTexture.Width - subDivisionSize,
                    0,
                    subDivisionSize,
                    subDivisionSize
                    ),
                new Rectangle(
                    outputTexture.Width - subDivisionSize,
                    0,
                    subDivisionSize,
                    subDivisionSize
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do bottom-left
            StretchRect(
                new Rectangle(
                    0,
                    inputTexture.Height - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize
                    ),
                new Rectangle(
                    0,
                    outputTexture.Height - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do bottom-right
            StretchRect(
                new Rectangle(
                    inputTexture.Width - subDivisionSize,
                    inputTexture.Height - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize
                    ),
                new Rectangle(
                    outputTexture.Width - subDivisionSize,
                    outputTexture.Height - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do top
            StretchRect(
                new Rectangle(
                    subDivisionSize,
                    0,
                    inputTexture.Width - subDivisionSize * 2,
                    subDivisionSize
                ),
                new Rectangle(
                    subDivisionSize,
                    0,
                    outputTexture.Width - subDivisionSize * 2,
                    subDivisionSize
                ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do bottom
            StretchRect(
                new Rectangle(
                    subDivisionSize,
                    inputTexture.Height - subDivisionSize,
                    inputTexture.Width - subDivisionSize * 2,
                    subDivisionSize
                ),
                new Rectangle(
                    subDivisionSize,
                    outputTexture.Height - subDivisionSize,
                    outputTexture.Width - subDivisionSize * 2,
                    subDivisionSize
                ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do left
            StretchRect(
                new Rectangle(
                    0,
                    subDivisionSize,
                    subDivisionSize,
                    inputTexture.Height - subDivisionSize * 2
                    ),
                new Rectangle(
                    0,
                    subDivisionSize,
                    subDivisionSize,
                    outputTexture.Height - subDivisionSize * 2
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do right
            StretchRect(
                new Rectangle(
                    inputTexture.Width - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize,
                    inputTexture.Height - subDivisionSize * 2
                    ),
                new Rectangle(
                    outputTexture.Width - subDivisionSize,
                    subDivisionSize,
                    subDivisionSize,
                    outputTexture.Height - subDivisionSize * 2
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Do Middle
            StretchRect(
                new Rectangle(
                    subDivisionSize,
                    subDivisionSize,
                    inputTexture.Width - subDivisionSize * 2,
                    inputTexture.Height - subDivisionSize * 2
                    ),
                new Rectangle(
                    subDivisionSize,
                    subDivisionSize,
                    outputTexture.Width - subDivisionSize * 2,
                    outputTexture.Height - subDivisionSize * 2
                    ),
                in inputTexture,
                in textureData,
                ref outputTexture
                );

            // Return result
            return outputTexture;
        }
        private void StretchRect(
            Rectangle baseRect,
            Rectangle stretchRect,
            in Texture2D inputTexture,
            in Color[] textureData,
            ref Texture2D outputTexture
            )
        {
            // Calculate stretch factor and remaining pixels
            Rectangle stretch = new Rectangle(
                stretchRect.Width / baseRect.Width,
                stretchRect.Height / baseRect.Height,
                stretchRect.Width % baseRect.Width,
                stretchRect.Height % baseRect.Height
                );

            // Copy across texture data
            for (int y = 0; y < baseRect.Height; y++)
            {
                for (int sy = 0; sy < (y + 1 != baseRect.Height ? stretch.Y : stretch.Y + stretch.Height); sy++)
                {
                    for (int x = 0; x < baseRect.Width; x++) // Loop through the rect representing the top of the texture
                    {
                        // Declare colour arrays
                        Color[] dataCache = x + 1 != baseRect.Width ? new Color[stretch.X] : new Color[stretch.X + stretch.Width];

                        for (int sx = 0; sx < dataCache.Length; sx++) // Loop as many times as there is stretch factor
                        {
                            // Fetch correct pixel for top
                            dataCache[sx] = textureData[baseRect.X + x + inputTexture.Width * y + baseRect.Y * inputTexture.Width];
                        }

                        // Apply stretch
                        outputTexture.SetData(
                            0,
                            new Rectangle(
                                stretchRect.X + (x * stretch.X),
                                stretchRect.Y + (y * stretch.Y) + sy,
                                dataCache.Length,
                                1
                                ),
                            dataCache,
                            0,
                            dataCache.Length
                            );
                    }
                }
            }
        }
    }
}

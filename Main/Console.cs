using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        // Console State Tracking
        private Rectangle _window;
        private bool _shown;

        // Console Texture Management
        private Texture2D _baseTexture;
        private Texture2D _windowTexture;

        // Constructors
        public Console(
            ref GraphicsDeviceManager graphics,
            ref SpriteBatch spriteBatch,
            SpriteFont font,
            Texture2D baseTexture
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
            _window = new Rectangle(
                20,
                20,
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
                );

            // Size window texture
            _windowTexture = GenerateTiledTexture(baseTexture, _window.Width, _window.Height);
        }

        // Methods
        public void RunLogic()
        {
            if (_queueLength != (uint)_messages.Count)
            {
                // Re-evaluate the value of messagesAsString
                foreach (ConsoleMessage message in _messages)
                {
                    _messagesAsString += message.Message + "\n";
                }

                // Raise queue re-measure
                _font.MeasureString(_messagesAsString);
            }
        }
        public void RunGraphics()
        {
            if (_shown)
            {
                // Draw console
                _spriteBatch.Draw(_windowTexture, _window, Color.White);
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

            // Draw top left, top right, bottom left and bottom right parts of texture
            for (int i = 0; i < subDivisionSize; i++)
            {
                // Define texture data cache
                Color[] dataCache = new Color[subDivisionSize];

                // Top left
                Array.Copy(
                    textureData,
                    i * inputTexture.Width,
                    dataCache,
                    0,
                    subDivisionSize
                    );
                outputTexture.SetData(
                    0,
                    new Rectangle(
                        0,
                        i,
                        subDivisionSize,
                        1
                        ),
                    dataCache,
                    0,
                    subDivisionSize
                    );

                // Top right
                Array.Copy(
                    textureData,
                    i * inputTexture.Width + inputTexture.Width - 1 - subDivisionSize,
                    dataCache,
                    0,
                    subDivisionSize
                    );
                outputTexture.SetData(
                    0,
                    new Rectangle(
                        outputTexture.Width - subDivisionSize - 1,
                        i,
                        subDivisionSize,
                        1
                        ),
                    dataCache,
                    0,
                    subDivisionSize
                    );

                // Bottom left
                Array.Copy(
                    textureData,
                    textureData.Length - (subDivisionSize - i) * inputTexture.Width,
                    dataCache,
                    0,
                    subDivisionSize
                    );
                outputTexture.SetData(
                    0,
                    new Rectangle(
                        0,
                        outputTexture.Height - 1 - subDivisionSize + i,
                        subDivisionSize,
                        1
                        ),
                    dataCache,
                    0,
                    subDivisionSize
                    );

                // Bottom right
                Array.Copy(
                    textureData,
                    textureData.Length - (subDivisionSize - i - 1) * inputTexture.Width - subDivisionSize,
                    dataCache,
                    0,
                    subDivisionSize
                    );
                outputTexture.SetData(
                    0,
                    new Rectangle(
                        outputTexture.Width - 1 - subDivisionSize,
                        outputTexture.Height - 1 - subDivisionSize + i,
                        subDivisionSize,
                        1
                        ),
                    dataCache,
                    0,
                    subDivisionSize
                    );
            }

            // Return result
            return outputTexture;
        }
    }
}

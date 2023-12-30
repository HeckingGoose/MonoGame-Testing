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

        // Width of edge rect for detecting mouse hover
        private const int EDGE_WIDTH = 16;

        // Subdivision size
        private const int SUBDIVISIONS = 3;

        // Default colours
        private Color DEFAULT_COLOUR = Color.White;
        private Color WARNING_COLOUR = new Color(245, 232, 89);
        private Color ERROR_COLOUR = new Color(230, 31, 34);

        // Variables
        // Builtin
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Font
        private SpriteFont _font;

        // Text box
        private FancyBox _fancyBox;

        // Queue Tracking
        private Queue<ConsoleMessage> _messages;
        private string _messagesAsString;
        private uint _queueLength;
        private uint _maxQueueSize;

        // Window Tracking
        private Rectangle _windowLastFrame;
        private Rectangle _window;
        private Rectangle _minMaxSize;

        // Console drag tracking
        private bool _drag = false;
        private Point _dragOffset = Point.Zero;

        // Console State Tracking
        private bool _shown;

        // Console Texture Management
        private Texture2D _baseTexture;
        private Texture2D _windowTexture;
        private Effect _tileTextureShader;

        // Cross rect management
        private Rectangle _closeButtonRect;

        // Edge rect management
        private Dictionary<string, Rectangle> _edgeRects;

        // Resize mode
        private string _resizeMode;

        // Stencil States
        private DepthStencilState _s1;
        private DepthStencilState _s2;
        private Texture2D _bufferTex;

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
            Vector2 defaultSize = new Vector2(),
            uint maxQueueSize = 50
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
            _maxQueueSize = maxQueueSize;
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

            // Copy window info
            _windowLastFrame = _window;

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
                _graphics.GraphicsDevice.DispatchCompute((int)Math.Ceiling((float)_baseTexture.Width / 8), (int)Math.Ceiling((float)_baseTexture.Height / 8), 1);
            }

            // Create new fancyBox
            _fancyBox = new FancyBox(
                new Rectangle(
                    _window.X + _baseTexture.Width / SUBDIVISIONS,
                    _window.Y + _baseTexture.Height / SUBDIVISIONS,
                    _window.Width - 2 * (_baseTexture.Width / SUBDIVISIONS),
                    _window.Height - 2 * (_baseTexture.Height / SUBDIVISIONS)
                    ),
                String.Empty,
                _font,
                DEFAULT_COLOUR
                );

            // Generate edge rects
            _edgeRects = GenerateEdgeRects();

            // Set resize mode
            _resizeMode = String.Empty;

            // Create stencil states
            _s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            _s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            _bufferTex = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _bufferTex.SetData(new Color[] { Color.White });
        }

        // Methods
        public void RunLogic( // Add code for generating rects when console size changes
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

            // If the console is shown
            if (_shown)
            {
                // If the left mouse button is pressed
                if (mouseState.LeftState == State.Key.Pressed)
                {
                    // If the close button is pressed
                    if (_closeButtonRect.Intersects(new Rectangle(mouseState.Position, Point.Zero)))
                    {
                        // Hide the console
                        _shown = false;
                    }

                    // If the console is being moved
                    else if (_edgeRects["TopBar"].Intersects(new Rectangle(mouseState.Position, Point.Zero)))
                    {
                        // Set the console state to being dragged
                        _drag = true;

                        // Calculate drag offset
                        _dragOffset = new Point(
                            _window.X - mouseState.Position.X,
                            _window.Y - mouseState.Position.Y
                            );
                    }

                    // Otherwise
                    else
                    {
                        // Check if the mouse is attempting to resize the console
                        foreach (KeyValuePair<string, Rectangle> kvp in _edgeRects)
                        {
                            // Is the mouse intersecting with this rect?
                            if (kvp.Value.Intersects(new Rectangle(mouseState.Position, Point.Zero)))
                            {
                                // Set the resize mode to the key value (e.g. TopMiddle)
                                _resizeMode = kvp.Key;

                                // Exit the loop to save time
                                break;
                            }
                        }
                    }
                }
                // If the mouse is released
                else if (mouseState.LeftState == State.Key.Released)
                {
                    // State that we are no longer dragging the window
                    _drag = false;

                    // Reset resize mode
                    _resizeMode = String.Empty;

                    // Generate new rects
                    _edgeRects = GenerateEdgeRects();
                }
            }

            // Handle window dragging
            ManageDrag(mouseState);

            // Handle window resizing
            ManageResize(mouseState);

            // If the number of messages has changed
            if (_queueLength != (uint)_messages.Count)
            {
                // Reset messages as string
                _messagesAsString = String.Empty;

                // Re-evaluate the value of messagesAsString
                foreach (ConsoleMessage message in _messages)
                {
                    _messagesAsString += $"\n {message.Message} ";
                }
                
                // Pass this into fancy box
                _fancyBox.Message = _messagesAsString;

                _queueLength = (uint)_messages.Count;
            }

            // Were any changes to the window made?
            if (_windowLastFrame != _window)
            {
                // Generate new cross rect
                Vector2 temp = _font.MeasureString("X");
                _closeButtonRect = new Rectangle(
                    _window.X + _window.Width - (int)temp.X - 10,
                    _window.Y + 2,
                    (int)temp.X,
                    (int)temp.Y
                    );

                // Size window texture
                _windowTexture.Dispose();
                _windowTexture = new Texture2D(_graphics.GraphicsDevice, _window.Width, _window.Height, false, SurfaceFormat.Color, ShaderAccess.ReadWrite);
                _tileTextureShader.Parameters["InputTexture"].SetValue(_baseTexture);
                _tileTextureShader.Parameters["OutputTexture"].SetValue(_windowTexture);
                foreach (var pass in _tileTextureShader.CurrentTechnique.Passes)
                {
                    pass.ApplyCompute();
                    _graphics.GraphicsDevice.DispatchCompute((int)Math.Ceiling((float)_baseTexture.Width / 8), (int)Math.Ceiling((float)_baseTexture.Height / 8), 1);
                }
            }

            // Copy window size
            _windowLastFrame = _window;
        }
        public void RunGraphics()
        {
            if (_shown)
            {
                // Start draw
                _spriteBatch.Begin();

                // Draw console window
                _spriteBatch.Draw(_windowTexture, _window, Color.White);

                // Draw console title
                _spriteBatch.DrawString(_font, "Console", new Vector2(_window.X + 15, _window.Y + 2), new Color(1f, 1f, 1f, 0.75f));

                // Draw console X
                _spriteBatch.DrawString(_font, "X", new Vector2(_closeButtonRect.X, _closeButtonRect.Y), new Color(1f, 1f, 1f, 0.75f));

                // End draw
                _spriteBatch.End();

                // Draw stencil buffer
                _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, _s1);
                _spriteBatch.Draw(
                    _bufferTex,
                    new Rectangle(
                        _window.X + (_baseTexture.Width / SUBDIVISIONS),
                        _window.Y + (_baseTexture.Height / SUBDIVISIONS),
                        _window.Width - 2 * (_baseTexture.Width / SUBDIVISIONS),
                        _window.Height - 2 * (_baseTexture.Height / SUBDIVISIONS)
                        ),
                    new Color(1, 1, 1, 0)
                    );
                _spriteBatch.End();

                // Prep to draw text
                _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, _s2);

                // Draw fancy box text
                _fancyBox.DrawBox(
                    new Vector2(
                        _fancyBox.Rect.X,
                        _window.Y + _window.Height - 2 * (_baseTexture.Height / SUBDIVISIONS) - _fancyBox.MessageSize.Y
                        ),
                    _spriteBatch
                    );

                _spriteBatch.End();
            }
        }

        public void Write(string message, Type type = Type.Log)
        {
            // Append extra stuff if needed
            switch (type)
            {
                case Type.Warn:
                    message = $"{FancyBox.PRELIMINARY_CHARACTER}text-colour{FancyBox.SPLITTING_CHARACTER}{FancyBox.CreateValidValueString(WARNING_COLOUR.ToVector4().ToString())}{FancyBox.POSTLIMINARY_CHARACTER}Warning: " + message;
                    break;
                case Type.Error:
                    message = $"{FancyBox.PRELIMINARY_CHARACTER}text-colour{FancyBox.SPLITTING_CHARACTER}{FancyBox.CreateValidValueString(ERROR_COLOUR.ToVector4().ToString())}{FancyBox.POSTLIMINARY_CHARACTER}Error: " + message;
                    break;
            }

            _messages.Enqueue(new ConsoleMessage(message, type));

            if (_queueLength > _maxQueueSize)
            {
                _messages.Dequeue();
            }
        }
        private Dictionary<string, Rectangle> GenerateEdgeRects()
        {
            // Create new dictionary
            Dictionary<string, Rectangle> edgeRects = new Dictionary<string, Rectangle>
            {
                {
                    "TopBar",
                    new Rectangle(
                        _window.X,
                        _window.Y,
                        _window.Width,
                        _baseTexture.Height / SUBDIVISIONS
                        )
                },
                {
                    "TopLeft",
                    new Rectangle(
                        _window.X - (EDGE_WIDTH / 2),
                        _window.Y - (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                },
                {
                    "TopMiddle",
                    new Rectangle(
                        _window.X + (EDGE_WIDTH / 2),
                        _window.Y - (EDGE_WIDTH / 2),
                        _window.Width - EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                },
                {
                    "TopRight",
                    new Rectangle(
                        _window.X + _window.Width - (EDGE_WIDTH / 2),
                        _window.Y - (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                },
                {
                    "MiddleLeft",
                    new Rectangle(
                        _window.X - (EDGE_WIDTH / 2),
                        _window.Y + (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        _window.Height - EDGE_WIDTH
                        )
                },
                {
                    "MiddleRight",
                    new Rectangle(
                        _window.X + _window.Width - (EDGE_WIDTH / 2),
                        _window.Y + (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        _window.Height - EDGE_WIDTH
                        )
                },
                {
                    "BottomLeft",
                    new Rectangle(
                        _window.X - (EDGE_WIDTH / 2),
                        _window.Y + _window.Height - (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                },
                {
                    "BottomMiddle",
                    new Rectangle(
                        _window.X + (EDGE_WIDTH / 2),
                        _window.Y + _window.Height - (EDGE_WIDTH / 2),
                        _window.Width - EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                },
                {
                    "BottomRight",
                    new Rectangle(
                        _window.X + _window.Width - (EDGE_WIDTH / 2),
                        _window.Y + _window.Height - (EDGE_WIDTH / 2),
                        EDGE_WIDTH,
                        EDGE_WIDTH
                        )
                }
            };

            // Return result
            return edgeRects;
        }
        private void ManageDrag(State.Mouse mouseState)
        {
            // Is the window being dragged?
            if (_drag)
            {
                // Move console window to mouse pos relative on the current drag offset
                _window.X = mouseState.Position.X + _dragOffset.X;
                _window.Y = mouseState.Position.Y + _dragOffset.Y;
            }

            // Pass this info to fancybox
            _fancyBox.Rect = new Rectangle(
                _window.X + _baseTexture.Width / SUBDIVISIONS,
                _window.Y + _baseTexture.Height / SUBDIVISIONS,
                _window.Width - 2 * (_baseTexture.Width / SUBDIVISIONS),
                _window.Height - 2 * (_baseTexture.Height / SUBDIVISIONS)
                );
        }
        private void ManageResize(State.Mouse mouseState)
        {
            // Is the window being resized?
            if (_resizeMode != String.Empty)
            {
                // Declare position change variables
                int sizeChangeX = 0;
                int sizeChangeY = 0;
                Point temp;

                // Figure out what resize we should do
                switch (_resizeMode)
                {
                    case "TopLeft":
                        // Calculate position changes
                        sizeChangeX = _window.X - mouseState.Position.X;
                        sizeChangeY = _window.Y - mouseState.Position.Y;

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            _window.Height + sizeChangeY
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;

                            // Set origin to new position
                            _window.X = mouseState.Position.X;
                        }
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;

                            // Set origin to new position
                            _window.Y = mouseState.Position.Y;
                        }
                        break;
                    case "TopMiddle":
                        // Calculate position change
                        sizeChangeY = _window.Y - mouseState.Position.Y;

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            0,
                            _window.Height + sizeChangeY
                            );

                        // | Check if point is valid
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;

                            // Set origin to new position
                            _window.Y = mouseState.Position.Y;
                        }
                        break;
                    case "TopRight":
                        // Calculate position changes
                        sizeChangeX = mouseState.Position.X - (_window.X + _window.Width);
                        sizeChangeY = _window.Y - mouseState.Position.Y;

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            _window.Height + sizeChangeY
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;
                        }
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            && temp.Y != _window.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;

                            // Set origin to new position
                            _window.Y = mouseState.Position.Y;
                        }
                        break;
                    case "MiddleLeft":
                        // Calculate position changes
                        sizeChangeX = _window.X - mouseState.Position.X;

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            0
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;

                            // Apply origin change
                            _window.X = mouseState.Position.X;
                        }
                        break;
                    case "MiddleRight":
                        // Calculate position changes
                        sizeChangeX = mouseState.Position.X - (_window.X + _window.Width);

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            0
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;
                        }
                        break;
                    case "BottomLeft":
                        // Calculate position changes
                        sizeChangeX = _window.X - mouseState.Position.X;
                        sizeChangeY = mouseState.Position.Y - (_window.Y + _window.Height);

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            _window.Height + sizeChangeY
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;

                            // Set origin to new position
                            _window.X = mouseState.Position.X;
                        }
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;
                        }
                        break;
                    case "BottomMiddle":
                        // Calculate position changes
                        sizeChangeY = mouseState.Position.Y - (_window.Y + _window.Height);

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            _window.Height + sizeChangeY
                            );
                        // | Check if point is valid
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;
                        }
                        break;
                    case "BottomRight":
                        // Calculate position changes
                        sizeChangeX = mouseState.Position.X - (_window.X + _window.Width);
                        sizeChangeY = mouseState.Position.Y - (_window.Y + _window.Height);

                        // Apply size change
                        // | Create temp point
                        temp = new Point(
                            _window.Width + sizeChangeX,
                            _window.Height + sizeChangeY
                            );
                        // | Check if point is valid
                        if (
                            temp.X <= _minMaxSize.Width
                            && temp.X >= _minMaxSize.X
                            )
                        {
                            // Apply size change
                            _window.Width += sizeChangeX;
                        }
                        if (
                            temp.Y <= _minMaxSize.Height
                            && temp.Y >= _minMaxSize.Y
                            )
                        {
                            // Apply size change
                            _window.Height += sizeChangeY;
                        }
                        break;
                }

                // Pass this info to fancybox
                _fancyBox.Rect = new Rectangle(
                    _window.X + _baseTexture.Width / SUBDIVISIONS,
                    _window.Y + _baseTexture.Height / SUBDIVISIONS,
                    _window.Width - 2 * (_baseTexture.Width / SUBDIVISIONS),
                    _window.Height - 2 * (_baseTexture.Height / SUBDIVISIONS)
                    );
            }
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
        #region Legacy Functions
        private Texture2D CPU_GenerateTiledTexture(Texture2D inputTexture, int targetWidth, int targetHeight)
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
            CPU_StretchRect(
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
        private void CPU_StretchRect(
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
        #endregion
    }
}

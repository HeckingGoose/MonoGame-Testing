using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Main
{
    public class FancyBox
    {
        // Constants
        public const char POSTLIMINARY_CHARACTER = '┤';
        public const char SPLITTING_CHARACTER = '┼';
        public const char PRELIMINARY_CHARACTER = '├';

        // Variables
        private Rectangle _rect;
        private string _message;
        private SpriteFont _font;
        private Color _defaultTextColour;

        private string _messageFormatted;
        private Vector2 _messageSizeCache;

        // Constructors
        public FancyBox() // Empty Constructor
        {
            // Default Values
            _rect = new Rectangle();
            _message = string.Empty;
            _font = null;
            _defaultTextColour = Color.White;

            _messageFormatted = string.Empty;
            _messageSizeCache = Vector2.Zero;
        }
        public FancyBox(Rectangle rect, SpriteFont font, Color defaultTextColour) // Constructor for empty message
        {
            // Pass in value and set rest to default
            _rect = rect;
            _message = string.Empty;
            _font = font;
            _defaultTextColour = defaultTextColour;

            _messageFormatted = string.Empty;
            _messageSizeCache = Vector2.Zero;
        }
        public FancyBox( // Filled constructor
            Rectangle rect,
            string message,
            SpriteFont font,
            Color defaultTextColour
            )
        {
            // Pass in values
            _rect = rect;
            _message = message;
            _font = font;
            _defaultTextColour = defaultTextColour;

            // Perform message re-measure
            _messageSizeCache = MeasureAndFormatMessage(in _message, out _messageFormatted, _rect.Width);
        }

        // Methods
        private Vector2 MeasureAndFormatMessage(in string message, out string messageFormatted, int width)
        {
            // Initialise output
            messageFormatted = String.Empty;

            // Pre-measure a space
            float spaceWidth = _font.MeasureString(" ").X;

            // Setup size tracking
            Vector2 currentLineSize = Vector2.Zero;
            Vector2 outputSize = Vector2.Zero;

            // Seperate message into individual words
            string[] words = message.Split(' ');

            // Loop through every word
            foreach (string word in words)
            {
                // Variables to describe each part of the word
                string text = String.Empty;
                Color colour = _defaultTextColour;
                Vector2 position = Vector2.Zero;

                // Parse word
                ParseWord(
                    in word,
                    out text,
                    out colour,
                    out _
                    );

                // Get word size
                Vector2 wordSize = _font.MeasureString(text);

                // Is this the tallest word on the line?
                if (wordSize.Y > currentLineSize.Y)
                {
                    currentLineSize.Y = wordSize.Y;
                }
                
                // Is the word just a new line?
                if (text == "\n" || text == "\r" || text == "\r\n")
                {
                    // Add new line to output
                    messageFormatted += text;

                    // Increment output size
                    outputSize.Y += currentLineSize.Y;

                    // Is this the longest line
                    if (currentLineSize.X > outputSize.X)
                    {
                        // Set this line as the longest line
                        outputSize.X = currentLineSize.X;
                    }

                    // Reset the current line
                    currentLineSize = Vector2.Zero;
                }
                
                // Is the word too big to fit on this line
                if (currentLineSize.X + wordSize.X + spaceWidth > width)
                {
                    // Is this the longest line
                    if (currentLineSize.X > outputSize.X)
                    {
                        // Set this line as the longest line
                        outputSize.X = currentLineSize.X;
                    }

                    // Add the last line's height to the height tracking
                    outputSize.Y += currentLineSize.Y;

                    // Add a new line
                    messageFormatted += '\n';

                    // Add word colour to formatted message
                    messageFormatted += $"{PRELIMINARY_CHARACTER}text-colour{SPLITTING_CHARACTER}{CreateValidValueString(colour.ToVector4().ToString())}{POSTLIMINARY_CHARACTER}";

                    // Add word position to formatted message
                    position = new Vector2(0, outputSize.Y);
                    messageFormatted += $"{PRELIMINARY_CHARACTER}position{SPLITTING_CHARACTER}{CreateValidValueString(position.ToString())}{POSTLIMINARY_CHARACTER}";

                    // Add the word, plus add space
                    messageFormatted += $"{text} ";

                    // Set the current width to this word plus a space
                    currentLineSize.X = wordSize.X + spaceWidth;

                    // Reset the current line height
                    currentLineSize.Y = 0;
                }

                // Otherwise
                else
                {
                    // Add word colour to formatted message
                    messageFormatted += $"{PRELIMINARY_CHARACTER}text-colour{SPLITTING_CHARACTER}{CreateValidValueString(colour.ToVector4().ToString())}{POSTLIMINARY_CHARACTER}";

                    // Add word position to formatted message
                    position = new Vector2(currentLineSize.X, outputSize.Y);
                    messageFormatted += $"{PRELIMINARY_CHARACTER}position{SPLITTING_CHARACTER}{CreateValidValueString(position.ToString())}{POSTLIMINARY_CHARACTER}";

                    // Add word to string
                    messageFormatted += $"{text} ";

                    // Track current line width
                    currentLineSize.X += wordSize.X + spaceWidth;
                }
            }

            // Return output size
            return outputSize;
        }
        private void ParseWord(
            in string word,
            out string text,
            out Color colour,
            out Vector2 position
            )
        {
            // Assign default values
            text = String.Empty;
            colour = _defaultTextColour;
            position = Vector2.Zero;

            // Split word into parts
            string[] parts = word.Split(POSTLIMINARY_CHARACTER);

            // Loop through each part
            foreach (string part in parts)
            {
                // If we are currently looking at metadata
                if (part.Contains(SPLITTING_CHARACTER))
                {
                    // Define variables for name and value
                    string name, value;

                    // Load in values
                    string[] temp = part.Split(SPLITTING_CHARACTER);

                    // Check for if new line has slipped into name
                    if (temp[0].Contains('\n'))
                    {
                        name = temp[0].Remove(0, 2);
                    }
                    else
                    {
                        name = temp[0].Remove(0, 1);
                    }
                    value = temp[1];
                    // Work with values
                    switch (name.ToLower())
                    {
                        case "text-colour":
                            // Format {X:val_Y:val_Z:val_W:val}
                            colour = new Color(Vector4FromString(value));
                            break;
                        case "position":
                            // Format {X:val_Y:val}
                            position = Vector2FromString(value);
                            break;
                    }
                }
                // If we are looking at the word itself
                else
                {
                    // Set text equal to the word itself
                    text = part;
                }

            }
        }
        private Vector2 Vector2FromString(string inputValue)
        {
            // Parse value
            Vector4 vector4 = VectorFromString(inputValue);

            // Remove useless parts and return
            return new Vector2(vector4.X, vector4.Y);
        }
        private Vector4 Vector4FromString(string inputValue)
        {
            // Parse value
            Vector4 vector4 = VectorFromString(inputValue);

            // Return value
            return vector4;
        }
        private Vector4 VectorFromString(string inputValue)
        {
            // Input is of form {X:val_Y:val_....}

            // Create output
            Vector4 output = Vector4.Zero;

            // Remove fluff and split across spaces
            string[] nameValuePairs = inputValue.Remove(inputValue.Length - 1, 1).Remove(0, 1).Split('_');

            // Loop through each name-value pair
            foreach(string nameValuePair in nameValuePairs)
            {
                // Split name and value across colon
                string[] nameValueSplit = nameValuePair.Split(':');

                // Store value based on name
                switch (nameValueSplit[0])
                {
                    case "X":
                        output.X = float.Parse(nameValueSplit[1]);
                        break;
                    case "Y":
                        output.Y = float.Parse(nameValueSplit[1]);
                        break;
                    case "Z":
                        output.Z = float.Parse(nameValueSplit[1]);
                        break;
                    case "W":
                        output.W = float.Parse(nameValueSplit[1]);
                        break;
                }
            }

            // Return result
            return output;
        }
        public static string CreateValidValueString(string rawValue)
        {
            // Return regular Object->String formatting with spaces replaced with underscores
            return rawValue.Replace(' ', '_');
        }
        public void DrawBox(Vector2 rootPosition, SpriteBatch spriteBatch)
        {
            // Get words in message
            string[] words = _messageFormatted.Split(' ');

            // Loop through every word in the message
            foreach (string word in words)
            {
                // Variables to describe each part of the word
                string text = String.Empty;
                Color colour = _defaultTextColour;
                Vector2 position = Vector2.Zero;

                // Parse word
                ParseWord(
                    in word,
                    out text,
                    out colour,
                    out position
                    );

                // Draw word
                spriteBatch.DrawString(
                    _font,
                    text,
                    rootPosition + position,
                    colour
                    );
            }
        }
        public Rectangle Rect
        {
            get { return _rect; }
            set
            {
                _rect = value;
                _messageSizeCache = MeasureAndFormatMessage(in _message, out _messageFormatted, _rect.Width);
            }
        }
        public Vector2 MessageSize
        {
            get { return _messageSizeCache; }
        }
        public string Message
        {
            get { return _messageFormatted; }
            set
            {
                _message = value;
                _messageSizeCache = MeasureAndFormatMessage(in _message, out _messageFormatted, _rect.Width);
            }
        }
        public SpriteFont Font
        {
            get { return _font; }
            set
            {
                _font = value;
                _messageSizeCache = MeasureAndFormatMessage(in _message, out _messageFormatted, _rect.Width);
            }
        }
        public Color DefaultTextColour
        {
            get { return _defaultTextColour; }
            set { _defaultTextColour = value; }
        }
    }
}

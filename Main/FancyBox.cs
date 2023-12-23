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
    public class FancyBox
    {
        // Variables
        private Rectangle _rect;
        private string _message;
        private SpriteFont _font;

        private string _messageFormatted;
        private Vector2 _messageSizeCache;

        // Constructors
        public FancyBox() // Empty Constructor
        {
            // Default Values
            _rect = new Rectangle();
            _message = string.Empty;
            _font = null;

            _messageFormatted = string.Empty;
            _messageSizeCache = Vector2.Zero;
        }
        public FancyBox(Rectangle rect, SpriteFont font) // Constructor for empty message
        {
            // Pass in value and set rest to default
            _rect = rect;
            _message = string.Empty;
            _font = font;

            _messageFormatted = string.Empty;
            _messageSizeCache = Vector2.Zero;
        }
        public FancyBox( // Filled constructor
            Rectangle rect,
            string message,
            SpriteFont font
            )
        {
            // Pass in values
            _rect = rect;
            _message = message;
            _font = font;

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
                // Get word size
                Vector2 wordSize = _font.MeasureString(word);

                // Is this the tallest word on the line?
                if (wordSize.Y > currentLineSize.Y)
                {
                    currentLineSize.Y = wordSize.Y;
                }
                
                // Is the word just a new line?
                if (word == "\n" || word == "\r" || word == "\r\n")
                {
                    // Add new line to output
                    messageFormatted += word;

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

                    // Add a new line to the output and then add the word, plus add space
                    messageFormatted += $"\n{word} ";

                    // Set the current width to this word plus a space
                    currentLineSize.X = wordSize.X + spaceWidth;

                    // Add the last line's height to the height tracking
                    outputSize.Y += currentLineSize.Y;

                    // Reset the current line height
                    currentLineSize.Y = 0;
                }

                // Otherwise
                else
                {
                    // Add word to string
                    messageFormatted += $"{word} ";

                    // Track current line width
                    currentLineSize.X += wordSize.X + spaceWidth;
                }
            }

            // Return output size
            return outputSize;
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
    }
}

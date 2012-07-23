namespace Nine.Components
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine.Graphics;


    /// <summary>
    /// Event args used by GameConsole.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GameConsoleEventArgs : EventArgs
    {
        public string Command { get; internal set; }
        public string Parameters { get; internal set; }

        public GameConsoleEventArgs() { }
        public GameConsoleEventArgs(string command, string parameters)
        {
            this.Command = command;
            this.Parameters = parameters;
        }
    }

    /// <summary>
    /// Command collection used by game console.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GameConsoleCommandCollection : Dictionary<string, EventHandler<GameConsoleEventArgs>>
    {
        internal GameConsoleCommandCollection(StringComparer stringComparer) : base(stringComparer) { }
    }

    /// <summary>
    /// In game console
    /// </summary>
    public class GameConsole : Nine.IUpdateable, Nine.IDrawable
    {
        private float currentBlinkTime;
        private string lastLine = "";
        private List<string> messages = new List<string>();

        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public int MaxLines { get; set; }
        public int LineSpacing { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public string CursorText { get; set; }
        public float CursorBlinkInterval { get; set; }
        public float FontSize { get; set; }
        public SpriteFont Font { get; set; }
        public Input Input { get; private set; }
        public Microsoft.Xna.Framework.Input.Keys ActivateKey { get; set; }
        public GameConsoleCommandCollection Commands { get; private set; }


        /// <summary>
        /// Gets or sets current console text
        /// </summary>
        public string Text
        {
            get
            {
                StringBuilder result = new StringBuilder();

                foreach (string value in messages)
                    result.AppendLine(value);

                result.Append(lastLine.ToString());
                return result.ToString();
            }

            set
            {
                Clear();
                Write(value);
            }
        }


        public GameConsole(GraphicsDevice graphics, SpriteFont font)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Font = font;
            GraphicsDevice = graphics;
            LineSpacing = 4;
            MaxLines = 20;
            FontSize = 12;
            CursorBlinkInterval = 1;
            CursorText = "_";
            BackgroundColor = new Color(0, 0, 0, 128);
            ForegroundColor = new Color(255, 255, 255, 255);
            ActivateKey = Microsoft.Xna.Framework.Input.Keys.OemTilde;
            Enabled = Visible = false;

            Input = new Input();
            Input.KeyDown += new EventHandler<KeyboardEventArgs>(Input_KeyDown);

            Commands = new GameConsoleCommandCollection(StringComparer.CurrentCultureIgnoreCase);

            // Add default commands
            Commands.Add("Help", delegate(object sender, GameConsoleEventArgs args)
            {
                WriteLine("Type \"List\" for a list of available commands.");
            });
            Commands.Add("List", delegate(object sender, GameConsoleEventArgs args)
            {
                WriteLine("-------- Begin Command List --------");
                foreach (string key in Commands.Keys)
                    WriteLine(key);
                WriteLine("--------  End Command List  --------");
            });
        }

        void Input_KeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.Key == ActivateKey)
            {
                // Toggle enable state
                Enabled = Visible = !Enabled;
            }
            else if (e.Key == Keys.Enter && !string.IsNullOrEmpty(lastLine))
            {
                string command = lastLine;

                NewLine();

                TriggerCommand(command);
            }
        }

        public void Write(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                // Replace "\t" with space
                if (value[i] == '\t')
                {
                    const int TabSize = 4;
                    int n = (lastLine.Length + TabSize) % TabSize;
                    for (int k = 0; k < n; k++)
                        lastLine += ' ';
                }
                // Replace "\n" with new line
                else if (value[i] == '\n')
                {
                    NewLine();
                }
                // Append the charactor
                else lastLine += value[i];
            }
        }

        public void WriteLine(string value)
        {
            Write(value);
            NewLine();
        }

        public void Clear()
        {
            messages.Clear();
            lastLine.Remove(0, lastLine.Length);
        }

        public void NewLine()
        {
            messages.Add(lastLine.ToString());
            lastLine = "";
        }

        public void Draw(TimeSpan elapsedTime)
        {
            if (Font == null)
                return;

            SpriteBatch spriteBatch = GraphicsResources<SpriteBatch>.GetInstance(GraphicsDevice);
            spriteBatch.Begin();

            const int Border = 8;

            // Compute number of lines to be rendered
            int height = (int)(FontSize + LineSpacing);
            int lineCount = messages.Count + 1;
            if (lineCount > MaxLines)
                lineCount = MaxLines;

            // Use Graphics.Clear to draw a background quad
            int y = GraphicsDevice.PresentationParameters.BackBufferHeight - lineCount * height - Border;
            
            // Draw text
            float fontScale = 1.0f * FontSize / (Font.MeasureString("A").Y - 10);
            int i = messages.Count - (lineCount - 1);
            if (i < 0)
                i = 0;
            
            for (; i < messages.Count; i++)
            {
                spriteBatch.DrawString(
                    Font, messages[i], new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);

                y += height;
            }

            // Update text cursor
            currentBlinkTime += (float)elapsedTime.TotalMilliseconds;

            if (currentBlinkTime > CursorBlinkInterval)
                currentBlinkTime = -CursorBlinkInterval;

            // Draw last line and text cursor
            // FIXME: Timing not working when game is paused
            if (currentBlinkTime < 0)
            {
                spriteBatch.DrawString(
                    Font, lastLine, new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.DrawString(
                    Font, lastLine + CursorText, new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        public void Update(TimeSpan elapsedTime)
        {
            Input.CatchKeyboardInput(ref lastLine, 80);
        }

        private void TriggerCommand(string line)
        {
            // Find a space
            line.TrimStart(new char[] { ' ', '\t', '\r', '\n' });
            int start = line.IndexOf(' ');

            if (start < 0)
                start = line.Length;

            string cmd = line.Substring(0, start);

            if (Commands.ContainsKey(cmd))
            {
                string args = (start == line.Length ? "" : line.Substring(start + 1));
                EventHandler<GameConsoleEventArgs> handler = Commands[cmd];

                try
                {
                    if (handler != null)
                        handler(this, new GameConsoleEventArgs(cmd, args));
                }
                catch (Exception e)
                {
                    WriteLine(e.Message);
                }
            }
            else
            {
                WriteLine("Unknown Command: " + cmd + ". " + "Type \"List\" for a list of available commands.");
            }
        }
    }
}
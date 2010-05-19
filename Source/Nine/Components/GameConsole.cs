#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nine.Components
{
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
    /// In game console
    /// </summary>
    public class GameConsole : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private string fontFile;
        private float currentBlinkTime;
        private string lastLine = "";
        private List<string> messages = new List<string>();


        public int MaxLines { get; set; }
        public int LineSpacing { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public string CursorText { get; set; }
        public float CursorBlinkInterval { get; set; }
        public float FontSize { get; set; }
        public SpriteFont Font { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public Input Input { get; private set; }
        public Microsoft.Xna.Framework.Input.Keys ActivateKey { get; set; }
        public IDictionary<string, EventHandler<GameConsoleEventArgs>> Commands { get; private set; }


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


        public GameConsole(Game game) : base(game)
        {
            if (game == null)
                throw new ArgumentNullException();

            Initialize();
        }

        public GameConsole(Game game, string fontFile) : base(game)
        {
            if (game == null)
                throw new ArgumentNullException();

            this.fontFile = fontFile;

            Initialize();
        }

        private new void Initialize()
        {
            LineSpacing = 4;
            MaxLines = 20;
            FontSize = 12;
            CursorBlinkInterval = 1;
            CursorText = "_";
            BackgroundColor = new Color(0, 0, 0, 128);
            ForegroundColor = new Color(255, 255, 255, 255);
            ActivateKey = Microsoft.Xna.Framework.Input.Keys.OemTilde;
            Enabled = Visible = false;


            if (Game != null)
            {
                Input = new Input();
                Input.KeyDown += new EventHandler<KeyboardEventArgs>(Input_KeyDown);

                Game.Components.Add(Input);
            }


            Commands = new Dictionary
                <string, EventHandler<GameConsoleEventArgs>>(StringComparer.CurrentCultureIgnoreCase);

            // Add default commands
            Commands.Add("Help", delegate(object sender, GameConsoleEventArgs args)
            {
                WriteLine("Type \"List\" for a list of available commands.");
            });
            Commands.Add("Quit", delegate(object sender, GameConsoleEventArgs args)
            {
                Game.Exit();
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

        protected override void LoadContent()
        {
            if (SpriteBatch == null)
                SpriteBatch = new SpriteBatch(GraphicsDevice);

            if (!string.IsNullOrEmpty(fontFile))
                Font = Game.Content.Load<SpriteFont>(fontFile);

            base.LoadContent();
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

        public override void Draw(GameTime gameTime)
        {
            if (SpriteBatch == null || Font == null)
                return;
            

            SpriteBatch.Begin();

            const int Border = 8;

            // Compute number of lines to be rendered
            int height = (int)(FontSize + LineSpacing);
            int lineCount = messages.Count + 1;
            int maxCount = Math.Min(MaxLines, (GraphicsDevice.PresentationParameters.BackBufferHeight - Border * 2) / height);
            if (lineCount > MaxLines)
                lineCount = MaxLines;

            // Use Graphics.Clear to draw a background quad
            int y = GraphicsDevice.PresentationParameters.BackBufferHeight - lineCount * height - Border;

            /*
            GraphicsDevice.Clear(
                ClearOptions.Target, BackgroundColor, 0, 0,
                new Rectangle[] { new Rectangle(
                    Border, y, GraphicsDevice.PresentationParameters.BackBufferWidth - Border * 2, lineCount * height) });
            */


            // Draw text
            float fontScale = 1.0f * FontSize / (Font.MeasureString("A").Y - 10);
            int i = messages.Count - (lineCount - 1);
            if (i < 0)
                i = 0;
            
            for (; i < messages.Count; i++)
            {
                SpriteBatch.DrawString(
                    Font, messages[i], new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);

                y += height;
            }

            // Update text cursor
            currentBlinkTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (currentBlinkTime > CursorBlinkInterval)
                currentBlinkTime = -CursorBlinkInterval;

            // Draw last line and text cursor
            // FIXME: Timing not working when game is paused
            if (currentBlinkTime < 0)
            {
                SpriteBatch.DrawString(
                    Font, lastLine, new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }
            else
            {
                SpriteBatch.DrawString(
                    Font, lastLine + CursorText, new Vector2(Border + 2, y), ForegroundColor, 0,
                    Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            Input.CatchKeyboardInput(ref lastLine, 80);
            
            base.Update(gameTime);
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
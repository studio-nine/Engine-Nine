namespace Microsoft.Xna.Framework
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Graphics;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// The system window associated with a Game.
    /// </summary>
    public class GameWindow
    {
        Game game;

        /// <summary>
        /// Gets the handle to the system window.
        /// This value will always be IntPtr.Zero on Silverlight.
        /// </summary>
        public IntPtr Handle { get { return IntPtr.Zero; } }

        public string Title { get; set; }

        public event EventHandler<EventArgs> ClientSizeChanged;

        internal GameWindow(Game game)
        {
            this.game = game;
            game.SizeChanged += new SizeChangedEventHandler(game_SizeChanged);
        }

        void game_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            game.Surface.Width = e.NewSize.Width;
            game.Surface.Height = e.NewSize.Height;
            if (ClientSizeChanged != null)
                ClientSizeChanged(this, EventArgs.Empty);
        }
    }
}
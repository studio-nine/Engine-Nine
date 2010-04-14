using System;

namespace ScreenEffects
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ScreenEffectsGame game = new ScreenEffectsGame())
            {
                game.Run();
            }
        }
    }
}


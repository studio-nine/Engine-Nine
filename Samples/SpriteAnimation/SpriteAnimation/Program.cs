using System;

namespace SpriteAnimationGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SpriteAnimationGame game = new SpriteAnimationGame())
            {
                game.Run();
            }
        }
    }
#endif
}


using System;

namespace AvatarAnimationGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AvatarAnimationGame game = new AvatarAnimationGame())
            {
                game.Run();
            }
        }
    }
#endif
}


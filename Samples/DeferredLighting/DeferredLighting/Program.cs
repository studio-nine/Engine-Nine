using System;

namespace DeferredLighting
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DeferredLightingGame game = new DeferredLightingGame())
            {
                game.Run();
            }
        }
    }
#endif
}


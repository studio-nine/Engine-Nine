using System;

namespace ShadowMapping
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ShadowMappingGame game = new ShadowMappingGame())
            {
                game.Run();
            }
        }
    }
}


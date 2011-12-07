using System;

namespace Tutorial
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial game = new Tutorial())
            {
                game.Run();
            }
        }
    }
#endif
}


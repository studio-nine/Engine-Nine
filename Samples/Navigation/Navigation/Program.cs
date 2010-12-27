using System;

namespace NavigationSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (NavigationGame game = new NavigationGame())
            {
                game.Run();
            }
        }
    }
#endif
}


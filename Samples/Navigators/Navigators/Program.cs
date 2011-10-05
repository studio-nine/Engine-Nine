using System;

namespace Navigators
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (NavigatorsSampleGame game = new NavigatorsSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}


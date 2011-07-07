using System;

namespace Game
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SampleGame game = new SampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}


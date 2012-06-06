using System;

namespace CubeStressTest
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CubeStressTestGame game = new CubeStressTestGame())
            {
                game.Run();
            }
        }
    }
#endif
}


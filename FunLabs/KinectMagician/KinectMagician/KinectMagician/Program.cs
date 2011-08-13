using System;

namespace KinectMagician
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KinectMagicianGame game = new KinectMagicianGame())
            {
                game.Run();
            }
        }
    }
#endif
}


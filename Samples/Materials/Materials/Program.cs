using System;

namespace MaterialsSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MaterialsGame game = new MaterialsGame())
            {
                game.Run();
            }
        }
    }
#endif
}


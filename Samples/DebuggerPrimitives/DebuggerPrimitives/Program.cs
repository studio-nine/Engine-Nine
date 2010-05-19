using System;

namespace DebuggerPrimitives
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DebuggerPrimitiveGame game = new DebuggerPrimitiveGame())
            {
                game.Run();
            }
        }
    }
#endif
}


using System;

namespace FlockingBehaviors
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (FlockingBehaviorGame game = new FlockingBehaviorGame())
            {
                game.Run();
            }
        }
    }
#endif
}


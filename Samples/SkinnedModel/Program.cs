using System;

namespace SkinnedModel
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SkinnedModelGame game = new SkinnedModelGame())
            {
                game.Run();
            }
        }
    }
}


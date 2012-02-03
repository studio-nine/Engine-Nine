
namespace PathFinding
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PathFindingGame game = new PathFindingGame())
            {
                game.Run();
            }
        }
    }
#endif
}


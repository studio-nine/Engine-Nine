
namespace GeometryGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GeometryTestGame game = new GeometryTestGame())
            {
                game.Run();
            }
        }
    }
#endif
}


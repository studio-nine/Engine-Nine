
namespace MultiPassLighting
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MultiPassLightingGame game = new MultiPassLightingGame())
            {
                game.Run();
            }
        }
    }
#endif
}


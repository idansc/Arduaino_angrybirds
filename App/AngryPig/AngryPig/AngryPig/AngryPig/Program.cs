using System;

namespace AngryPig
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AngryPigGame game = new AngryPigGame())
            {
                game.Run();
            }
        }
    }
#endif
}


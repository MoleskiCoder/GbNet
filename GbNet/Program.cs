namespace GbNet
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var configuration = new Configuration();

#if DEBUG
            configuration.DebugMode = true;
#endif

            using (var game = new Cabinet(configuration))
            {
                game.Plug("games/Tetris (World).gb");
                game.Run();
            }
        }
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new Main.Wrapper();

        game.Window.Title = "Console Window Test";

        game.Run();
    }
}
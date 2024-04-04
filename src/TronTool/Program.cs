namespace tron;
public class Program
{
    public static async Task Main(string[] args)
    {
        var commandHandler = new CommandHandler();
        if (commandHandler != null)
        {
            await commandHandler.HandleCommands(args);
        }
    }
}

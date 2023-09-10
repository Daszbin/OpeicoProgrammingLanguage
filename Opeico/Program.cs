namespace Opeico;

class Program
{
    [Obsolete]
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Repl.RunRepl();
        }
        else
        {
            await TerminalOpeico.Perform(args);
        }
    }
}
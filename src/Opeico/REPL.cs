using OpeicoCompiler;
using Spectre.Console;
using System.Diagnostics;
using System.Text;

namespace Opeico;

public class Repl
{
    [Obsolete]
    public static async Task RunRepl()
    {
        Console.Title = "Opeico Programming Language";
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.OutputEncoding = Encoding.UTF8;

        bool isFuncOrClass = false;
        string prompt = "[bold green]Opeico[/]([red]" + CurrentVersion.Get() + "[/])> ";

        AnsiConsole.Write(
            new FigletText("Opeico").Color(Color.Red));

        AnsiConsole.MarkupLine(
            "[bold yellow]Hello[/] and [bold red]Welcome to 🍲 Opeico Programming Language![/] For info visit [link blue]https://Opeicolang.com[/]. Type [bold palegreen1]!menu[/] to see options");

        Compiler compiler = new();

        string dataStart = "func main() = {";
        string dataEnd = "}";

        Stack<string> funcData = new();
        AnsiConsole.Markup(prompt);

        Stack<string> operations = new();

        bool inloop = true;

        while (inloop)
        {
            string? readLine = Console.ReadLine();
            if (string.IsNullOrEmpty(readLine))
            {
                AnsiConsole.Markup(prompt);
                continue;
            }


            if (readLine.Equals("!menu", StringComparison.OrdinalIgnoreCase))
            {
                Table table = new();

                // Add some columns
                _ = table.AddColumn("Command");
                _ = table.AddColumn(new TableColumn("Description"));

                // Add some rows
                _ = table.AddRow("!list", "[red]Lists the current code[/]");
                _ = table.AddRow("!clear", "[green]Clears The REPL[/]");
                _ = table.AddRow("!undo", "[blue]Undoes last entered command[/]");
                _ = table.AddRow("!update", "[yellow]Update Opeico to latest version[/]");
                _ = table.AddRow("!restart", "[fuchsia]Restart Application[/]");
                _ = table.AddRow("!get", "[aqua]Get List of Libraries for Opeico[/]");
                _ = table.AddRow("!exit", "[darkred_1]Exits REPL[/]");
                AnsiConsole.Write(table);
                AnsiConsole.Markup(prompt);
                continue;
            }

            if (readLine.Equals("!clear", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                compiler = new Compiler();
                isFuncOrClass = false;
                funcData.Clear();
                dataStart = "func main() = {";
                dataEnd = "}";
                AnsiConsole.Markup(prompt);
                continue;
            }

            if (readLine.Equals("!list", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string? data in funcData.Reverse())
                {
                    Console.WriteLine(data);
                }

                AnsiConsole.Markup(prompt);
                continue;
            }

            if (readLine.Equals("!get", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string? data in funcData.Reverse())
                {
                    Console.WriteLine(data);
                }

                AnsiConsole.Markup(prompt);
                continue;
            }

            if (readLine.Equals("!undo", StringComparison.OrdinalIgnoreCase))
            {
                string templine = funcData.Pop();
                AnsiConsole.MarkupLine("[bold red]Removed: [/]" + templine);
                AnsiConsole.Markup(prompt);
                continue;
            }

            // Exit Opeico
            if (readLine.Equals("!exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (readLine.StartsWith("func") || readLine.StartsWith("class"))
            {
                isFuncOrClass = true;
                funcData.Push(readLine);
                Trace.WriteLine("Starting Func: " + readLine);
                continue;
            }
            else if (isFuncOrClass)
            {
                if (readLine.StartsWith("}", StringComparison.OrdinalIgnoreCase))
                {
                    funcData.Push(readLine);
                    Trace.WriteLine("Ending Func: " + readLine);

                    StringBuilder userDataToExecute = new();
                    foreach (string item in funcData.Reverse())
                    {
                        _ = userDataToExecute.Append(item);
                    }

                    funcData = new();

                    dataEnd += userDataToExecute.ToString();
                    isFuncOrClass = false;
                    AnsiConsole.Markup(prompt);
                }
                else
                {
                    Trace.WriteLine("Reading Func: " + readLine);
                    funcData.Push(readLine);
                }
            }
            else
            {
                if (readLine.StartsWith("var"))
                {
                    dataStart += readLine;
                    readLine = "";
                }

                funcData.Push(readLine);

                string codeToExecute = dataStart + readLine + dataEnd;

                Trace.WriteLine(codeToExecute);
                string output = "Something went wrong! Please restart the application";

                await AnsiConsole.Status().Spinner(Spinner.Known.Star).StartAsync("Computing...", ctx =>
                {
                    try
                    {
                        //Console.WriteLine(codeToExecute);
                        compiler = new Compiler(); // Clear OLD params -- In future, we don't need to do this
                        output = compiler.Go(codeToExecute, isFile: false, debug: 1);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                    }

                    return Task.CompletedTask;
                });

                Console.WriteLine(output);
                AnsiConsole.Markup(prompt);
            }
        }
    }

}
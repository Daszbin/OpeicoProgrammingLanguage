using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Expressions;
using OpeicoCompiler.Interpreter;
using OpeicoCompiler.Lexer;
using OpeicoCompiler.Parse;
using OpeicoCompiler.Scan;
using OpeicoCompiler.Statements;
using OpeicoCompiler.SystemCalls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace OpeicoCompiler
{
    /*
     * Maing entry point into the compiler responsible for setting up DI container 
     * Calling parser and compiler (currently interpreter.
     */
    public class Compiler
    {
        private ServiceProvider? serviceProvider = null;

        public Compiler()
        {
            Initialize();
        }

        internal void Initialize()
        {
            HostBuilder hostBuilder = new();
            _ = hostBuilder.ConfigureServices(services =>
            {
                _ = services.AddSingleton<ICompilerError, CompilerError>();
                _ = services.AddSingleton<IOpeicoCallable, OpeicoSystemCalls>();
                _ = services.AddSingleton<IFileOpen, FileOpen>();
                _ = services.AddSingleton<ICSharp, CSharp>();
                _ = services.AddSingleton<ISystemClock, SystemClock>();
                _ = services.AddSingleton<IGetAvailableMemory, GetAvailableMemory>();
                serviceProvider = services.BuildServiceProvider();
            });
            _ = hostBuilder.Build();
        }

        // Run the Compiler (Step: 3)
        public string Go(string data, bool isFile = true, int debug = 0)
        {
            try
            {
                ServiceProvider? provider = serviceProvider;
                if (provider != null)
                {
                    provider.GetRequiredService<ICompilerError>().SourceFileName(data);

                    Parser parser = new(new Scanner(data, provider, isFile), provider);
                    List<Stmt> statements = parser.Parse();

                    return Compile(statements);
                }
            }
            catch (Exception ex)
            {
                return $"Error compiling {ex.Message}";
            }

            throw new Exception("unhandled errors");
        }

        private string Compile(List<Stmt> statements)
        {
            if (serviceProvider != null)
            {
                Interpreter.OpeicoInterpreter interpreter = new(serviceProvider);
                Resolver? resolver = new(interpreter);
                resolver.Resolve(statements);

                SetupMainMethodRuntimeHook(statements, resolver);

                TextWriter currentOut = Console.Out;

                try
                {
                    using StringWriter stringWriter = new();
                    Console.SetOut(stringWriter);
                    interpreter.Interpret(statements);

                    string consoleOutput = stringWriter.ToString();
                    Console.SetOut(currentOut);
                    return consoleOutput;
                }
                catch (Exception e)
                {
                    Console.SetOut(currentOut);
                    return e.ToString();
                }
            }

            throw new JRuntimeException("Service provider is not created");
        }

        private static void SetupMainMethodRuntimeHook(List<Stmt> statements, Resolver resolver)
        {
            List<Stmt> allFunctions = statements.Where(e => (e is Stmt.Function) == true).ToList();

            foreach (Stmt? m in allFunctions)
            {
                if (((Stmt.Function)m).StmtLexemeName.Equals("main"))
                {
                    break;
                }
                else
                {
                    continue;
                }

                throw new Exception("No main function is defined");
            }

            Lexeme lexeme = new(LexemeType.Types.IDENTIFIER, 0, 0);
            lexeme.AddToken("main");
            Expr.Variable functionName = new(lexeme);
            Expr.Call call = new(functionName, false, new List<Expr>());
            Stmt.Expression expression = new(call);
            resolver.Resolve(new List<Stmt>() { expression });
        }

        public bool HasErrors()
        {
            ServiceProvider? provider = serviceProvider;
            return provider != null && provider.GetRequiredService<ICompilerError>().HasErrors();
        }

        public List<string> ListErrors()
        {
            ServiceProvider? provider = serviceProvider;
            return provider != null
                ? provider.GetRequiredService<ICompilerError>().ListErrors()
                : throw new JRuntimeException("unable to initialize provider for errors");
        }

    }
}


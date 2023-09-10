using OpeicoCompiler.Expressions;
using OpeicoCompiler.Interpreter;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace OpeicoCompiler.SystemCalls
{
    public enum CallableServices
    {
        GetAvailableMemory,
        FileOpen,
        CSharp
    }

    /// <summary>
    /// Need a way to implement structured exception handling
    /// All this does is catch an exception and returns an object.
    /// </summary>
    internal class SystemCallException : Exception
    {
        internal Exception internalException;
        internal SystemCallException(Exception ex)
        {
            internalException = ex;
        }
    }

    internal class OpeicoSystemCalls : IOpeicoCallable
    {
        public static readonly Dictionary<string, Type> kv = new()
        {
            {"FileOpen", typeof(IFileOpen)},
            {"CSharp", typeof(ICSharp)},
        };

        public int Arity()
        {
            throw new NotImplementedException();
        }

        public object? Call(string methodName, Interpreter.OpeicoInterpreter interpreter, List<object?> arguments)
        {
            if (OpeicoSystemCalls.kv.TryGetValue(methodName, out Type? theType))
            {
                IOpeicoCallable Opeicocall = (IOpeicoCallable)interpreter.ServiceProvider.GetService(theType)!;
                return Opeicocall?.Call(methodName, interpreter, arguments);
            }

            throw new Exception("");
        }
    }

    internal class FileOpen : IFileOpen, IOpeicoCallable
    {
        public int Arity()
        {
            return 1;
        }

        public object? Call(string methodName, Interpreter.OpeicoInterpreter interpreter, List<object?> arguments)
        {
            try
            {
                foreach (object? argument in arguments)
                {
                    if (argument is Expr.LexemeTypeLiteral literal)
                    {
                        byte[] bytes = File.ReadAllBytes(literal.literal?.ToString() ?? string.Empty);
                        Console.Out.WriteLine(literal);
                        return bytes;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SystemCallException(ex);
            }

            return Array.Empty<byte>();
        }
    }

    internal class CSharp : ICSharp, IOpeicoCallable
    {
        public int Arity()
        {
            return 1;
        }

        public object? Call(string methodName, Interpreter.OpeicoInterpreter interpreter, List<object?> arguments)
        {
            try
            {
                foreach (object? argument in arguments)
                {
                    if (argument is Expr.LexemeTypeLiteral literal)
                    {
                        string csharp = literal.literal?.ToString() ?? string.Empty;

                        string result = "";
                        try
                        {
                            Task<object> task = CSharpScript.EvaluateAsync(csharp, ScriptOptions.Default.WithImports(new List<string> { "System.IO", "System.Math" }));
                            System.Runtime.CompilerServices.TaskAwaiter<object> taskCompleted = task.GetAwaiter();
                            if (taskCompleted.GetResult() != null)
                            {
                                result = taskCompleted.GetResult().ToString() ?? "";
                            }
                        }
                        catch (Exception)
                        {
                            result = string.Empty;
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SystemCallException(ex);
            }

            return "";
        }
    }
}

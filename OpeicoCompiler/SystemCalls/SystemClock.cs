using OpeicoCompiler.Expressions;
using OpeicoCompiler.Interpreter;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpeicoCompiler.SystemCalls
{
    internal class SystemClock : ISystemClock, IOpeicoCallable
    {
        public int Arity()
        {
            return 0;
        }
        public object? Call(string methodName, Interpreter.OpeicoInterpreter interpreter, List<object?> arguments)
        {
            Expr.LexemeTypeLiteral? lexemeTypeLiteral = new()
            {
                literal = DateTime.Now.Millisecond / 1000.0
            };
            return lexemeTypeLiteral;
        }
        public override string ToString()
        {
            return "[Native Fn]";
        }
    }


    /*
     * Gets the available memory of the process. Currently including the Opeico Runtime.
     */
    internal class GetAvailableMemory : IGetAvailableMemory, IOpeicoCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object? Call(string methodName, Interpreter.OpeicoInterpreter interpreter, List<object?> arguments)
        {
            decimal memory;

            Process proc = Process.GetCurrentProcess();
            memory = Math.Round((decimal)proc.PrivateMemorySize64 / (1024 * 1024), 2);

            if (memory == 0 && RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                memory = Math.Round((decimal)proc.VirtualMemorySize64 / (1024 * 1024), 2);
            }

            proc.Dispose();

            Expr.LexemeTypeLiteral? lexemeTypeLiteral = new()
            {
                literal = memory
            };
            return lexemeTypeLiteral;
        }
    }
}

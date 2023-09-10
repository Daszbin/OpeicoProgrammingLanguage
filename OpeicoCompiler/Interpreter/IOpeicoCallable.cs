namespace OpeicoCompiler.Interpreter
{
    internal interface IOpeicoCallable
    {
        int Arity();
        object? Call(string methodName, OpeicoInterpreter interpreter, List<object?> arguments);
    }
}

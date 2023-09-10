namespace OpeicoCompiler.Interpreter
{
    internal class OpeicoClass : IOpeicoCallable
    {
        private readonly string name;
        private readonly OpeicoClass superClass;
        private readonly Dictionary<string, OpeicoFunction> methods;

        internal OpeicoClass(string name, OpeicoClass superClass, Dictionary<string, OpeicoFunction> methods)
        {
            this.superClass = superClass;
            this.name = name;
            this.methods = methods;
        }

        public int Arity()
        {
            OpeicoFunction? initializer = FindMethod("main");
            return initializer == null ? 0 : initializer.Arity();
        }

        public object? Call(string methodName, OpeicoInterpreter interpreter, List<object?> arguments)
        {
            // FIND METHOD is broken
            // Declaration is never set correctly.
            OpeicoInstance? instance = new(this);
            OpeicoFunction? initializer = FindMethod("main");
            _ = (initializer?.Bind(instance).Call(methodName, interpreter, arguments));

            return instance;
        }

        internal OpeicoFunction? FindMethod(string name)
        {
            return methods.ContainsKey(name) ? methods[name] : (superClass?.FindMethod(name));
        }
    }
}

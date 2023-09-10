using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Statements;

namespace OpeicoCompiler.Interpreter
{
    internal class OpeicoFunction : IOpeicoCallable
    {
        private readonly bool isInitializer;

        internal OpeicoFunction(Stmt.Function declaration, OpeicoEnvironment closure, bool isInitializer)
        {
            this.isInitializer = isInitializer;
            Declaration = declaration;
            Closure = closure;
        }

        internal OpeicoFunction Bind(OpeicoInstance? instance)
        {
            if (Closure != null)
            {
                OpeicoEnvironment env = new(Closure);
                env.Define("this", instance);

                if (Declaration != null)
                {
                    return new OpeicoFunction(Declaration, env, isInitializer);
                }
            }

            throw new JRuntimeException("Unable to bind");
        }

        internal Stmt.Function? Declaration { get; }

        internal OpeicoEnvironment? Closure { get; }


        public int Arity()
        {
            return Declaration == null ? 0 : Declaration.Params.Count;
        }

        public object? Call(string methodName, OpeicoInterpreter interpreter, List<object?> arguments)
        {
            if (Closure != null)
            {
                OpeicoEnvironment environment = new(Closure);

                if (Declaration != null)
                {
                    for (int i = 0; i < Declaration.Params.Count; i++)
                    {
                        Lexer.Lexeme? parameterNameExpressionLexeme = Declaration.Params[i].parameterName.ExpressionLexeme;
                        if (parameterNameExpressionLexeme != null)
                        {
                            string? name = parameterNameExpressionLexeme.ToString();
                            if (string.IsNullOrEmpty(name))
                            {
                                throw new ArgumentException("Unable to call function");
                            }

                            object? value = arguments[i];
                            environment.Define(name, value);
                        }
                    }
                }

                try
                {
                    if (Declaration?.body != null)
                    {
                        List<Stmt> body = Declaration?.body!;
                        interpreter.ExecuteBlock(body, environment);
                    }
                }
                catch (Return ex)
                {
                    return isInitializer ? Closure.GetAt(0, "this") : ex.value;
                }
            }

            return isInitializer ? (Closure?.GetAt(0, "this")) : null;
        }
    }
}

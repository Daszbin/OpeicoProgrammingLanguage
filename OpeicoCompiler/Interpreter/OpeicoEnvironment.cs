using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Lexer;

namespace OpeicoCompiler.Interpreter
{
    internal class OpeicoEnvironment
    {
        private OpeicoEnvironment? enclosing;
        private readonly Dictionary<string, object?> values = new();

        internal OpeicoEnvironment()
        {
            enclosing = null;
        }

        internal OpeicoEnvironment Enclosing
        {
            set => enclosing = value;
        }

        internal OpeicoEnvironment(OpeicoEnvironment enclosing)
        {
            this.enclosing = enclosing;
        }

        internal object? Get(Lexeme name)
        {
            if (values.ContainsKey(name.ToString()))
            {
                return values[name.ToString()];
            }

            return enclosing != null
                ? enclosing.Get(name)
                : throw new ArgumentException("Opeico Environment.Get() has an undefined variable ('" + name.ToString() + "') undefined variable");
        }

        internal void Assign(Lexeme? name, object? value)
        {
            string nameAsString = name?.ToString() ?? throw new JRuntimeException("unable to get variable name");

            if (values.ContainsKey(nameAsString))
            {
                values[name.ToString()] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new ArgumentException("Opeico Environment.Assign() has an undefined variable ('" + name + "') undefined variable");
        }

        internal void Define(string name, object? value)
        {
            values.Add(name, value);
        }

        internal OpeicoEnvironment? Ancestor(int distance)
        {
            OpeicoEnvironment? environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment?.enclosing;
            }

            return environment;
        }

        internal object? GetAt(int distance, string name)
        {
            return Ancestor(distance)?.values[name];
        }

        internal void AssignAt(int distance, Lexeme name, object? value)
        {
            Ancestor(distance)!.values[name.ToString()] = value;
        }

        public override string ToString()
        {
            string result = values.ToString()!;
            if (enclosing != null)
            {
                result += " -> " + enclosing;
            }

            return result;
        }
    }
}

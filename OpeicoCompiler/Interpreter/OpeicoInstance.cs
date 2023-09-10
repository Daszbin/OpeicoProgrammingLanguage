using OpeicoCompiler.Lexer;

namespace OpeicoCompiler.Interpreter
{
    internal class OpeicoInstance
    {
        private readonly OpeicoClass Class;
        private readonly Dictionary<string, object> fields = new();

        internal OpeicoInstance(OpeicoClass Class)
        {
            this.Class = Class;
        }

        internal object Get(Lexeme name)
        {
            if (fields.ContainsKey(name.ToString()))
            {
                return fields[name.ToString()];
            }

            OpeicoFunction? method = Class.FindMethod(name.ToString());

            if (method != null)
            {
                return method;//.Bind(this);
            }

            throw new ArgumentException("Undefined property" + name);
        }

        internal void Set(Lexeme name, object value)
        {
            fields.Add(name.ToString(), value);
        }
    }
}

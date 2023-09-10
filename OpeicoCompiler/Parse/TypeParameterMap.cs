using OpeicoCompiler.Expressions;
using OpeicoCompiler.Lexer;

namespace OpeicoCompiler.Parse
{
    internal class TypeParameterMap
    {
        internal TypeParameterMap(Lexeme? parameterType, Expr varName)
        {
            this.parameterType = parameterType;
            parameterName = varName;
        }

        internal Lexeme? parameterType;
        internal Expr parameterName;
    }
}

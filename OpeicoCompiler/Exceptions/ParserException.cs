using OpeicoCompiler.Lexer;

namespace OpeicoCompiler.Exceptions
{
    internal class ParserException : Exception
    {
        internal ParserException(string? message, LexemeType lexemeType)
            : base(message)
        {
        }
    }
}

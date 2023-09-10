using System.Text;

namespace OpeicoCompiler.Lexer
{
    internal class LexemeType
    {
        internal enum Types
        {
            UNDEFINED,
            CHAR,
            INTERNALFUNCTION,
            STRING,
            IDENTIFIER,
            NUMBER,
            WHITESPACE,
            SYMBOL,
            EOF,
            LEFT_PAREN,
            RIGHT_PAREN,
            LEFT_BRACE,
            RIGHT_BRACE,
            COMMA,
            DOT,
            MINUS,
            PLUS,
            SEMICOLON,
            SLASH,
            STAR,
            BANG,
            BANG_EQUAL,
            EQUAL,
            EQUAL_EQUAL,
            GREATER,
            GREATER_EQUAL,
            LESS,
            LESS_EQUAL,
            AND,
            CLASS,
            ELSS,
            FLS,
            FUNC,
            FOR,
            IFF,
            NL,
            OR,
            PRINTLINE,
            RET,
            SUPER,
            THIS,
            TRU,
            RUN,
            WHL,
            INT,
            LONG,
            SHORT,
            DOUBLE,
            FLOAT,
            BOOL,
            PRINT,
            BREAK,
            ARRAY,
            PLUSPLUS,
            NEW,
            DELETE,
        }
    }
    internal class Lexeme
    {
        private readonly StringBuilder tokenBuilder = new();
        private bool isKeyWord = false;

        internal LexemeType.Types LexemeType { get; set; }

        internal Lexeme()
        {
        }

        internal Lexeme(LexemeType.Types ltype, int lineNumber, int columnNumber) : this()
        {
            LexemeType = ltype;
            LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
        }

        internal void AddToken(char token)
        {
            _ = tokenBuilder.Append(token);
        }

        internal void AddToken(string token)
        {
            _ = tokenBuilder.Append(token);
        }

        internal void AddToken(Lexeme? token)
        {
            _ = tokenBuilder.Append(token?.ToString());
        }

        internal bool IsKeyWord
        {
            set => isKeyWord = true;
            get => isKeyWord;
        }

        internal long TypeOfKeyWord { get; set; }

        internal int LineNumber { get; }

        internal int ColumnNumber { get; }

        public override string ToString()
        {
            return tokenBuilder.ToString();
        }

        internal string Literal()
        {
            return ToString();
        }
    }
}

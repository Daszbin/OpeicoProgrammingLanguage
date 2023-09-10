using Microsoft.Extensions.DependencyInjection;
using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Lexer;
using System.Text;
using static System.Char;

namespace OpeicoCompiler.Scan
{
    internal class Scanner
    {
        private int start = 0;
        private int current = 0;
        private int line = 1;
        private int column = 0;
        private readonly byte[] fileData;
        private readonly List<Lexeme?> lexemes = new();
        private readonly ICompilerError compilerError;

        private static readonly Dictionary<string, LexemeType.Types> keywordsDictionary = new()
        {
            { "and",    LexemeType.Types.AND },
            { "class",  LexemeType.Types.CLASS },
            { "elss",   LexemeType.Types.ELSS },
            { "func",   LexemeType.Types.FUNC },
            { "for",    LexemeType.Types.FOR },
            { "iff",     LexemeType.Types.IFF },
            { "nl",   LexemeType.Types.NL },
            { "or",     LexemeType.Types.OR },
            { "ret", LexemeType.Types.RET },
            { "super",  LexemeType.Types.SUPER },
            { "this",   LexemeType.Types.THIS },
            { "fls",  LexemeType.Types.FLS },
            { "tru",   LexemeType.Types.TRU },
            { "run",    LexemeType.Types.RUN },
            { "whl",  LexemeType.Types.WHL },
            { "int",    LexemeType.Types.INT },
            { "char",   LexemeType.Types.CHAR },
            { "string", LexemeType.Types.STRING },
            { "break",  LexemeType.Types.BREAK },
            { "array",  LexemeType.Types.ARRAY},
            { "new"  ,  LexemeType.Types.NEW},
            { "delete", LexemeType.Types.DELETE}
        };

        private static readonly Dictionary<string, LexemeType.Types> internalFunctionsList = new()
        {
            {"print", LexemeType.Types.PRINT},
            {"printLine", LexemeType.Types.PRINTLINE}
        };

        internal Scanner(string data, IServiceProvider serviceProvider, bool isFile = true)
        {
            compilerError = serviceProvider.GetRequiredService<ICompilerError>();
            if (isFile)
            {
                if (string.IsNullOrEmpty(data))
                {
                    throw new ArgumentNullException("The path is null");
                }

                if (!File.Exists(data))
                {
                    throw new FileLoadException("Unable to find file " + data);
                }

                fileData = File.ReadAllBytes(data);
                return;
            }

            fileData = Encoding.ASCII.GetBytes(data);
        }

        internal List<Lexeme?> Scan()
        {
            while (!IsEof())
            {
                start = current;
                ReadToken();
            }

            return lexemes;
        }

        internal bool IsEof()
        {
            return current == fileData.Length;
        }


        internal void ReadToken()
        {
            column++;
            char t = Advance();


            if (IsLetter(t) || t == '_')
            {
                Identifier();
                return;
            }

            if (IsDigit(t) || IsNumber(t))
            {
                Number();
                return;
            }

            if (IsPunctuation(t) || IsSymbol(t))
            {
                switch (t)
                {
                    case '(':
                        {
                            AddSymbol(t, LexemeType.Types.LEFT_PAREN);
                            break;
                        }
                    case ')': AddSymbol(t, LexemeType.Types.RIGHT_PAREN); break;
                    case '{': AddSymbol(t, LexemeType.Types.LEFT_BRACE); break;
                    case '}': AddSymbol(t, LexemeType.Types.RIGHT_BRACE); break;
                    case ',': AddSymbol(t, LexemeType.Types.COMMA); break;
                    case '.': AddSymbol(t, LexemeType.Types.DOT); break;
                    case '+': AddSymbol(t, LexemeType.Types.PLUS); break;
                    case ';': AddSymbol(t, LexemeType.Types.SEMICOLON); break;
                    case '*': AddSymbol(t, LexemeType.Types.STAR); break;
                    case '[':
                        {
                            AddSymbol(t, LexemeType.Types.LEFT_BRACE);
                            if (IsDigit(Peek()) || IsNumber(Peek()))
                            {
                                // HACK
                                start++;
                                // HACK
                                Number();
                            }

                            if (Peek() == ']')
                            {
                                AddSymbol(Peek(), LexemeType.Types.RIGHT_BRACE);
                            }

                            break;
                        }
                    case '-':
                        {
                            if (IsDigit(Peek()) || IsNumber(Peek()))
                            {
                                Number();
                            }
                            else
                            {
                                AddSymbol(t, LexemeType.Types.MINUS);
                            }

                            break;
                        }
                    case '/':
                        if (Peek() == '/')
                        {
                            while (Peek() != '\n' && !IsEof())
                            {
                                _ = Advance();
                            }
                        }
                        else if (Peek() == '*')
                        {
                            while (true)
                            {
                                if (Advance() == '*' && Peek() == '/')
                                {
                                    _ = Advance();
                                    break;
                                }
                                if (IsEof())
                                {
                                    throw new Exception("Comment is not closed");
                                }
                            }
                        }
                        else
                        {
                            AddSymbol(t, LexemeType.Types.SLASH);
                        }
                        break;


                    case '=':
                        {
                            if (Peek() == '=')
                            {
                                AddSymbol(t, LexemeType.Types.EQUAL_EQUAL);
                                break;
                            }

                            AddSymbol(t, LexemeType.Types.EQUAL);
                            break;
                        }

                    case '<':
                        {
                            if (Peek() == '=')
                            {
                                AddSymbol(t, LexemeType.Types.LESS_EQUAL);
                                break;
                            }

                            AddSymbol(t, LexemeType.Types.LESS);
                            break;
                        }

                    case '>':
                        {
                            if (Peek() == '=')
                            {
                                AddSymbol(t, LexemeType.Types.GREATER_EQUAL);
                                break;
                            }

                            AddSymbol(t, LexemeType.Types.GREATER);
                            break;
                        }


                    /*
                    case '!':
                        { 
                            if (Match('='))
                            { 
                                kind = LexemeType.Types.BANG_EQUAL;
                                break;
                            }

                            kind = LexemeType.Types.BANG;
                            break;
                        }
                position++;
                */
                    case '"': String(); break;
                    default:
                        //Lox.error(line, "Unexpected character.");
                        break;
                }
            }

            _ = IsWhiteSpace();
        }

        internal void AddSymbol(char symbol, LexemeType.Types type)
        {
            Lexeme lex = new(type, line, column);
            lex.AddToken(symbol);
            lexemes.Add(lex);
        }

        internal bool TryGetKeyWord(Lexeme? lex)
        {
            bool isKeyword = false;

            if (keywordsDictionary.TryGetValue(lex?.ToString()!, out LexemeType.Types lexemeType))
            {
                if (lex != null)
                {
                    isKeyword = lex.IsKeyWord = true;
                    lex.LexemeType = lexemeType;
                }
            }

            if (internalFunctionsList.ContainsKey(lex?.ToString()!))
            {
                if (lex != null)
                {
                    lex.LexemeType = internalFunctionsList[lex.ToString()];
                    lex.LexemeType |= LexemeType.Types.INTERNALFUNCTION;
                }
            }

            return isKeyword;
        }

        internal bool IsWhiteSpace()
        {
            if (IsEof())
            {
                return false;
            }

            char c = (char)fileData[current];
            if (char.IsWhiteSpace(c) || c == '\r' || c == '\n')
            {
                if (c == '\n')
                {
                    line++;
                    column = 0;
                }

                return true;
            }

            return false;
        }
        internal void Identifier()
        {
            while (IsLetterOrDigit(Peek()) || Peek() == '_')
            {
                _ = Advance();
            }

            string svalue = Encoding.Default.GetString(Memcopy(fileData, start, current));
            Lexeme? identifier = new(LexemeType.Types.IDENTIFIER, line, column);

            identifier.AddToken(svalue);

            _ = TryGetKeyWord(identifier);
            lexemes.Add(identifier);
        }
        internal void Number()
        {
            char temp = Peek();

            while (IsNumber(temp) || temp.Equals('.'))
            {
                _ = Advance();
                temp = Peek();
            }

            string svalue = System.Text.Encoding.Default.GetString(Memcopy(fileData, start, current));
            Lexeme? number = new(LexemeType.Types.NUMBER, line, column);

            number.AddToken(svalue);
            lexemes.Add(number);
        }


        private void String()
        {
            while (Peek() != '"' && !IsEof())
            {
                if (Peek() == '\n')
                {
                    line++;
                }

                _ = Advance();
            }

            if (IsEof())
            {
                // Log exception;
                return;
            }

            string svalue = Encoding.Default.GetString(Memcopy(fileData, start + 1, current - 1));
            Lexeme? s = new(LexemeType.Types.STRING, line, column);
            s.AddToken(svalue.ToString());
            lexemes.Add(s);
            _ = Advance();
        }

        private byte[] Memcopy(byte[] from, int start, int size)
        {
            byte[] to = new byte[current - start];
            for (int toIndex = 0, i = start; i < current; i++, toIndex++)
            {
                to[toIndex] = from[i];
            }

            return to;
        }

        private char Advance()
        {
            return !IsEof() ? (char)fileData[current++] : '\0';
        }
        internal char Peek()
        {
            return !IsEof() ? (char)fileData[current] : '\0';
        }

    }
}


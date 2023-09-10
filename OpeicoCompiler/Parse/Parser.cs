using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Expressions;
using OpeicoCompiler.Lexer;
using OpeicoCompiler.Scan;
using OpeicoCompiler.Statements;
using Microsoft.Extensions.DependencyInjection;
using static OpeicoCompiler.Expressions.Expr;

namespace OpeicoCompiler.Parse
{
    public class Parser
    {
        private List<Lexeme> tokens = new();
        private int current = 0;
        private readonly Scanner? scanner;
        public ServiceProvider Services { get; }
        private readonly ICompilerError compilerError;

        internal Parser(Scanner scanner, ServiceProvider services)
        {
            this.scanner = scanner;
            Services = services;

            compilerError = services.GetRequiredService<ICompilerError>();
        }

        internal List<Stmt> Parse()
        {
            if (scanner == null)
            {
                throw new ArgumentNullException("Scanner is null");
            }

            tokens = scanner.Scan()!;
            List<Stmt> statements = new();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private bool IsAtEnd()
        {
            return current == tokens.Count || Peek().LexemeType == LexemeType.Types.EOF;
        }

        private Lexeme Peek()
        {
            return tokens[current];
        }

        private Lexeme Previous()
        {
            return tokens[current - 1];
        }

        private Lexeme Peek(int lookAhead)
        {
            return current + lookAhead > tokens.Count ? throw new JRuntimeException("Looked pass") : tokens[current + lookAhead];
        }

        private Lexeme Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        private Stmt Declaration()
        {
            if (Match(LexemeType.Types.CLASS))
            {
                return ClassDeclaration("class");
            }


            if (Match(LexemeType.Types.FUNC))
            {
                return Function("func");
            }

            return MatchKeyWord() ? VariableDeclaration() : Statement();
        }

        private Stmt Statement()
        {
            if (Match(LexemeType.Types.INTERNALFUNCTION | LexemeType.Types.PRINTLINE))
            {
                return PrintLine();
            }

            if (Match(LexemeType.Types.INTERNALFUNCTION | LexemeType.Types.PRINT))
            {
                return Print();
            }

            if (Match(LexemeType.Types.RET))
            {
                return ReturnStatement();
            }

            if (Match(LexemeType.Types.IFF))
            {
                return IfStatement();
            }

            if (Match(LexemeType.Types.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            if (Match(LexemeType.Types.WHL))
            {
                return WhileStatement();
            }

            if (Match(LexemeType.Types.FOR))
            {
                return ForStatement();
            }

            return Match(LexemeType.Types.BREAK) ? BreakStatement() : ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            _ = Consume(LexemeType.Types.LEFT_PAREN, Previous());

            Expr? condition = Expr();

            if (condition == null)
            {
                compilerError.AddError("no if condition statement");
            }

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Previous());

            Stmt thenBlock = Statement();
            Stmt? elseBlock = null;

            if (Match(LexemeType.Types.ELSS))
            {
                elseBlock = Statement();
            }

            return condition != null && elseBlock != null
                ? (Stmt)new Stmt.If(condition, thenBlock, elseBlock)
                : throw new JRuntimeException("If statement failure");
        }

        private Stmt WhileStatement()
        {
            _ = Consume(LexemeType.Types.LEFT_PAREN, Previous());

            Expr condition = Expr();

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Previous());

            Stmt whileBlock = Statement();

            return new Stmt.While(condition, whileBlock);
        }

        private Stmt ForStatement()
        {
            _ = Consume(LexemeType.Types.LEFT_PAREN, Previous());
            if (MatchKeyWord())
            {
                Stmt.Var initCondition = VariableDeclaration();

                Expr.Binary breakCondition = (Expr.Binary)Equality();
                _ = Consume(LexemeType.Types.SEMICOLON, Previous());

                Expr incrementCondition = Unary();

                _ = Consume(LexemeType.Types.RIGHT_PAREN, Previous());

                Stmt forBody = null!;
                if (Match(LexemeType.Types.LEFT_BRACE))
                {
                    forBody = Statement();
                }

                _ = Consume(LexemeType.Types.RIGHT_BRACE, Peek());

                return new Stmt.For(initCondition, breakCondition, incrementCondition, forBody);
            }

            throw new JRuntimeException("no valid variable");
        }

        private Stmt BreakStatement()
        {
            _ = Consume(LexemeType.Types.SEMICOLON, Previous());
            Expr expr = Expr();
            return new Stmt.Break(expr);
        }


        private Stmt ReturnStatement()
        {
            Lexeme keyword = Previous();
            Expr value = null!;

            if (!Check(LexemeType.Types.SEMICOLON))
            {
                value = Expr();
            }

            if (value == null)
            {
                throw new JRuntimeException("unable to parse return statement");
            }

            _ = Consume(LexemeType.Types.SEMICOLON, Previous());
            return new Stmt.Return(keyword, value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expr();
            _ = Consume(LexemeType.Types.SEMICOLON, Peek());
            return new Stmt.Expression(expr);
        }

        private Stmt PrintLine()
        {
            _ = Previous();
            _ = Consume(LexemeType.Types.LEFT_PAREN, Peek());

            Expr value = Expr();

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Peek());
            _ = Consume(LexemeType.Types.SEMICOLON, Peek());

            return new Stmt.PrintLine(value);
        }

        private Stmt Print()
        {
            _ = Previous();
            _ = Consume(LexemeType.Types.LEFT_PAREN, Peek());

            Expr value = Expr();

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Peek());
            _ = Consume(LexemeType.Types.SEMICOLON, Peek());

            return new Stmt.Print(value);
        }

        private bool MatchKeyWord()
        {
            if (MatchInternalFunction())
            {
                return true;
            }

            return Match(LexemeType.Types.INT) ||
                Match(LexemeType.Types.STRING) ||
                Match(LexemeType.Types.FLOAT) ||
                Match(LexemeType.Types.DOUBLE) ||
                Match(LexemeType.Types.RUN);
        }

        private Lexeme Consume(LexemeType.Types type, Lexeme currentLexeme)
        {
            if (Check(type))
            {
                return Advance();
            }

            compilerError.AddError($"Error trying to parse '{currentLexeme}' not valid at line:{currentLexeme.LineNumber} column:{currentLexeme.ColumnNumber} ");

            return new Lexeme(LexemeType.Types.UNDEFINED, 0, 0);
        }

        private Lexeme ConsumeKeyword()
        {
            if (CheckKeyWord())
            {
                return Advance();
            }

            compilerError.AddError("Unable to Keyword");
            return new Lexeme(LexemeType.Types.UNDEFINED, 0, 0);
        }

        private bool Match(LexemeType.Types lexType)
        {
            if (Check(lexType))
            {
                _ = Advance();
                return true;
            }

            return false;
        }

        private bool MatchInternalFunction()
        {
            if (Check(LexemeType.Types.INTERNALFUNCTION))
            {
                _ = Advance();
                return true;
            }

            return false;
        }

        private bool Check(LexemeType.Types type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().IsKeyWord ? Peek().LexemeType == type : Peek().LexemeType == type;
        }

        private bool CheckKeyWord()
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().IsKeyWord;
        }

        private Stmt Function(string kind)
        {
            Lexeme name = Consume(LexemeType.Types.IDENTIFIER, Peek());
            _ = Consume(LexemeType.Types.LEFT_PAREN, Peek());
            List<TypeParameterMap> typeMap = new();

            if (!Check(LexemeType.Types.RIGHT_PAREN))
            {
                do
                {
                    Lexeme parameterType = ConsumeKeyword();
                    Expr varName = Expr();
                    if (parameterType.IsKeyWord)
                    {
                        typeMap.Add(new TypeParameterMap(parameterType, varName));
                    }
                }
                while (Match(LexemeType.Types.COMMA));
            }

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Peek());
            _ = Consume(LexemeType.Types.EQUAL, Peek());
            _ = Consume(LexemeType.Types.LEFT_BRACE, Peek());

            List<Stmt> statements = Block();

            Stmt.Function stmt = new(name, typeMap, statements);
            return stmt;
        }

        private Stmt ClassDeclaration(string kind)
        {
            Lexeme name = Consume(LexemeType.Types.IDENTIFIER, Peek());
            _ = Consume(LexemeType.Types.EQUAL, Peek());
            _ = Consume(LexemeType.Types.LEFT_BRACE, Peek());
            List<Stmt.Function> functions = new();
            List<Stmt> variableDeclarations = new();

            if (!Check(LexemeType.Types.RIGHT_BRACE))
            {
                while (true)
                {
                    Lexeme isFunc = Peek();
                    if (!isFunc.IsKeyWord)
                    {
                        break;
                    }

                    if (isFunc.LexemeType == LexemeType.Types.FUNC)
                    {
                        functions.Add((Stmt.Function)Declaration());
                    }

                    if (MatchKeyWord())
                    {
                        variableDeclarations.Add(VariableDeclaration());
                    }
                }
            }

            _ = Consume(LexemeType.Types.RIGHT_BRACE, Peek());

            return new Stmt.Class(name, functions, variableDeclarations);
        }

        private List<Stmt> Block()
        {
            List<Stmt> stmts = new();
            while (!Check(LexemeType.Types.RIGHT_BRACE) && !IsAtEnd())
            {
                stmts.Add(Declaration());
            }

            _ = Consume(LexemeType.Types.RIGHT_BRACE, Peek());
            return stmts;
        }

        private Stmt.Var VariableDeclaration()
        {
            Lexeme name = Consume(LexemeType.Types.IDENTIFIER, Peek());

            if (Match(LexemeType.Types.EQUAL))
            {
                Expr initalizedState = Expr();
                _ = Consume(LexemeType.Types.SEMICOLON, Peek());
                return new Stmt.Var(name, initalizedState);
            }

            _ = Consume(LexemeType.Types.SEMICOLON, Peek());

            return new Stmt.Var(name);
        }

        private Expr Expr()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(LexemeType.Types.EQUAL))
            {
                _ = Previous();
                Expr value = Assignment();

                //expr.ExpressionLexemeName =

                if (expr is Expr.Variable &&
                    ((Expr.Variable)expr) != null &&
                    ((Expr.Variable)expr).ExpressionLexeme != null)
                {
                    Expr.Variable variable = (Expr.Variable)expr;
                    //expr.ExpressionLexemeName = variable.ExpressionLexeme.
                    if (variable != null && variable.ExpressionLexeme != null)
                    {
                        return new Expr.Assign(variable.ExpressionLexeme, value);
                    }
                    //> Classes assign-set
                }

                if (expr is Expr.Get &&
                    ((Expr.Get)expr) != null &&
                    ((Expr.Get)expr).ExpressionLexeme != null)
                {
                    Expr.Get get = (Expr.Get)expr;
                    if (get != null && get.ExpressionLexeme != null)
                    {
                        return new Expr.Set(get, get.ExpressionLexeme, value);
                    }
                    //< Classes assign-set
                }

                //error(equals, "Invalid assignment target."); // [no-throw]
            }

            return expr;
        }


        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(LexemeType.Types.BANG_EQUAL) || Match(LexemeType.Types.EQUAL_EQUAL))
            {
                Lexeme op = Previous();

                // Bug - I need to consume the second operator when doing comparisions.
                // Hack I need to figure out a better way to do this.
                Lexeme secondOp = Advance();
                if (secondOp.LexemeType == LexemeType.Types.EQUAL)
                {
                    op.AddToken(secondOp);
                    op.LexemeType = LexemeType.Types.EQUAL_EQUAL;
                }
                // see what it is? maybe update the lexeme?

                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        //< Statements and State parse-assignment
        //> Control Flow or
        private Expr Or()
        {
            Expr expr = And();

            while (Match(LexemeType.Types.OR))
            {
                Lexeme op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(LexemeType.Types.AND))
            {
                Lexeme op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(LexemeType.Types.GREATER) || Match(LexemeType.Types.GREATER_EQUAL) || Match(LexemeType.Types.LESS) || Match(LexemeType.Types.LESS_EQUAL))
            {
                Lexeme op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(LexemeType.Types.MINUS) || Match(LexemeType.Types.PLUS))
            {
                Lexeme op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(LexemeType.Types.SLASH) || Match(LexemeType.Types.STAR))
            {
                Lexeme op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            // Expr expr = Array();

            if (Match(LexemeType.Types.BANG))
            {
                _ = Previous();
                _ = Unary();
                //******* return new Expr.Unary(op, right);
            }

            Lexeme idLexeme = Peek();
            if (idLexeme.LexemeType == LexemeType.Types.NEW)
            {
                _ = Match(LexemeType.Types.NEW);
                return new NewDeclarationExpr(Expr());
            }

            if (idLexeme.LexemeType == LexemeType.Types.DELETE)
            {
                _ = Match(LexemeType.Types.DELETE);

                if (Peek().LexemeType == LexemeType.Types.IDENTIFIER)
                {
                    // If we get here then we have an identifier on the right hand side.
                    // the parent expression will need to validate that it is a variable.
                    Lexeme lexeme = Consume(LexemeType.Types.IDENTIFIER, Peek());
                    Variable variable = new(lexeme);
                    return new DeleteDeclarationExpr(variable);
                }
            }

            if (idLexeme.LexemeType == LexemeType.Types.IDENTIFIER)
            {
                Lexeme isPlus = Peek(1);
                Lexeme isPlusPlus = Peek(2);
                if (isPlus.LexemeType == LexemeType.Types.PLUS && isPlusPlus.LexemeType == LexemeType.Types.PLUS)
                {
                    _ = Match(LexemeType.Types.IDENTIFIER);
                    _ = Match(LexemeType.Types.PLUS);
                    _ = Match(LexemeType.Types.PLUS);
                    _ = Match(LexemeType.Types.SEMICOLON);
                    return new Expr.Unary(idLexeme, LexemeType.Types.PLUSPLUS);
                }
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(LexemeType.Types.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(LexemeType.Types.DOT))
                {
                    Lexeme name = Consume(LexemeType.Types.IDENTIFIER, Peek());
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expr Primary()
        {
            if (Match(LexemeType.Types.FLS))
            {
                return new Expr.Literal(Previous(), LexemeType.Types.FLS);
            }

            if (Match(LexemeType.Types.TRU))
            {
                return new Expr.Literal(Previous(), LexemeType.Types.TRU);
            }

            if (Match(LexemeType.Types.NL))
            {
                return new Expr.Literal(Previous(), LexemeType.Types.NL);
            }

            if (Match(LexemeType.Types.STRING))
            {
                return new Expr.Literal(Previous(), LexemeType.Types.STRING);
            }

            if (Match(LexemeType.Types.NUMBER))
            {
                return new Expr.Literal(Previous(), LexemeType.Types.NUMBER);
            }

            if (Match(LexemeType.Types.ARRAY))
            {
                _ = Consume(LexemeType.Types.LEFT_BRACE, Peek());
                Lexeme size = Consume(LexemeType.Types.NUMBER, Peek());
                _ = Consume(LexemeType.Types.RIGHT_BRACE, Peek());
                Lexeme name = new(LexemeType.Types.ARRAY, 0, 0);
                name.AddToken("array");
                return new Expr.ArrayDeclarationExpr(name, size);
            }

            if (Match(LexemeType.Types.NEW))
            {
                _ = Expr();
            }

            if (Match(LexemeType.Types.IDENTIFIER))
            {
                Lexeme identifierName = Previous();
                if (Match(LexemeType.Types.LEFT_BRACE))
                {
                    Lexeme index = Consume(LexemeType.Types.NUMBER, Peek());
                    _ = Consume(LexemeType.Types.RIGHT_BRACE, Peek());
                    return Match(LexemeType.Types.EQUAL)
                        ? new Expr.ArrayAccessExpr(identifierName, index, Expr())
                        : (Expr)new Expr.ArrayAccessExpr(identifierName, index);
                }

                return new Expr.Variable(identifierName);
            }

            if (Match(LexemeType.Types.LEFT_PAREN))
            {
                Expr expr = Expr();
                _ = Consume(LexemeType.Types.RIGHT_PAREN, Peek());
                return new Expr.Grouping(expr);
            }

            return Match(LexemeType.Types.RUN) ? Assignment() : throw new Exception(Peek() + "Expect expr");
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new();
            if (!Check(LexemeType.Types.RIGHT_PAREN))
            {
                do
                {
                    ////> check-max-arity
                    //if (arguments.size() >= 255)
                    //{
                    //    error(peek(), "Can't have more than 255 arguments.");
                    //}
                    ////< check-max-arity

                    arguments.Add(Expr());

                } while (Match(LexemeType.Types.COMMA));
            }


            Array callableServices = Enum.GetValues(typeof(OpeicoCompiler.SystemCalls.CallableServices));
            bool isCallable = false;
            foreach (object? callableService in callableServices)
            {
                if (callableService != null)
                {
                    string? serviceName = callableService.ToString();
                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        isCallable = serviceName.Equals(callee.ExpressionLexeme?.ToString());
                    }
                }
            }

            _ = Consume(LexemeType.Types.RIGHT_PAREN, Peek());

            return new Expr.Call(callee, isCallable, arguments);
        }
    }
}
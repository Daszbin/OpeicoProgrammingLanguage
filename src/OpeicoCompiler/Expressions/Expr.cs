using OpeicoCompiler.Lexer;

namespace OpeicoCompiler.Expressions
{
    internal abstract class Expr
    {
        internal interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitCallExpr(Call expr);
            R VisitGetExpr(Get expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitLogicalExpr(Logical expr);
            R VisitSetExpr(Set expr);
            R VisitSuperExpr(Super expr);
            R VisitThisExpr(This expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
            R VisitLexemeTypeLiteral(LexemeTypeLiteral expr);
            R VisitArrayExpr(ArrayDeclarationExpr expr);
            R VisitNewExpr(NewDeclarationExpr expr);
            R VisitArrayAccessExpr(ArrayAccessExpr expr);
            R VisitDeleteExpr(DeleteDeclarationExpr expr);
        }

        internal abstract R Accept<R>(Expr.IVisitor<R> visitor);

        internal Lexeme? initializerContextVariableName;

        internal Lexeme? ExpressionLexeme { get; set; }
        internal string ExpressionLexemeName => ExpressionLexeme?.ToString()!;
        internal class Assign : Expr
        {
            internal readonly Expr value;

            internal Assign(Lexeme ExpressionLexeme, Expr value)
            {
                base.ExpressionLexeme = ExpressionLexeme;
                this.value = value;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }
        internal class Variable : Expr
        {
            internal LexemeType.Types lexemeType;

            internal Variable(Lexeme ExpressionLexeme)
            {
                this.ExpressionLexeme = ExpressionLexeme;
                lexemeType = ExpressionLexeme.LexemeType;
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }
        internal class Binary : Expr
        {
            internal Expr? left;
            internal Lexeme? op;
            internal Expr? right;

            internal Binary(Expr expr, Lexeme op, Expr right)
            {
                left = expr;
                this.op = op;
                this.right = right;
            }

            internal Binary()
            {
                left = null;
                right = null;
                op = null;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

        internal class Increment : Expr
        {
            internal Lexeme variable;
            internal Lexeme opLexeme;

            internal Increment(Lexeme variable, Lexeme opLexeme)
            {
                this.variable = variable;
                this.opLexeme = opLexeme;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }

        internal class NewDeclarationExpr : Expr
        {
            public Expr NewDeclarationExprInit { get; }

            internal NewDeclarationExpr(Expr expr)
            {
                NewDeclarationExprInit = expr;
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitNewExpr(this);
            }
        }

        internal class DeleteDeclarationExpr : Expr
        {
            public Expr variable;

            internal DeleteDeclarationExpr(Expr expr)
            {
                variable = expr;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitDeleteExpr(this);
            }
        }

        internal class ArrayDeclarationExpr : Expr
        {
            internal int ArraySize { get; }
            internal Lexeme ArrayDeclarationName { get; }

            internal ArrayDeclarationExpr(int size)
            {
                ArraySize = size;
                ArrayDeclarationName = new Lexeme(0, 0, 0);
            }

            internal ArrayDeclarationExpr(Lexeme arrayDeclarationName, Lexeme arraySize)
            {
                ArraySize = int.Parse(arraySize.ToString());
                ExpressionLexeme = arrayDeclarationName;
                ArrayDeclarationName = arrayDeclarationName;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitArrayExpr(this);
            }
        }
        internal class ArrayAccessExpr : Expr
        {
            internal int ArrayIndex { get; }
            internal Lexeme ArrayVariableName { get; }
            internal Expr LvalueExpr { get; }
            internal bool HasInitalizer { get; }

            internal ArrayAccessExpr(Lexeme arrayVariableName, Lexeme arraySize)
            {
                ArrayIndex = int.Parse(arraySize.ToString());
                ExpressionLexeme = ArrayVariableName = arrayVariableName;
                HasInitalizer = false;
                LvalueExpr = new DefaultExpr();
            }

            internal ArrayAccessExpr(Lexeme arrayVariableName, Lexeme arraySize, Expr expr)
            {
                ArrayIndex = int.Parse(arraySize.ToString());
                ExpressionLexeme = ArrayVariableName = arrayVariableName;
                LvalueExpr = expr;
                HasInitalizer = true;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitArrayAccessExpr(this);
            }
        }
        internal class Call : Expr
        {
            internal Expr callee;
            internal List<Expr> arguments;
            internal bool isOpeicoCallable = false;

            internal Call(Expr callee, bool isCallable, List<Expr> arguments)
            {
                this.callee = callee;
                this.arguments = arguments;
                isOpeicoCallable = isCallable;
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitCallExpr(this);
            }
        }
        internal class Get : Expr
        {
            internal Expr expr;
            internal Get(Expr expr, Lexeme lex)
            {
                this.expr = expr;
                ExpressionLexeme = lex;
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGetExpr(this);
            }
        }
        internal class Unary : Expr
        {
            internal Unary(Lexeme lex, LexemeType.Types lexemeType)
            {
                ExpressionLexeme = lex;
                this.LexemeType = lexemeType;
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public LexemeType.Types LexemeType { get; }
        }
        internal class Grouping : Expr
        {
            internal Expr? expression;

            internal Grouping(Expr expr)
            {
                expression = expr;
            }

            internal Grouping()
            {
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }
        internal class Literal : Expr
        {
            private readonly object? value;

            internal Literal(Lexeme literal, LexemeType.Types type)
            {
                ExpressionLexeme = literal;
                value = literal;
                this.Type = type;
            }
            internal Literal()
            {
            }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            internal object? LiteralValue()
            {
                if (value == null)
                {
                    throw new ArgumentNullException("literal");
                }

                LexemeTypeLiteral? v = new()
                {
                    lexemeType = Type,
                    literal = value.ToString()
                };

                return v;
                //return literal;
            }

            internal LexemeType.Types Type { get; }
        }
        internal class Logical : Expr
        {
            internal Logical(Expr expr, Lexeme lex, Expr right)
            {
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }
        internal class Set : Expr
        {
            internal Set(Expr expr, Lexeme lex, Expr right)
            {
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }
        internal class Super : Expr
        {
            internal Super(Expr expr, Lexeme lex, Expr right)
            {
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }
        internal class This : Expr
        {
            internal This(Expr expr, Lexeme lex, Expr right)
            {
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }
        internal class DefaultExpr : Expr
        {
            internal DefaultExpr()
            { }
            internal override R Accept<R>(IVisitor<R> visitor)
            {
                throw new NotImplementedException();
            }
        }
        internal class LexemeTypeLiteral : Expr
        {
            internal LexemeType.Types lexemeType;
            internal object? literal;

            internal new object Literal
            {
                get => literal ?? string.Empty;
                set => literal = value;
            }

            internal LexemeType.Types LexemeType
            {
                get => lexemeType;
                set => lexemeType = value;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLexemeTypeLiteral(this);
            }
        }
    }
}

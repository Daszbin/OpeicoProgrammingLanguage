using OpeicoCompiler.Expressions;
using OpeicoCompiler.Lexer;
using OpeicoCompiler.Parse;

namespace OpeicoCompiler.Statements
{
    internal abstract class Stmt
    {
        internal interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitClassStmt(Class stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintLine(PrintLine stmt);
            R VisitPrint(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
            R VisitBreakStmt(Break stmt);

            R VisitForStmt(For stmt);
        }
        internal abstract R Accept<R>(Stmt.IVisitor<R> vistor);

        internal Lexeme StmtLexeme { get; set; } = new Lexeme();

        internal string StmtLexemeName => StmtLexeme.ToString();

        internal class Block : Stmt
        {
            internal Block(List<Stmt> statements)
            {
                this.statements = statements;
            }

            internal List<Stmt> statements;
            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitBlockStmt(this);
            }
        }
        internal class Function : Stmt
        {
            internal List<Stmt> body = new();
            internal List<TypeParameterMap> typeParameterMaps;

            public override bool Equals(object? obj)
            {
                return StmtLexemeName.Equals(obj);
            }
            internal Function(Lexeme stmtLexeme, List<TypeParameterMap> parametersMap, List<Stmt> body)
            {
                if (!stmtLexeme.ToString().All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    throw new Exception("Function {ExpressionLexeme.ToString()} has an invalid ExpressionLexeme");
                }
                StmtLexeme = stmtLexeme;
                typeParameterMaps = parametersMap;
                this.body = body;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitFunctionStmt(this);
            }

            internal List<TypeParameterMap> Params => typeParameterMaps;

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        internal class Class : Stmt
        {
            internal Lexeme name;
            internal List<Stmt.Function> methods;
            internal List<Stmt> variableDeclarations;
            internal Expr.Variable? superClass;

            internal Class(Lexeme name, List<Stmt.Function> methods, List<Stmt> variableDeclarations)
            {
                this.name = name;
                this.methods = methods;
                this.variableDeclarations = variableDeclarations;
                superClass = null;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitClassStmt(this);
            }
        }
        internal class Expression : Stmt
        {
            internal Expr Expr;

            internal Expression(Expr expr)
            {
                Expr = expr;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitExpressionStmt(this);
            }
        }
        internal class If : Stmt
        {
            internal Expr condition;
            internal Stmt thenBranch;
            internal Stmt elseBranch;

            internal If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

        }
        internal class PrintLine : Stmt
        {
            internal Expr? expr;

            internal PrintLine(Expr expr)
            {
                this.expr = expr;
            }

            internal PrintLine()
            {
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitPrintLine(this);
            }
        }
        internal class Print : Stmt
        {
            internal Expr? expr;

            internal Print(Expr expr)
            {
                this.expr = expr;
            }

            internal Print()
            {
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitPrint(this);
            }
        }
        internal class Var : Stmt
        {
            internal Lexeme? name;
            internal Expr? exprInitializer;
            internal bool isInitalizedVar = false;

            internal Var(Lexeme name, Expr expr)
            {
                if (!name.ToString().All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    throw new Exception("Variable {ExpressionLexeme.ToString()} has an invalid ExpressionLexeme");
                }
                this.name = name;
                exprInitializer = expr;
                exprInitializer.initializerContextVariableName = name;
                isInitalizedVar = true;

            }

            internal Var(Lexeme name)
            {
                if (!name.ToString().All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    throw new Exception("Variable {ExpressionLexeme.ToString()} has an invalid ExpressionLexeme");
                }
                this.name = name;
                exprInitializer = null;
            }

            internal Var()
            {
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitVarStmt(this);
            }
        }

        internal class While : Stmt
        {
            internal Expr condition;
            internal Stmt whileBlock;

            internal While(Expr condition, Stmt whileBlock)
            {
                this.condition = condition;
                this.whileBlock = whileBlock;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitWhileStmt(this);
            }
        }

        internal class For : Stmt
        {
            private readonly Expr incExpr;

            internal For(Stmt.Var init, Expr breakExpr, Expr incExpr, Stmt forBody)
            {
                this.Init = init;
                this.BreakExpr = breakExpr;
                this.incExpr = incExpr;
                this.ForBody = forBody;
            }

            internal Stmt.Var Init { get; }
            internal Expr BreakExpr { get; }
            internal Expr IncExpr => incExpr;
            internal Stmt ForBody { get; }

            internal override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }
        }
        internal class Return : Stmt
        {
            internal Lexeme? keyword;
            internal Expr? expr;
            internal Return(Lexeme keyword, Expr expr)
            {
                this.keyword = keyword;
                this.expr = expr;
            }

            internal Return()
            {
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitReturnStmt(this);
            }
        }
        internal class Break : Stmt
        {
            internal Expr expr;

            internal Break(Expr expr)
            {
                this.expr = expr;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                return vistor.VisitBreakStmt(this);
            }
        }
        internal class DefaultStatement : Stmt
        {
            // Does nothing used for return values;
            internal override R Accept<R>(IVisitor<R> vistor)
            {
                throw new NotImplementedException();
            }
        }

        internal class LiteralLexemeExpression : Stmt
        {
            internal Expr.LexemeTypeLiteral ltl;

            internal LiteralLexemeExpression(Expr.LexemeTypeLiteral lexemeTypeLiteral)
            {
                ltl = lexemeTypeLiteral;
            }

            internal override R Accept<R>(IVisitor<R> vistor)
            {
                throw new NotImplementedException();
            }
        }
    }
}

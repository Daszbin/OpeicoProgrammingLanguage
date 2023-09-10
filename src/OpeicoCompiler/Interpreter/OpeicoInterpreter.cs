using OpeicoCompiler.Exceptions;
using OpeicoCompiler.Expressions;
using OpeicoCompiler.Lexer;
using OpeicoCompiler.Statements;
using OpeicoCompiler.SystemCalls;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using static OpeicoCompiler.Expressions.Expr;
using static OpeicoCompiler.Interpreter.StackFrame;

namespace OpeicoCompiler.Interpreter
{
    internal class OpeicoInterpreter : Stmt.IVisitor<Stmt>, IVisitor<object>
    {
        private readonly OpeicoEnvironment globals;
        private OpeicoEnvironment environment;
        private readonly Dictionary<Expr, int?> locals = new();
        private readonly Stack<StackFrame> frames = new();
        private readonly string globalScope = "__global__scope__";
        private readonly int __max_stack_depth__ = 500;

        internal OpeicoInterpreter(ServiceProvider services)
        {
            environment = globals = new();
            ServiceProvider = services;

            if (ServiceProvider == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            globals.Define("csharp", ServiceProvider.GetService<ICSharp>());
            globals.Define("clock", ServiceProvider.GetService<ISystemClock>());
            globals.Define("fileOpen", services.GetService<IFileOpen>());
            globals.Define("getAvailableMemory", services.GetService<IGetAvailableMemory>());
        }

        internal void Interpret(List<Stmt> statements)
        {
            // populate the env with the function call locations
            // only functions are populated in the env
            // classes will need to be added. 
            // no local variables.
            frames.Clear();
            frames.Push(new(globalScope));
            foreach (Stmt stmt in statements)
            {
                if (stmt is Stmt.Function or Stmt.Class)
                {
                    Execute(stmt);
                }
            }

            Lexeme lexeme = new(LexemeType.Types.IDENTIFIER, 0, 0);
            lexeme.AddToken("main");
            Expr.Variable functionName = new(lexeme);
            Expr.Call call = new(functionName, false, new());
            Stmt.Expression expression = new(call);
            Execute(expression);
        }

        private void Execute(Stmt stmt)
        {
            _ = stmt.Accept(this);
        }

        internal void ExecuteBlock(List<Stmt> statements, OpeicoEnvironment env)
        {
            OpeicoEnvironment previous = environment;

            try
            {
                environment = env;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                environment = previous;
            }
        }
        Stmt Stmt.IVisitor<Stmt>.VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new(environment));
            return new Stmt.DefaultStatement();
        }

        Stmt Stmt.IVisitor<Stmt>.VisitFunctionStmt(Stmt.Function stmt)
        {
            OpeicoFunction? functionCallable = new(stmt, environment, false);
            environment.Define(stmt.StmtLexemeName, functionCallable);
            return new Stmt.DefaultStatement();
        }
        Stmt Stmt.IVisitor<Stmt>.VisitClassStmt(Stmt.Class stmt)
        {
            object? superclass = null;
            if (stmt.superClass != null)
            {
                superclass = Evaluate(stmt.superClass);
            }

            environment.Define(stmt.name.ToString(), null);

            if (stmt.superClass != null)
            {
                environment = new(environment);
                environment.Define("super", superclass);
            }

            Dictionary<string, OpeicoFunction> functions = new();
            foreach (Stmt.Function method in stmt.methods)
            {
                OpeicoFunction OpeicoFunction = new(method, environment, false);
                functions.Add(method.StmtLexemeName, OpeicoFunction);
            }

            OpeicoClass? OpeicoClass = new(stmt.name.ToString(), ((OpeicoClass)superclass!)!, functions);

            if (superclass != null)
            {
                environment = new(environment);
                environment.Define("super", superclass);
            }

            environment.Assign(stmt.name, OpeicoClass);
            return new Stmt.DefaultStatement();
        }

        public Stmt VisitExpressionStmt(Stmt.Expression stmt)
        {
            _ = Evaluate(stmt.Expr);
            return new Stmt.DefaultStatement();
        }

        public Stmt VisitIfStmt(Stmt.If stmt)
        {
            Stmt.DefaultStatement defaultStatement = new();

            if (IsTrue(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return defaultStatement;
        }
        public Stmt VisitPrintLine(Stmt.PrintLine stmt)
        {
            if (stmt.expr != null)
            {
                _ = VisitPrintAllInternal(stmt.expr, Console.WriteLine);
            }

            return new Stmt.PrintLine();
        }
        public Stmt VisitPrint(Stmt.Print stmt)
        {
            if (stmt.expr != null)
            {
                _ = VisitPrintAllInternal(stmt.expr, Console.Write);
            }

            return new Stmt.Print();
        }


        private Stmt.Print VisitPrintAllInternal(Expr expr, Action<object> printAction)
        {
            if (expr != null)
            {
                PrintLiteralExpr(expr, printAction);

                if (expr is Variable)
                {
                    _ = PrintVariableLookUp((Expr.Variable)expr, o =>
                    {
                        printAction(o);
                    });
                }

                if (expr is ArrayAccessExpr)
                {
                    ArrayAccessExpr? variableExpression = expr as ArrayAccessExpr;
                    string? variableName = variableExpression?.ArrayVariableName.ToString();

                    if (variableName != null && frames.Peek().TryGetStackVariableByName(variableName, out StackVariableState? stackVariableState))
                    {
                        if (variableExpression != null)
                        {
                            object? arrayData = stackVariableState?.arrayValues[variableExpression.ArrayIndex];
                            if (arrayData is Literal)
                            {
                                PrintLiteral((Expr.Literal)arrayData, printAction);
                            }
                        }
                    }
                }
            }

            return new();
        }

        private void PrintLiteralExpr(Expr expr, Action<object> printAction)
        {
            if (expr is Expr.Literal or Expr.LexemeTypeLiteral)
            {
                if (Evaluate(expr) is LexemeTypeLiteral lexemeTypeLiteral)
                {
                    PrintLiteral(lexemeTypeLiteral, printAction);
                }
            }
        }

        private void PrintLiteral(Expr.Literal expr, Action<object> printAction)
        {
            printAction(expr.ExpressionLexemeName);
        }

        private void PrintLiteral(Expr.LexemeTypeLiteral expr, Action<object> printAction)
        {
            if (expr?.Literal != null)
            {
                if (expr?.Literal != null)
                {
                    printAction(expr?.Literal!);
                }
            }
        }

        private bool PrintVariableLookUp(Expr.Variable expr, Action<object> printTypeAction)
        {
            if (expr is not null)
            {
                if (expr.ExpressionLexeme != null)
                {
                    object? variable = LookUpVariable(expr.ExpressionLexeme, expr);

                    if (variable is StackVariableState stackVariable)
                    {
                        if (stackVariable.Value is Expr.LexemeTypeLiteral typeLiteral)
                        {
                            object? o = typeLiteral.literal;
                            if (o != null)
                            {
                                printTypeAction(o);
                            }
                        }

                        if (stackVariable.Value is Expr.Literal literal)
                        {
                            printTypeAction(literal.ExpressionLexeme?.ToString()!);
                        }

                        if (stackVariable.Value is string)
                        {
                            printTypeAction(stackVariable.Value);
                        }
                    }

                    if (variable is Expr.LexemeTypeLiteral lexemeTypeLiteral)
                    {
                        object? literal = lexemeTypeLiteral.literal;
                        if (literal != null)
                        {
                            printTypeAction(literal);
                        }
                    }
                    else
                    {
                        if (variable is Literal literal)
                        {
                            printTypeAction(literal.ExpressionLexeme?.ToString()!);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public Stmt VisitReturnStmt(Stmt.Return stmt)
        {
            object? value = null;
            if (stmt.expr != null)
            {
                value = Evaluate(stmt.expr);
            }

            throw new Return(value);
        }

        public Stmt VisitBreakStmt(Stmt.Break stmt)
        {
            Stmt.Return returnStatement = new();
            return VisitReturnStmt(returnStatement);
        }

        public Stmt VisitForStmt(Stmt.For stmt)
        {
            _ = stmt.Init.Accept(this);

            object? breakExpression = Evaluate(stmt.BreakExpr);
            while (IsTrue(breakExpression))
            {
                Execute(stmt.ForBody);
                _ = stmt.IncExpr.Accept(this);
                breakExpression = Evaluate(stmt.BreakExpr);
            }

            return new Stmt.DefaultStatement();
        }

        public Stmt VisitVarStmt(Stmt.Var stmt)
        {
            // Hack. If the expr is a method call assignment
            // i.e. var x = foo(); 
            // AddVariable does an Evaluate which will put the assigee
            // in the stack frame of foo().
            // doing the check of the initializer allows the code
            // to put the variable in the callers stack.
            object? value = frames.Peek().AddVariable(stmt, this);
            if (stmt.exprInitializer is Call)
            {
                _ = frames.Pop();
                frames.Peek().AddVariable(stmt.name?.ToString()!, value, stmt.name?.GetType()!, stmt.exprInitializer);

                //This could be a problem in the future.
                //The stack frame is popped but never pushed.
                //There could be issues when/if CTOR is implemented
            }

            environment.Define(stmt.name?.ToString()!, value);
            return new Stmt.DefaultStatement();
        }
        public Stmt VisitWhileStmt(Stmt.While stmt)
        {
            if (frames.Count >= __max_stack_depth__)
            {
                throw new JRuntimeException($"Stack depth exceeded. Depth = {frames.Count}");
            }
            while (IsTrue(Evaluate(stmt.condition)))
            {
                Execute(stmt.whileBlock);
            }

            return new Stmt.DefaultStatement();
        }
        public object VisitLexemeTypeLiteral(Expr.LexemeTypeLiteral expr)
        {
            return expr.Accept(this);
        }

        internal object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }
        public object VisitAssignExpr(Expr.Assign expr)
        {
            object? value = Evaluate(expr.value);
            StackVariableState stackVariableState = new()
            {
                Name = expr.ExpressionLexeme?.ToString()!,
                Value = value,
                type = expr.GetType(),
                expressionContext = expr,
            };

            StackFrame stackFrame = frames.Peek();

            if (!stackFrame.UpdateVariable(expr.ExpressionLexeme?.ToString() ?? string.Empty, stackVariableState))
            {
                if (expr.ExpressionLexeme != null)
                {
                    Stmt.Var newVar = new(expr.ExpressionLexeme, expr);
                    _ = stackFrame.AddVariable(newVar, this);
                }
            }

            Debug.Assert(value != null, nameof(value) + " != null");
            return value;
        }

        private (Literal LiteralValue, LexemeType.Types LiteralType) GetLiteralData(Expr expr)
        {
            if (expr is Variable)
            {
                StackVariableState? stackVariableState = Evaluate(expr) as StackVariableState;

                if (stackVariableState?.expressionContext is Literal context)
                {
                    Literal literal = context;
                    LexemeType.Types lexemeType = ((LexemeTypeLiteral)stackVariableState.Value!)!.LexemeType;

                    return (
                        literal,
                        lexemeType);
                }

                if (stackVariableState?.expressionContext is Assign assign)
                {
                    Literal literal = (Literal)assign.value;
                    LexemeType.Types lexemeType = assign.ExpressionLexeme!.LexemeType;

                    return (
                        literal,
                        lexemeType);
                }
            }

            return expr is Literal expr1 ? ((Literal LiteralValue, LexemeType.Types LiteralType))(expr1, expr1.Type) : throw new JRuntimeException("Can't get literal data" + expr);

            //return VisitBinaryExpr(expr);
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            (Literal literalValue, LexemeType.Types rightValueType) = GetLiteralData(expr.right!);
            (Literal literal, LexemeType.Types leftValueType) = GetLiteralData(expr.left!);
            object leftValue = literal.ExpressionLexeme?.ToString()!;

            object rightValue = literalValue.ExpressionLexeme?.ToString()!;

            return expr.op?.ToString() switch
            {
                "!=" => !IsEqual(leftValue, rightValue),
                "==" => IsEqual(leftValue, rightValue),
                ">" => IsGreaterThan(leftValueType, rightValueType, leftValue, rightValue),
                "<" => IsLessThan(leftValueType, rightValueType, leftValue, rightValue),
                "/" => DivideTypes(leftValueType, rightValueType, leftValue, rightValue),
                "*" => MultiplyTypes(leftValueType, rightValueType, leftValue, rightValue),
                "-" => SubtractTypes(leftValueType, rightValueType, leftValue, rightValue),
                "+" => AddTypes(leftValueType, rightValueType, leftValue, rightValue),
                _ => new Expr.LexemeTypeLiteral()
            };
        }
        private static object IsLessThan(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literal = new()
                {
                    literal = Convert.ToInt32(leftValue) < Convert.ToInt32(rightValue),
                    lexemeType = LexemeType.Types.BOOL
                };
                return literal;
            }

            if (leftValueType == LexemeType.Types.STRING || rightValueType == LexemeType.Types.STRING)
            {
                throw new ArgumentException("can't apply less than operator to strings");
            }

            throw new ArgumentException("Can't compare types");
        }

        private static object IsGreaterThan(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literal = new()
                {
                    literal = Convert.ToInt32(leftValue) > Convert.ToInt32(rightValue),
                    lexemeType = LexemeType.Types.BOOL
                };
                return literal;
            }

            if (leftValueType == LexemeType.Types.STRING || rightValueType == LexemeType.Types.STRING)
            {
                throw new ArgumentException("can't apply less than operator to strings");
            }

            throw new ArgumentException("Can't compare types");
        }

        private static object AddTypes(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literalSum = new()
                {
                    literal = Convert.ToInt32(leftValue) + Convert.ToInt32(rightValue),
                    lexemeType = LexemeType.Types.NUMBER
                };
                return literalSum;
            }

            if (leftValueType == LexemeType.Types.STRING && rightValueType == LexemeType.Types.STRING)
            {
                LexemeTypeLiteral literalStringSum = new()
                {
                    literal = Convert.ToString(leftValue) + Convert.ToString(rightValue),
                    lexemeType = LexemeType.Types.STRING
                };

                return literalStringSum;
            }

            throw new ArgumentNullException(nameof(leftValueType));
        }
        private static object SubtractTypes(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literalSum = new()
                {
                    literal = Convert.ToInt32(leftValue) - Convert.ToInt32(rightValue),
                    lexemeType = LexemeType.Types.NUMBER
                };
                return literalSum;
            }

            if (leftValueType == LexemeType.Types.STRING && rightValueType == LexemeType.Types.STRING)
            {
                throw new ArgumentException("Can't subtract strings");
            }

            throw new ArgumentNullException(nameof(leftValueType));
        }

        private static object MultiplyTypes(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literalProduction = new()
                {
                    literal = Convert.ToInt32(leftValue) * Convert.ToInt32(rightValue),
                    lexemeType = LexemeType.Types.NUMBER
                };
                return literalProduction;
            }

            if (leftValueType == LexemeType.Types.STRING && rightValueType == LexemeType.Types.STRING)
            {
                throw new ArgumentException("Can't multiply strings");
            }

            throw new ArgumentNullException(nameof(leftValueType));
        }
        private static object DivideTypes(LexemeType.Types leftValueType, LexemeType.Types rightValueType, object leftValue, object rightValue)
        {
            if (leftValueType == LexemeType.Types.NUMBER && rightValueType == LexemeType.Types.NUMBER)
            {
                LexemeTypeLiteral literalProduction = new();
                int divident = Convert.ToInt32(rightValue);

                if (divident == 0)
                {
                    throw new ArgumentException("Can't divide by zero");

                }
                literalProduction.literal = Convert.ToInt32(leftValue) / Convert.ToInt32(rightValue);
                literalProduction.lexemeType = LexemeType.Types.NUMBER;
                return literalProduction;
            }

            if (leftValueType == LexemeType.Types.STRING && rightValueType == LexemeType.Types.STRING)
            {
                throw new ArgumentException("can't divide strings");
            }

            throw new ArgumentNullException(nameof(leftValueType));
        }
        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            return a != null && a.Equals(b);
        }

#pragma warning disable CS8766
        public object? VisitCallExpr(Expr.Call expr)
#pragma warning restore CS8766
        {
            if (expr == null || expr.callee == null || expr.callee.ExpressionLexeme == null)
            {
                throw new("VisitCallExpr - runtime exception interperter");
            }

            switch (expr.callee)
            {
                case Get:
                    {
                        object instanceMethod = expr.callee.Accept(this);
                        Stmt.Function? declaration = ((OpeicoFunction)instanceMethod).Declaration;
                        if (declaration != null)
                        {
                            StackFrame instanceStackFrame = new(declaration.StmtLexemeName);
                            frames.Push(instanceStackFrame);
                            object? instanceMethodReturn = ((OpeicoFunction)instanceMethod).Call(declaration.StmtLexemeName, this, null!);
                            _ = frames.Pop();
                            return instanceMethodReturn;
                        }

                        break;
                    }
            }

            StackFrame currentStackFrame = new(expr.callee.ExpressionLexeme.ToString());
            frames.Push(currentStackFrame);

            List<object?> arguments = new();
            Dictionary<string, object?> argumentsMap = new();

            foreach (Expr argument in expr.arguments)
            {
                switch (argument)
                {
                    case Variable lexeme when lexeme.ExpressionLexeme != null:
                        {
                            object? variable = environment.Get(lexeme.ExpressionLexeme);
                            arguments.Add(variable);
                            argumentsMap.Add(lexeme.ExpressionLexeme.ToString(), variable);
                            break;
                        }
                    case Literal literal1:
                        {
                            Literal literal = literal1;
                            arguments.Add(literal.LiteralValue);
                            argumentsMap.Add(literal.ExpressionLexeme?.ToString()!, literal);
                            break;
                        }
                }
            }

            switch (argumentsMap.Count)
            {
                case > 0:
                    currentStackFrame.AddVariables(argumentsMap, this);
                    break;
            }

            switch (expr.isOpeicoCallable)
            {
                case true:
                    try
                    {
                        IOpeicoCallable? Opeicocall = (IOpeicoCallable)ServiceProvider.GetService(typeof(IOpeicoCallable))!;
                        return Opeicocall.Call(methodName: expr.callee.ExpressionLexeme.ToString(), this, arguments);
                    }
                    catch (SystemCallException? sce)
                    {
                        return sce;
                    }
            }

            object? callee = Evaluate(expr.callee);
            IOpeicoCallable? function = (IOpeicoCallable)callee!;
            return arguments.Count != function.Arity()
                ? throw new ArgumentException("Wrong number of arguments")
                : (function?.Call(expr.callee.ExpressionLexeme.ToString(), this, arguments));
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            StackVariableState? getexpr = Evaluate(expr.expr) as StackVariableState;
            if (getexpr?.Value is OpeicoInstance instance)
            {
                if (expr.ExpressionLexeme != null)
                {
                    return instance.Get(expr.ExpressionLexeme);
                }
            }

            throw new("not a class instances");
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return expr == null || expr.expression == null
                ? throw new ArgumentNullException(nameof(expr))
                : Evaluate(expr.expression) ?? throw new JRuntimeException("Grouping is null");
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.LiteralValue() ?? throw new JRuntimeException("Literal is null");
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            if (expr.ExpressionLexeme != null)
            {
                StackVariableState? stackVariableState = (StackVariableState)LookUpVariable(expr.ExpressionLexeme, expr)!;

                if (stackVariableState?.expressionContext is Literal context && expr.LexemeType == LexemeType.Types.PLUSPLUS)
                {
                    long unaryUpdate = Convert.ToInt64(context.ExpressionLexeme?.ToString()) + 1;

                    Lexeme lexeme = new(LexemeType.Types.NUMBER, 0, 0);
                    lexeme.AddToken(unaryUpdate.ToString());

                    Literal newLiteral = new(lexeme, lexeme.LexemeType);

                    stackVariableState.expressionContext = newLiteral;

                    LexemeTypeLiteral lexemeTypeLiteral = new()
                    {
                        LexemeType = lexeme.LexemeType,
                        ExpressionLexeme = lexeme,
                        literal = lexeme.Literal()
                    };

                    stackVariableState.Value = lexemeTypeLiteral;
                    return new Stmt.DefaultStatement();
                }
            }

            throw new JRuntimeException("Bad unary expression");
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (expr.ExpressionLexeme != null)
            {
                object? lookUp = LookUpVariable(expr.ExpressionLexeme, expr);
                return lookUp ?? throw new("Variable is null");
            }

            throw new JRuntimeException("Visit variable returned null");
        }

        public object VisitArrayExpr(Expr.ArrayDeclarationExpr expr)
        {
            _ = frames.Peek();
            //if (expr.initializerContextVariableName != null)
            //    currentFrame.AddStackArray(expr.initializerContextVariableName, expr.ArrayIndex);
            return expr.ArraySize;
        }

        public object VisitNewExpr(NewDeclarationExpr expr)
        {
            //throw new NotImplementedException("The new keyword has not been implemented. I need to have a model for allocating and dealocating objects. Potentially a garbage collection model.");
            return new DefaultExpr();
        }

        public object VisitArrayAccessExpr(ArrayAccessExpr expr)
        {
            StackVariableState? stackVariableState = LookUpVariable(expr.ArrayVariableName, expr) as StackVariableState;

            int arraySize = 0;
            if (stackVariableState?.expressionContext is ArrayDeclarationExpr)
            {
                arraySize = (int)(stackVariableState?.Value)!;
            }
            else if (stackVariableState?.expressionContext is NewDeclarationExpr context)
            {
                arraySize = ((ArrayDeclarationExpr)context.NewDeclarationExprInit).ArraySize;
            }

            if (expr.ArrayIndex > arraySize)
            {
                throw new JRuntimeException("JArray index out of bounds");
            }

            if (stackVariableState?.arrayValues == null)
            {
                stackVariableState!.arrayValues = new object[arraySize];
            }

            stackVariableState.arrayValues[expr.ArrayIndex] = expr.LvalueExpr;

            return new Stmt.DefaultStatement();
        }

        public object VisitDeleteExpr(DeleteDeclarationExpr expr)
        {

            StackFrame currentFrame = frames.Peek();
            _ = currentFrame.DeleteVariable(expr.variable.ExpressionLexemeName);

            Console.WriteLine(currentFrame);

            return new Stmt.DefaultStatement();
        }

        internal object? LookUpVariable(Lexeme name, Expr expr)
        {
            _ = locals.TryGetValue(expr, out int? distance);

            if (frames.Peek().TryGetStackVariableByName(name.ToString(), out StackVariableState? variable))
            {
                return variable;
            }

            return distance != null ? environment.GetAt(distance.Value, name.ToString()) : globals.Get(name);
        }
        internal ServiceProvider ServiceProvider { get; }

        internal void Resolve(Expr expr, int depth)
        {
            if (locals.Where(f => f.Key.ExpressionLexeme != null && expr.ExpressionLexeme != null && f.Key.ExpressionLexeme.ToString().Equals(expr.ExpressionLexeme.ToString())).Count() <= 1)
            {
                locals.Add(expr, depth);
            }
        }

        private bool IsTrue(object? o)
        {
            switch (o)
            {
                case null:
                    return false;
                case bool b:
                    return b;
                case LexemeTypeLiteral literal:
                    {
                        object boolValue = literal.Literal;
                        if (boolValue is bool)
                        {
                            return IsTrue(boolValue);
                        }

                        break;
                    }
            }

            return true;
        }
    }
}

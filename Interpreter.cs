namespace LoxLanguage;

using System.Collections;
using static TokenType;

class Interpreter : Expr.Visitor<Object?>, Stmt.Visitor<Object?>
{
    public readonly Environment globals = new Environment();
    private Environment environment;
    readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

    class clockClass : LoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public Object Call(Interpreter interpreter, ArrayList arguments)
        {
            return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }
    };

    public Interpreter()
    {
        this.environment = globals;

        globals.Define("clock", new clockClass());
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Program.RuntimeError(error);
        }
    }

    public Object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public Object? VisitLogicalExpr(Expr.Logical expr)
    {
        Object? left = Evaluate(expr.Left);

        if (expr?.Op?.Type == TokenType.OR)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr?.Right);
    }

    public Object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public Object? VisitUnaryExpr(Expr.Unary expr)
    {
        Object? right = Evaluate(expr.Right);

        switch (expr?.Op?.Type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(expr.Op, right);
                return -(double)right;
        }

        return null;
    }

    public Object? VisitVariableExpr(Expr.Variable? expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    Object? LookUpVariable(Token name, Expr expr)
    {
        if (locals.TryGetValue(expr, out int distance))
        {
            return environment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return globals.Get(name);
        }
    }

    private void CheckNumberOperand(Token op, Object? operand)
    {
        if (operand is Double)
            return;

        throw new RuntimeError(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, Object? left, Object? right)
    {
        if (right is Double && right is Double)
            return;

        throw new RuntimeError(op, "Operands must be numbers.");
    }

    private bool IsTruthy(Object? obj)
    {
        if (obj == null)
            return false;
        if (obj is bool)
            return (bool)obj;

        return true;
    }

    private bool IsEqual(Object? a, Object? b)
    {
        if (a == null && b == null)
            return true;
        if (a == null)
            return false;

        return a.Equals(b);
    }

    private String? Stringify(Object? obj)
    {
        if (obj == null)
            return "nil";

        if (obj is Double)
        {
            String? text = obj.ToString();
            if (text != null && text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return obj.ToString() ?? "";
    }

    private Object? Evaluate(Expr? expr)
    {
        return expr?.Accept(this);
    }

    private void Execute(Stmt? stmt)
    {
        stmt?.Accept(this);
    }

    public void Resolve(Expr expr, int depth)
    {
        locals.Add(expr, depth);
    }

    public void ExecuteBlock(ArrayList statements, Environment environment)
    {
        Environment previous = this.environment;

        try
        {
            this.environment = environment;

            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    public Object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(environment));
        return null;
    }

    public Object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public Object? VisitFunctionStmt(Stmt.Function stmt)
    {
        LoxFunction function = new LoxFunction(stmt, environment);
        environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public Object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public Object? VisitPrintStmt(Stmt.Print stmt)
    {
        Object? value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public Object? VisitReturnStmt(Stmt.Return stmt)
    {
        Object? value = null;
        if (stmt.Value != null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    public Object? VisitVarStmt(Stmt.Var stmt)
    {
        Object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        if (stmt?.Name?.Lexeme != null)
            environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public Object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }

    public Object? VisitAssignExpr(Expr.Assign expr)
    {
        Object? value = Evaluate(expr.Value);
        if (locals.TryGetValue(expr, out int distance))
        {
            environment.AssignAt(distance, expr.Name, value);
        } 
        else
        {
            environment.Assign(expr.Name, value);
        }
        return value;
    }

    public Object? VisitBinaryExpr(Expr.Binary expr)
    {
        Object? left = Evaluate(expr.Left);
        Object? right = Evaluate(expr.Right);

        switch (expr?.Op?.Type)
        {
            case GREATER:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left <= (double)right;
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
            case MINUS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left - (double)right;
            case PLUS:
                if (left is Double && right is Double)
                {
                    return (double)left + (double)right;
                }

                if (left is String && right is String)
                {
                    return (String)left + (String)right;
                }

                throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
            case SLASH:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left * (double)right;
        }

        return null;
    }

    public Object? VisitCallExpr(Expr.Call expr)
    {
        Object? callee = Evaluate(expr.Callee);

        ArrayList arguments = new ArrayList();
        foreach (Expr argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (!(callee is LoxCallable))
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }

        LoxCallable? function = (LoxCallable?)callee;
        if (arguments.Count != function?.Arity())
        {
            throw new RuntimeError(
                expr.Paren,
                "Expected " + function?.Arity() + " arguments but got " + arguments.Count + "."
            );
        }
        return function?.Call(this, arguments);
    }
}

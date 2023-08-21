namespace LoxLanguage;

using System.Collections;
using static TokenType;

class Interpreter : Expr.Visitor<Object?>, Stmt.Visitor<Object?>
{
    private Environment environment = new Environment();

    public void Interpret(ArrayList statements)
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
        return expr.value;
    }

    public Object? VisitLogicalExpr(Expr.Logical expr)
    {
        Object? left = Evaluate(expr.left);

        if (expr?.op?.type == TokenType.OR)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr?.right);
    }

    public Object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression);
    }

    public Object? VisitUnaryExpr(Expr.Unary expr)
    {
        Object? right = Evaluate(expr.right);

        switch (expr?.op?.type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(expr.op, right);
                return -(double)right;
        }

        return null;
    }

    public Object VisitVariableExpr(Expr.Variable? expr)
    {
        return environment.Get(expr?.name);
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
        ExecuteBlock(stmt.statements, new Environment(environment));
        return null;
    }

    public Object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public Object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch != null)
        {
            Execute(stmt.elseBranch);
        }

        return null;
    }

    public Object? VisitPrintStmt(Stmt.Print stmt)
    {
        Object? value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public Object? VisitVarStmt(Stmt.Var stmt)
    {
        Object? value = null;
        if (stmt.initializer != null)
        {
            value = Evaluate(stmt.initializer);
        }

        environment.Define(stmt.name.lexeme, value);
        return null;
    }

    public Object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.body);
        }

        return null;
    }

    public Object? VisitAssignExpr(Expr.Assign expr)
    {
        Object? value = Evaluate(expr.value);
        environment.Assign(expr.name, value);
        return value;
    }

    public Object? VisitBinaryExpr(Expr.Binary expr)
    {
        Object? left = Evaluate(expr.left);
        Object? right = Evaluate(expr.right);

        switch (expr?.op?.type)
        {
            case GREATER:
                CheckNumberOperands(expr.op, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left <= (double)right;
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
            case MINUS:
                CheckNumberOperands(expr.op, left, right);
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

                throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
            case SLASH:
                CheckNumberOperands(expr.op, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.op, left, right);
                return (double)left * (double)right;
        }

        return null;
    }
}

using LoxLanguage;
using System.Collections;

class Resolver : Expr.Visitor<Object?>, Stmt.Visitor<Object?> {
    readonly Interpreter interpreter;    
    readonly Stack<Dictionary<String, Boolean>> scopes = new Stack<Dictionary<String, Boolean>>();
    FunctionType currentFunction = FunctionType.NONE;
    
    public Resolver(Interpreter interpreter) 
    {
        this.interpreter = interpreter;
    }

    enum FunctionType {
        NONE,
        FUNCTION,   
    }

    public void Resolve(ArrayList statements)
    {
        foreach (Stmt statement in statements)
        {
            Resolve(statement);
        }
    }

    void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope();
        foreach (Token param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    public Object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public Object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public Object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public Object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null)
            Resolve(stmt.ElseBranch);
        return null;
    }

    public Object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public Object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            Program.Error(stmt.Keyword, "Can't return from top-level code.");
        }

        if (stmt.Value != null)
            Resolve(stmt.Value);
        return null;
    }

    public Object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public Object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    public Object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public Object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public Object? VisitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Count > 0)
        {
            Boolean has = true;
            scopes.Peek().TryGetValue(expr.Name.Lexeme, out has);
            if (has != false)
                Program.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public Object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (Expr argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public Object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public Object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public Object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public Object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    void BeginScope()
    {
        scopes.Push(new Dictionary<String, Boolean>());
    }

    void EndScope()
    {
        scopes.Pop();
    }

    void Declare(Token name)
    {
        if (scopes.Count == 0)
            return;

        Dictionary<String, Boolean> scope = scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            Program.Error(name, "Already a variable with this name in this scope.");
        }
        scope.Add(name.Lexeme, false);
    }

    void Define(Token name)
    {
        if (scopes.Count == 0)
            return;

        scopes.Peek().TryAdd(name.Lexeme, true);
    }

    void ResolveLocal(Expr expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }
}

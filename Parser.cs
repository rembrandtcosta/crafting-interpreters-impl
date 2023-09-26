using System.Collections;
using static LoxLanguage.TokenType;

namespace LoxLanguage;

class Parser
{
    private class ParseError : SystemException { }

    private List<Token> tokens;
    private int current = 0;
    private bool PromptMode = false;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Parser(List<Token> tokens, bool PromptMode)
    {
        this.tokens = tokens;
        this.PromptMode = PromptMode;
    }

    public List<Stmt> Parse()
    {
        List<Stmt> statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Expr? Expression()
    {
        return Assignment();
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(FUN))
            {
                return Function("function");
            }
            if (Match(VAR))
                return VarDeclaration();

            return Statement();
        }
        catch
        {
            Synchronize();
            return null;
        }
    }

    private Stmt Statement()
    {
        if (Match(FOR))
            return ForStatement();
        if (Match(IF))
            return IfStatement();
        if (Match(PRINT))
            return PrintStatement();
        if (Match(RETURN))
            return ReturnStatement();
        if (Match(WHILE))
            return WhileStatement();
        if (Match(LEFT_BRACE))
            return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer = null;
        if (Match(SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }
        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

        Stmt body = Statement();

        if (increment != null)
        {
            List<Stmt> a = new List<Stmt> { body, new Stmt.Expression(increment), };
            body = new Stmt.Block(a);
        }

        if (condition == null)
        {
            condition = new Expr.Literal(true);
        }
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(new List<Stmt> { initializer, body });
        }

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expr? condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        Expr? value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;
        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt VarDeclaration()
    {
        Token? name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        Expr? condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt ExpressionStatement()
    {
        Expr? expr = Expression();
        if (!PromptMode)
            Consume(SEMICOLON, "Expect ';' after expression.");
        else
        {
            if (Check(SEMICOLON))
            {
                Advance();
                return new Stmt.Expression(expr);
            }
            return new Stmt.Print(expr);
        }
        return new Stmt.Expression(expr);
    }

    private Stmt.Function Function(String kind)
    {
        Token name = Consume(IDENTIFIER, "Expect " + kind + " name.");
        Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
        List<Token> parameters = new List<Token>();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }
        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Stmt> body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new List<Stmt>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expr? Assignment()
    {
        Expr? expr = Or();

        if (Match(EQUAL))
        {
            Token equals = Previous();
            Expr? value = Assignment();

            if (expr is Expr.Variable)
            {
                Token? name = ((Expr.Variable)expr).Name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr? Or()
    {
        Expr? expr = And();

        while (Match(OR))
        {
            Token op = Previous();
            Expr? right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr? And()
    {
        Expr? expr = Equality();

        while (Match(AND))
        {
            Token op = Previous();
            Expr? right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr? Equality()
    {
        Expr? expr = Comparasion();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token op = Previous();
            Expr? right = Comparasion();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr? Comparasion()
    {
        Expr? expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token op = Previous();
            Expr? right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr? Term()
    {
        Expr? expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token op = Previous();
            Expr? right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr? Factor()
    {
        Expr? expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token op = Previous();
            Expr? right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr? Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token op = Previous();
            Expr? right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    private Expr FinishCall(Expr? callee)
    {
        ArrayList arguments = new ArrayList();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token? paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Expr? Call()
    {
        Expr? expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr? Primary()
    {
        if (Match(FALSE))
            return new Expr.Literal(false);
        if (Match(TRUE))
            return new Expr.Literal(true);
        if (Match(NIL))
            return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            Expr? expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenType type, String message)
    {
        if (Check(type))
            return Advance();

        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
            return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
            current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }

    private ParseError Error(Token token, String message)
    {
        Program.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous()?.Type == SEMICOLON)
                return;

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }
}

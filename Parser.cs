using static LoxLanguage.TokenType;

namespace LoxLanguage;

class Parser
{
    private class ParseError : SystemException { }

    private List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError error)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        Expr expr = Comparasion();

        while (Match(BANG_EQUAL, BANG_EQUAL))
        {
            Token op = Previous();
            Expr right = Comparasion();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparasion()
    {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(FALSE))
            return new Expr.Literal(false);
        if (Match(TRUE))
            return new Expr.Literal(true);
        if (Match(NIL))
            return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
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
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
            current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().type == EOF;
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
            if (Previous().type == SEMICOLON)
                return;

            switch (Peek().type)
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

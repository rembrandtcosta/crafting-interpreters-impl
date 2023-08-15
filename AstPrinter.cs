using static LoxLanguage.TokenType;

namespace LoxLanguage;

public class AstPrinter : Expr.Visitor<String>
{
    public void Test()
    {
        Expr expression = new Expr.Binary(
            new Expr.Unary(new Token(MINUS, "-", null, 1), new Expr.Literal(123)),
            new Token(STAR, "*", null, 1),
            new Expr.Grouping(new Expr.Literal(45.67))
        );

        Console.WriteLine(new AstPrinter().Print(expression));
    }

    public String Print(Expr? expr)
    {
        String? s = expr?.Accept(this);
        return s ?? "";
    }

    public String VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.op.lexeme, expr.left, expr.right);
    }

    public String VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.expression);
    }

    public String VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.op.lexeme, expr.right);
    }

    public String VisitLiteralExpr(Expr.Literal expr)
    {
        if (expr.value == null)
            return "nil";
        return expr.value.ToString() ?? "";
    }

    private String Parenthesize(String name, params Expr[] exprs)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        builder.Append("(").Append(name);

        foreach (Expr expr in exprs)
        {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");

        return builder.ToString();
    }
}

namespace LoxLanguage;

public class Token
{
    public TokenType Type;
    public String Lexeme;
    public Object? Literal;
    public int Line;

    public Token(TokenType Type, String Lexeme, Object? Literal, int Line)
    {
        this.Type = Type;
        this.Lexeme = Lexeme;
        this.Literal = Literal;
        this.Line = Line;
    }

    public override String ToString()
    {
        return Type + " " + Lexeme + " " + Literal;
    }
}

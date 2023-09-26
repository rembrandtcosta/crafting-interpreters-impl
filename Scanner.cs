using System.Collections;
using static LoxLanguage.TokenType;

namespace LoxLanguage;

class Scanner
{
    private String source;
    private List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 1;
    private Dictionary<String, TokenType> keywords;

    public Scanner(String source)
    {
        this.source = source;
        keywords = new Dictionary<string, TokenType>();
        keywords.Add("and", AND);
        keywords.Add("break", BREAK);
        keywords.Add("class", CLASS);
        keywords.Add("else", ELSE);
        keywords.Add("false", FALSE);
        keywords.Add("for", FOR);
        keywords.Add("fun", FUN);
        keywords.Add("if", IF);
        keywords.Add("nil", NIL);
        keywords.Add("or", OR);
        keywords.Add("print", PRINT);
        keywords.Add("return", RETURN);
        keywords.Add("super", SUPER);
        keywords.Add("this", THIS);
        keywords.Add("true", TRUE);
        keywords.Add("var", VAR);
        keywords.Add("while", WHILE);
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));
        return tokens;
    }

    private bool IsAtEnd()
    {
        return current >= source.Length;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(':
                AddToken(LEFT_PAREN);
                break;
            case ')':
                AddToken(RIGHT_PAREN);
                break;
            case '{':
                AddToken(LEFT_BRACE);
                break;
            case '}':
                AddToken(RIGHT_BRACE);
                break;
            case ',':
                AddToken(COMMA);
                break;
            case '.':
                AddToken(DOT);
                break;
            case '-':
                AddToken(MINUS);
                break;
            case '+':
                AddToken(PLUS);
                break;
            case ';':
                AddToken(SEMICOLON);
                break;
            case '*':
                AddToken(STAR);
                break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;
            case '"':
                String();
                break;
            case ' ':
            case '\r':
            case '\t':
            case '\n':
                line++;
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Program.Error(line, "Unexpected character.");
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();
        String text = source.Substring(start, current - start);
        TokenType type = IDENTIFIER;
        if (keywords.TryGetValue(text, out type))
        {
            AddToken(type);
        }
        else
        {
            AddToken(IDENTIFIER);
        }
    }

    private void Number()
    {
        while (IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();

            while (IsDigit(Peek()))
                Advance();
        }

        AddToken(NUMBER, Double.Parse(source.Substring(start, current - start)));
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Program.Error(line, "Unterminated string.");
            return;
        }

        Advance();

        String value = source.Substring(start + 1, current - start - 2);
        AddToken(STRING, value);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
            return false;
        if (source[current] != expected)
            return false;

        current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd())
            return '\0';
        return source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source.Length)
            return '\0';

        return source[current + 1];
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A') && (c <= 'Z') || (c == '_');
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private char Advance()
    {
        return source[current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, Object? literal)
    {
        String text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}

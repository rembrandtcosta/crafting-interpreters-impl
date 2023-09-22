namespace LoxLanguage;

class RuntimeError : System.Exception
{
    public readonly Token? Token;

    public RuntimeError(Token? Token, String message)
        : base(message)
    {
        this.Token = Token;
    }
}

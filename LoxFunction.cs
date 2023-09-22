using LoxLanguage;
using System.Collections;

class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly LoxLanguage.Environment closure;

    public LoxFunction(Stmt.Function declaration, LoxLanguage.Environment closure)
    {
        this.closure = closure;
        this.declaration = declaration;
    }

    public override String ToString()
    {
        return "<fn " + declaration.Name.Lexeme + ">";
    }

    public int Arity()
    {
        return declaration.Parameters.Count;
    }

    public Object? Call(Interpreter interpreter, ArrayList arguments)
    {
        LoxLanguage.Environment environment = new LoxLanguage.Environment(this.closure);
        for (int i = 0; i < declaration.Parameters.Count; i++)
        {
            Token tk = (Token)declaration.Parameters[i];
            environment.Define(tk.Lexeme, arguments[i]);
        }

        try 
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        } catch (Return returnValue)
        {
            return returnValue.Value;
        }
        return null;
    }
}

namespace LoxLanguage;

class Environment
{
    readonly Environment? enclosing;
    private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

    public Environment()
    {
        enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    public Object Get(Token name)
    {
        if (values.TryGetValue(name.lexeme, out Object? obj))
        {
            return obj;
        }

        if (enclosing != null)
            return enclosing.Get(name);

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }

    public Object? Assign(Token name, Object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return null;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return null;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }

    public void Define(String name, Object obj)
    {
        values.Add(name, obj);
    }
}

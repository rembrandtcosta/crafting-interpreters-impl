namespace LoxLanguage;

class Environment
{
    readonly Environment? Enclosing;
    private readonly Dictionary<String, Object?> Values = new Dictionary<String, Object?>();

    public Environment()
    {
        Enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.Enclosing = enclosing;
    }

    public Object? Get(Token? name)
    {
        if (name == null)
        {
            throw new RuntimeError(null, "Undefined variable '" + name?.Lexeme + "'.");
        }

        if (this.Values.TryGetValue(name.Lexeme, out Object? obj))
        {
            return obj;
        }

        if (this.Enclosing != null)
            return Enclosing.Get(name);

        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public Object? Assign(Token? name, Object? value)
    {
        if (name != null && this.Values.ContainsKey(name.Lexeme))
        {
            this.Values[name.Lexeme] = value;
            return null;
        }

        if (this.Enclosing != null)
        {
            this.Enclosing.Assign(name, value);
            return null;
        }

        throw new RuntimeError(name, "Undefined variable '" + name?.Lexeme + "'.");
    }

    public void Define(String name, Object? obj)
    {
        this.Values.Add(name, obj);
    }

    Environment? Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.Enclosing;
        }

        return environment;
    }

    public Object? GetAt(int distance, String name)
    {
        Object? ret;
        Ancestor(distance).Values.TryGetValue(name, out ret);
        return ret;
    }

    public void AssignAt(int distance, Token name, Object value)
    {
        Ancestor(distance).Values.Add(name.Lexeme, value);
    }
}

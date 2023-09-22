namespace LoxLanguage;

class Return : System.Exception {
    public readonly Object? Value;

    public Return(Object? Value) : base()
    {
        this.Value = Value;
    }
}

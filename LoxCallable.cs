using System.Collections;

namespace LoxLanguage;

interface LoxCallable
{
    int Arity();
    Object? Call(Interpreter interpreter, ArrayList arguments);
}

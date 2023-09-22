// See https://aka.ms/new-console-template for more information

using System.Collections;
using static LoxLanguage.TokenType;

namespace LoxLanguage;

static class Program
{
    private static readonly Interpreter interpreter = new Interpreter();
    static bool hadError = false;
    static bool hadRuntimeError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: jlox [script]");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(String path)
    {
        byte[] bytes = File.ReadAllBytes(Path.GetFullPath(path));
        Run(System.Text.Encoding.Default.GetString(bytes), false);
        if (hadError)
        {
            System.Environment.Exit(65);
        }
        if (hadRuntimeError)
        {
            System.Environment.Exit(70);
        }
    }

    private static void RunPrompt()
    {
        do
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null)
            {
                hadError = false;
                break;
            }
            Run(line, true);
            hadError = false;
        } while (!ShouldEscape());
    }

    private static bool ShouldEscape()
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        return key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control;
    }

    private static void Run(String source, bool promptMode)
    {
        Scanner scanner = new Scanner(source);
        ArrayList tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens.Cast<Token>().ToList(), promptMode);
        ArrayList statements = parser.Parse();

        if (hadError)
            return;

        interpreter.Interpret(statements);
    }

    public static void Error(int line, String message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, String message)
    {
        if (token.Type == EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine(error.GetBaseException().Message + "\n[line " + error?.Token?.Line + "]");
        hadRuntimeError = true;
    }

    private static void Report(int line, String where, String message)
    {
        Console.Error.WriteLine("[line " + line + "] " + where + ": " + message);
        hadError = true;
    }
}

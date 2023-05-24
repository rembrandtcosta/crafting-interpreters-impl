// See https://aka.ms/new-console-template for more information

using System.Collections;

namespace LoxLanguage;

class Program {
  static bool hadError = false;

  static void Main(string[] args) {
    if (args.Length > 1) {
      Console.WriteLine("Usage: jlox [script]");
      Environment.Exit(64);
    } else if (args.Length == 1) {
      if (args[0].Equals("printer")) {
        AstPrinter printer = new AstPrinter();
        printer.Test();
      } else {
        RunFile(args[0]);
      }
    } else {
      RunPrompt();
    }
  }

  private static void RunFile(String path) {
    byte[] bytes = File.ReadAllBytes(Path.GetFullPath(path));
    Run(System.Text.Encoding.Default.GetString(bytes));
    if (hadError) {
      Environment.Exit(65);
    }
  }

  private static void RunPrompt() {
    do { 
      Console.Write("> ");
      string? line = Console.ReadLine();
      if (line == null) {
        hadError = false;
        break;
      }
      Run(line);
      hadError = false;
    } while (!ShouldEscape());
  }

  private static bool ShouldEscape() {
    ConsoleKeyInfo key = Console.ReadKey(true);
    return key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control;
  }

  private static void Run(String source) {
    Scanner scanner = new Scanner(source);
    ArrayList tokens = scanner.ScanTokens();
    Parser parser = new Parser(tokens.Cast<Token>().ToList());
    Expr expression = parser.Parse();

    if (hadError) return;

    Console.WriteLine(new AstPrinter().Print(expression));
  }

  public static void Error(int line, String message) {
    Report(line, "", message);
  }

  public static void Error(Token token, String message) {
    if (token.type == TokenType.EOF) {
      Report(token.line, " at end", message);
    } else {
      Report(token.line, " at '" + token.lexeme + "'", message);
    }
  }

  private static void Report(int line, String where, String message) {
    Console.Error.WriteLine("[line " + line + "] " + where + ": " + message);
    hadError = true;
  }
}

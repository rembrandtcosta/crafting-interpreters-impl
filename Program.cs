// See https://aka.ms/new-console-template for more information

using System;
using System.Collections;

namespace LoxLanguage {
  class Program {
    static bool hadError = false;

    static void Main(string[] args) {
      if (args.Length > 1) {
        Console.WriteLine("Usage: jlox [script]");
        Environment.Exit(64);
      } else if (args.Length == 1) {
        RunFile(args[0]);
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

      foreach (Token token in tokens) {
        Console.WriteLine(token.ToString());
      }
    }

    public static void Error(int line, String message) {
      Report(line, "", message);
    }

    private static void Report(int line, String where, String message) {
      Console.Error.WriteLine("[line " + line + "] " + where + ": " + message);
      hadError = true;
    }
  }
}

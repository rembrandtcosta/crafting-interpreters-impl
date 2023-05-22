using System.Collections;

class GenerateAst {

  static void main(String[] args) {
    if (args.Length != 1) {
      Console.Error.WriteLine("Usage: generate_ast <output directory>");
      Environment.Exit(64);
    }
    String outputDir = args[0];
    String[] a = new String[]{
      "Binary   : Expr left, Token operator, Expr right",
      "Grouping : Expr expression",
      "Literal  : Object value",
      "Unary    : Token operator, Expr right"
    };
    DefineAst(outputDir, "Expr", a.ToList());
  }

  static void DefineAst(String outputDir, String baseName, List<String> types) {
    String path = outputDir + "/" + baseName + ".cs";
    MemoryStream mStrm = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(path));
    TextWriter writer = new StreamWriter(mStrm, System.Text.Encoding.UTF8);
    
    writer.WriteLine("abstract class " + baseName + " {");
    foreach (String type in types) {
      String className = type.Split(":")[0].Trim();
      String fields = type.Split(":")[1].Trim();
      DefineType(writer, baseName, className, fields);
    }
    writer.WriteLine("}");
    writer.Close();
  }

  private static void DefineType(TextWriter writer, String baseName, String className, String fieldList) {
    writer.WriteLine(" static class " + className + ": " + baseName + "{");

    // Constructor.
    writer.WriteLine("    " + className + "(" + fieldList + ") {");

    String[] fields = fieldList.Split(", ");
    foreach (String field in fields) {
      String name = field.Split(" ")[1];
      writer.WriteLine("    this." + name + " = " + name + ";");
    }

    writer.WriteLine("    }");

    writer.WriteLine();
    foreach (String field in fields) {
      writer.WriteLine("private " + field + ";");
    }

    writer.WriteLine("  }");
  }


}

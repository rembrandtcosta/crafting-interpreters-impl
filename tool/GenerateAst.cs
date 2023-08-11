using System.Collections;

class GenerateAst
{
    static void Main0(String[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }
        String outputDir = args[0];
        String[] a = new String[]
        {
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Unary    : Token op, Expr right"
        };
        DefineAst(outputDir, "Expr", a.ToList());
    }

    static void DefineAst(String outputDir, String baseName, List<String> types)
    {
        String path = outputDir + "/" + baseName + ".cs";
        FileStream mStrm = new FileStream(path, FileMode.OpenOrCreate);
        TextWriter writer = new StreamWriter(mStrm, System.Text.Encoding.UTF8);

        writer.WriteLine("namespace LoxLanguage {");
        writer.WriteLine("  public abstract class " + baseName + " {");
        DefineVisitor(writer, baseName, types);
        foreach (String type in types)
        {
            String className = type.Split(":")[0].Trim();
            String fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        writer.WriteLine();
        writer.WriteLine("    public abstract R Accept<R>(Visitor<R> visitor);");
        writer.WriteLine("  }");
        writer.WriteLine("}");
        writer.Close();
    }

    private static void DefineVisitor(TextWriter writer, String baseName, List<String> types)
    {
        writer.WriteLine("    public interface Visitor<R> {");

        foreach (String type in types)
        {
            String typeName = type.Split(":")[0].Trim();
            writer.WriteLine(
                "      public R Visit"
                    + typeName
                    + baseName
                    + "("
                    + typeName
                    + " "
                    + baseName.ToLower()
                    + ");"
            );
        }

        writer.WriteLine("  }");
    }

    private static void DefineType(
        TextWriter writer,
        String baseName,
        String className,
        String fieldList
    )
    {
        writer.WriteLine("    public class " + className + ": " + baseName + "{");

        // Constructor.
        writer.WriteLine("    " + className + "(" + fieldList + ") {");

        String[] fields = fieldList.Split(", ");
        foreach (String field in fields)
        {
            String name = field.Split(" ")[1];
            writer.WriteLine("      this." + name + " = " + name + ";");
        }

        writer.WriteLine("    }");

        writer.WriteLine();
        writer.WriteLine("      public override R Accept<R>(Visitor <R> visitor) {");
        writer.WriteLine("        return visitor.Visit" + className + baseName + "(this);");
        writer.WriteLine("    }");

        writer.WriteLine();
        foreach (String field in fields)
        {
            writer.WriteLine("    public " + field + ";");
        }

        writer.WriteLine("  }");
    }
}

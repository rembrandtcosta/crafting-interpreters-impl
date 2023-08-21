using System.Collections;

namespace LoxLanguage
{
    public abstract class Stmt
    {
        public interface Visitor<R>
        {
            public R VisitBlockStmt(Block stmt);
            public R VisitExpressionStmt(Expression stmt);
            public R VisitIfStmt(If stmt);
            public R VisitPrintStmt(Print stmt);
            public R VisitVarStmt(Var stmt);
            public R VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public Block(ArrayList statements)
            {
                this.statements = statements;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public ArrayList statements;
        }

        public class Expression : Stmt
        {
            public Expression(Expr? expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public Expr? expression;
        }

        public class If : Stmt
        {
            public If(Expr? condition, Stmt? thenBranch, Stmt? elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public Expr? condition;
            public Stmt? thenBranch;
            public Stmt? elseBranch;
        }

        public class Print : Stmt
        {
            public Print(Expr? expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public Expr? expression;
        }

        public class Var : Stmt
        {
            public Var(Token? name, Expr? initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public Token? name;
            public Expr? initializer;
        }

        public class While : Stmt
        {
            public While(Expr? condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public Expr? condition;
            public Stmt body;
        }

        public abstract R Accept<R>(Visitor<R> visitor);
    }
}

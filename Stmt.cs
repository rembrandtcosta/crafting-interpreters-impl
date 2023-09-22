using System.Collections;

namespace LoxLanguage
{
    public abstract class Stmt
    {
        public interface Visitor<R>
        {
            public R VisitBlockStmt(Block stmt);
            public R VisitExpressionStmt(Expression stmt);
            public R VisitFunctionStmt(Function stmt);
            public R VisitIfStmt(If stmt);
            public R VisitPrintStmt(Print stmt);
            public R VisitReturnStmt(Return stmt);
            public R VisitVarStmt(Var stmt);
            public R VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public Block(ArrayList Statements)
            {
                this.Statements = Statements;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public ArrayList Statements;
        }

        public class Expression : Stmt
        {
            public Expression(Expr? Expr)
            {
                this.Expr = Expr;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public Expr? Expr;
        }

        public class Function : Stmt
        {
            public Function(Token Name, ArrayList Parameters, ArrayList Body)
            {
                this.Name = Name;
                this.Parameters = Parameters;
                this.Body = Body;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public Token Name;
            public ArrayList Parameters;
            public ArrayList Body;
        }

        public class If : Stmt
        {
            public If(Expr? Condition, Stmt? ThenBranch, Stmt? ElseBranch)
            {
                this.Condition = Condition;
                this.ThenBranch = ThenBranch;
                this.ElseBranch = ElseBranch;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public Expr? Condition;
            public Stmt? ThenBranch;
            public Stmt? ElseBranch;
        }

        public class Print : Stmt
        {
            public Print(Expr? Expr)
            {
                this.Expr= Expr;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public Expr? Expr;
        }

        public class Return : Stmt
        {
            public Return(Token Keyword, Expr? Value)
            {
                this.Keyword = Keyword;
                this.Value = Value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public Token Keyword;
            public Expr? Value;
        }

        public class Var : Stmt
        {
            public Var(Token? Name, Expr? Initializer)
            {
                this.Name = Name;
                this.Initializer = Initializer;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public Token? Name;
            public Expr? Initializer;
        }

        public class While : Stmt
        {
            public While(Expr? Condition, Stmt? Body)
            {
                this.Condition = Condition;
                this.Body = Body;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public Expr? Condition;
            public Stmt? Body;
        }

        public abstract R Accept<R>(Visitor<R> visitor);
    }
}

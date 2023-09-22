using System.Collections;

namespace LoxLanguage
{
    public abstract class Expr
    {
        public interface Visitor<R>
        {
            public R VisitAssignExpr(Assign expr);
            public R VisitBinaryExpr(Binary expr);
            public R VisitCallExpr(Call expr);
            public R VisitGroupingExpr(Grouping expr);
            public R VisitLiteralExpr(Literal expr);
            public R VisitLogicalExpr(Logical expr);
            public R VisitUnaryExpr(Unary expr);
            public R VisitVariableExpr(Variable expr);
        }

        public class Assign : Expr
        {
            public Assign(Token? Name, Expr? Value)
            {
                this.Name = Name;
                this.Value = Value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public Token? Name;
            public Expr? Value;
        }

        public class Binary : Expr
        {
            public Binary(Expr? Left, Token? Op, Expr? Right)
            {
                this.Left = Left;
                this.Op = Op;
                this.Right = Right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr? Left;
            public Token? Op;
            public Expr? Right;
        }

        public class Call : Expr
        {
            public Call(Expr Callee, Token Paren, ArrayList Arguments)
            {
                this.Callee = Callee;
                this.Paren = Paren;
                this.Arguments = Arguments;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public Expr Callee;
            public Token Paren;
            public ArrayList Arguments;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr? Expression)
            {
                this.Expression = Expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr? Expression;
        }

        public class Literal : Expr
        {
            public Literal(Object? Value)
            {
                this.Value = Value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public Object? Value;
        }

        public class Logical : Expr
        {
            public Logical(Expr? Left, Token? Op, Expr? Right)
            {
                this.Left = Left;
                this.Op = Op;
                this.Right = Right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public Expr? Left;
            public Token? Op;
            public Expr? Right;
        }

        public class Unary : Expr
        {
            public Unary(Token? Op, Expr? Right)
            {
                this.Op = Op;
                this.Right = Right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token? Op;
            public Expr? Right;
        }

        public class Variable : Expr
        {
            public Variable(Token? Name)
            {
                this.Name = Name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public Token? Name;
        }

        public abstract R Accept<R>(Visitor<R> visitor);
    }
}

namespace LoxLanguage
{
    public abstract class Expr
    {
        public interface Visitor<R>
        {
            public R VisitAssignExpr(Assign expr);
            public R VisitBinaryExpr(Binary expr);
            public R VisitGroupingExpr(Grouping expr);
            public R VisitLiteralExpr(Literal expr);
            public R VisitLogicalExpr(Logical expr);
            public R VisitUnaryExpr(Unary expr);
            public R VisitVariableExpr(Variable? expr);
        }

        public class Assign : Expr
        {
            public Assign(Token? name, Expr? value)
            {
                this.name = name;
                this.value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public Token? name;
            public Expr? value;
        }

        public class Binary : Expr
        {
            public Binary(Expr? left, Token? op, Expr? right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr? left;
            public Token? op;
            public Expr? right;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr? expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr? expression;
        }

        public class Literal : Expr
        {
            public Literal(Object? value)
            {
                this.value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public Object? value;
        }

        public class Logical : Expr
        {
            public Logical(Expr? left, Token? op, Expr? right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public Expr? left;
            public Token? op;
            public Expr? right;
        }

        public class Unary : Expr
        {
            public Unary(Token? op, Expr? right)
            {
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token? op;
            public Expr? right;
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public Token name;
        }

        public abstract R Accept<R>(Visitor<R> visitor);
    }
}

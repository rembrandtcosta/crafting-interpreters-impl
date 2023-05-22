namespace LoxLanguage {
  abstract class Expr {
   class Binary : Expr {
     Binary(Expr left, Token op, Expr right) {
       this.left = left;
       this.op= op;
       this.right = right;
     }

     private Expr left;
     private Token op;
     private Expr right;
   }
  }

 }

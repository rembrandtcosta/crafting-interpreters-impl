
namespace LoxLanguage {
  class Token {
    TokenType type;
    String lexeme;
    Object? literal;
    int line;

    public Token(TokenType type, String lexeme, Object? literal, int line) {
      this.type = type;
      this.lexeme = lexeme;
      this.literal = literal;
      this.line = line;
    }

    public override String ToString() {
      return type + " " + lexeme + " " + literal;
    }
  }
}

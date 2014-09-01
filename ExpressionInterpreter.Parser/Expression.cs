using System;
using System.Collections.Generic;
using System.IO;

namespace ExpressionInterpreter.Parser
{
    public abstract class Expression
    {
		public abstract int Evaluate();

		public static Expression ParseLine(TextReader reader)
		{
			using (var enumerator = Token.ReadLine(reader).GetEnumerator()) {
				enumerator.MoveNext();
				try {
					return enumerator.Current == null ? null : ParseExpression(enumerator);
				} catch {
					while (enumerator.MoveNext()) ;
					throw;
				}
			}
		}

		static Expression ParseExpression(IEnumerator<Token> enumerator)
		{
			var expr = ParseTerm(enumerator);
			while (true) {
				var op = ParseOperator(enumerator, expressionOperatorMap);
				if (op == null) {
					var token = enumerator.Current;
					var symbolToken = token as SymbolToken;
					var isEndOfSequence = token == null;
					var isEndOfParentheses = symbolToken != null && symbolToken.Symbol == Symbol.RightParenthesis;
					Check(isEndOfSequence || isEndOfParentheses);
					break;
				}
				expr = new BinaryExpression(op.Value, expr, ParseTerm(enumerator));
			}

			return expr;
		}

		static Expression ParseTerm(IEnumerator<Token> enumerator)
		{
			var term = ParseFactor(enumerator);
			while (true) {
				var op = ParseOperator(enumerator, termOperatorMap);
				if (op == null) {
					break;
				}
				term = new BinaryExpression(op.Value, term, ParseFactor(enumerator));
			}

			return term;
		}

		static Expression ParseFactor(IEnumerator<Token> enumerator)
		{
			var num = ParseNumber(enumerator);
			if (num != null) {
				return new NumberExpression(num.Value);
			}

			Check(SkipSymbol(enumerator, Symbol.LeftParenthesis));
			var expr = ParseExpression(enumerator);
			Check(SkipSymbol(enumerator, Symbol.RightParenthesis));
			return expr;
		}

		static int? ParseNumber(IEnumerator<Token> enumerator)
		{
			var numberToken = enumerator.Current as NumberToken;
			if (numberToken == null) {
				return null;
			}
			enumerator.MoveNext();
			return numberToken.Value;
		}

		static Operator? ParseOperator(IEnumerator<Token> enumerator, IDictionary<Symbol, Operator> map)
		{
			var symbolToken = enumerator.Current as SymbolToken;
			Operator op;
			if (symbolToken == null || !map.TryGetValue(symbolToken.Symbol, out op)) {
				return null;
			}
			enumerator.MoveNext();
			return op;
		}

		static bool SkipSymbol(IEnumerator<Token> enumerator, Symbol symbol)
		{
			var symbolToken = enumerator.Current as SymbolToken;
			if (symbolToken == null || symbolToken.Symbol != symbol) {
				return false;
			}
			enumerator.MoveNext();
			return true;
		}

		static void Check(bool condition)
		{
			if (!condition) {
				throw new Exception("invalid token sequence");
			}
		}

		static Dictionary<Symbol, Operator> expressionOperatorMap = new Dictionary<Symbol, Operator> {
			{ Symbol.Plus, Operator.Addition },
			{ Symbol.Minus, Operator.Subtraction }
		};

		static Dictionary<Symbol, Operator> termOperatorMap = new Dictionary<Symbol, Operator> {
			{ Symbol.Asterisk, Operator.Multiply },
			{ Symbol.Slash, Operator.Division }
		};
    }

	public class NumberExpression : Expression
	{
		public int Value { get; private set; }

		internal NumberExpression(int value)
		{
			this.Value = value;
		}

		public override int Evaluate()
		{
			return this.Value;
		}
	}

	public class BinaryExpression : Expression
	{
		public Operator Operator { get; private set; }
		public Expression Left { get; private set; }
		public Expression Right { get; private set; }

		internal BinaryExpression(Operator op, Expression left, Expression right)
		{
			this.Operator = op;
			this.Left = left;
			this.Right = right;
		}

		public override int Evaluate()
		{
			return operatorFuncs[(int)this.Operator](this.Left.Evaluate(), this.Right.Evaluate());
		}

		static Func<int, int, int>[] operatorFuncs = {
			(x, y) => x + y,
			(x, y) => x - y,
			(x, y) => x * y,
			(x, y) => x / y
		};
	}

	public enum Operator { Addition, Subtraction, Multiply, Division }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace ExpressionInterpreter.Parser
{
	abstract class Token
	{
		public static IEnumerable<Token> ReadLine(TextReader reader)
		{
			var enumerator = EnumerateChars(reader).GetEnumerator();
			enumerator.MoveNext();

			while (true) {
				var ch = enumerator.Current;

				if (ch == ' ') {
					enumerator.MoveNext();
					continue;
				}

				var token = ReadNumber(enumerator) ?? ReadSymbol(enumerator);
				if (token != null) {
					yield return token;
					continue;
				}

				if (ch == '\n') {
					break;
				}

				while (enumerator.MoveNext()) ;
				throw new Exception("invalid character '" + ch + "'");
			}

			yield return null;
		}

		static Token ReadNumber(IEnumerator<char> enumerator)
		{
			var builder = new StringBuilder();
			while (char.IsDigit(enumerator.Current)) {
				builder.Append(enumerator.Current);
				enumerator.MoveNext();
			}
			return builder.Length == 0 ? null : new NumberToken(int.Parse(builder.ToString()));
		}

		static Token ReadSymbol(IEnumerator<char> enumerator)
		{
			var idx = SymbolToken.Symbols.IndexOf(enumerator.Current);
			if (idx == -1) {
				return null;
			} else {
				enumerator.MoveNext();
				return new SymbolToken((Symbol)idx);
			}
		}

		static IEnumerable<char> EnumerateChars(TextReader reader)
		{
			while (true) {
				var code = reader.Read();
				if (code == -1) {
					break;
				}
				var ch = (char)code;

				if (ch == '\r') {
					continue;
				}
				if (ch == '\n') {
					break;
				}

				yield return ch;
			}

			yield return '\n';
		}
	}

	class NumberToken : Token
	{
		public int Value { get; private set; }

		public NumberToken(int value)
		{
			this.Value = value;
		}
	}

	class SymbolToken : Token
	{
		public Symbol Symbol { get; private set; }

		public SymbolToken(Symbol symbol)
		{
			this.Symbol = symbol;
		}

		public static readonly ReadOnlyCollection<char> Symbols =
			new ReadOnlyCollection<char>(new[] { '+', '-', '*', '/', '(', ')' });
	}

	enum Symbol { Plus, Minus, Asterisk, Slash, LeftParenthesis, RightParenthesis }
}

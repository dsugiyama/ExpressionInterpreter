using System;
using ExpressionInterpreter.Parser;

namespace ExpressionInterpreter
{
	class Program
	{
		static void Main()
		{
			while (true) {
				try {
					Console.Write("> ");
					var expr = Expression.ParseLine(Console.In);
					if (expr == null) {
						break;
					}
					Console.WriteLine("= {0}", expr.Evaluate());
				} catch (Exception ex) {
					Console.WriteLine("error: " + ex.Message);
				}
			}
		}
	}
}

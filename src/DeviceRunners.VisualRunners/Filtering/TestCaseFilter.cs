using System.Text;
using System.Text.RegularExpressions;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// Parses and evaluates a <c>dotnet test --filter</c> style expression against test cases.
/// </summary>
/// <remarks>
/// Supports the documented VSTest filter grammar subset:
/// <list type="bullet">
/// <item>Conditions: <c>&lt;property&gt;&lt;operator&gt;&lt;value&gt;</c> with operators
/// <c>=</c> (equals), <c>!=</c> (not equals), <c>~</c> (contains) and <c>!~</c> (not contains).</item>
/// <item>Properties (case-insensitive): <c>FullyQualifiedName</c> (the default when the
/// property is omitted), <c>DisplayName</c>, <c>Name</c>, <c>ClassName</c>, <c>Namespace</c>,
/// and any trait name (e.g. <c>Category</c>).</item>
/// <item>Logic: <c>&amp;</c> (and), <c>|</c> (or), grouping with <c>(</c> and <c>)</c>.
/// <c>&amp;</c> binds tighter than <c>|</c>. Use <c>\</c> to escape a special character.</item>
/// <item>Wildcards: a <c>*</c> in the value of an equals condition (<c>=</c> or <c>!=</c>)
/// matches zero or more characters (e.g. <c>ClassName=Calc*</c>). This is a DeviceRunners
/// extension over VSTest; a value without <c>*</c> still matches exactly as before.</item>
/// </list>
/// </remarks>
public static class TestCaseFilter
{
	/// <summary>
	/// Parses the given filter expression into an evaluable <see cref="ITestCaseFilter"/>.
	/// An empty or whitespace expression returns a filter that matches everything.
	/// </summary>
	/// <exception cref="FormatException">The expression is malformed.</exception>
	public static ITestCaseFilter Parse(string? expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return MatchAllFilter.Instance;

		var tokens = Tokenize(expression!);
		var parser = new Parser(tokens, expression!);
		var filter = parser.Parse();
		return filter;
	}

	/// <summary>
	/// Attempts to parse the given filter expression. Returns <c>false</c> if it is malformed.
	/// </summary>
	public static bool TryParse(string? expression, out ITestCaseFilter filter)
	{
		try
		{
			filter = Parse(expression);
			return true;
		}
		catch (FormatException)
		{
			filter = MatchAllFilter.Instance;
			return false;
		}
	}

	enum FilterOperator
	{
		Equals,
		NotEquals,
		Contains,
		NotContains,
	}

	enum TokenKind
	{
		Condition,
		And,
		Or,
		OpenParen,
		CloseParen,
	}

	readonly struct Token
	{
		public Token(TokenKind kind, string property = "", FilterOperator op = FilterOperator.Contains, string value = "")
		{
			Kind = kind;
			Property = property;
			Operator = op;
			Value = value;
		}

		public TokenKind Kind { get; }
		public string Property { get; }
		public FilterOperator Operator { get; }
		public string Value { get; }
	}

	static List<Token> Tokenize(string expression)
	{
		var tokens = new List<Token>();
		var i = 0;

		while (i < expression.Length)
		{
			var c = expression[i];

			switch (c)
			{
				case '(':
					tokens.Add(new Token(TokenKind.OpenParen));
					i++;
					break;
				case ')':
					tokens.Add(new Token(TokenKind.CloseParen));
					i++;
					break;
				case '&':
					tokens.Add(new Token(TokenKind.And));
					i++;
					break;
				case '|':
					tokens.Add(new Token(TokenKind.Or));
					i++;
					break;
				default:
					i = ReadCondition(expression, i, tokens);
					break;
			}
		}

		return tokens;
	}

	static int ReadCondition(string expression, int start, List<Token> tokens)
	{
		var property = new StringBuilder();
		var value = new StringBuilder();
		FilterOperator? op = null;
		var target = property;

		var i = start;
		while (i < expression.Length)
		{
			var c = expression[i];

			if (c == '\\')
			{
				if (i + 1 < expression.Length)
				{
					target.Append(expression[i + 1]);
					i += 2;
				}
				else
				{
					target.Append('\\');
					i++;
				}

				continue;
			}

			if (c is '(' or ')' or '&' or '|')
				break;

			if (op is null)
			{
				if (c == '=')
				{
					op = FilterOperator.Equals;
					target = value;
					i++;
					continue;
				}

				if (c == '~')
				{
					op = FilterOperator.Contains;
					target = value;
					i++;
					continue;
				}

				if (c == '!' && i + 1 < expression.Length && (expression[i + 1] == '=' || expression[i + 1] == '~'))
				{
					op = expression[i + 1] == '=' ? FilterOperator.NotEquals : FilterOperator.NotContains;
					target = value;
					i += 2;
					continue;
				}
			}

			target.Append(c);
			i++;
		}

		if (op is null)
		{
			// A bare value with no operator matches FullyQualifiedName using "contains".
			var bareValue = property.ToString().Trim();
			if (bareValue.Length > 0)
				tokens.Add(new Token(TokenKind.Condition, string.Empty, FilterOperator.Contains, bareValue));
		}
		else
		{
			tokens.Add(new Token(TokenKind.Condition, property.ToString().Trim(), op.Value, value.ToString().Trim()));
		}

		return i;
	}

	sealed class Parser
	{
		readonly List<Token> _tokens;
		readonly string _expression;
		int _position;

		public Parser(List<Token> tokens, string expression)
		{
			_tokens = tokens;
			_expression = expression;
		}

		public ITestCaseFilter Parse()
		{
			if (_tokens.Count == 0)
				return MatchAllFilter.Instance;

			var filter = ParseOr();

			if (_position != _tokens.Count)
				throw Error("unexpected trailing input");

			return filter;
		}

		ITestCaseFilter ParseOr()
		{
			var left = ParseAnd();

			while (Peek()?.Kind == TokenKind.Or)
			{
				_position++;
				var right = ParseAnd();
				left = new OrFilter(left, right);
			}

			return left;
		}

		ITestCaseFilter ParseAnd()
		{
			var left = ParsePrimary();

			while (Peek()?.Kind == TokenKind.And)
			{
				_position++;
				var right = ParsePrimary();
				left = new AndFilter(left, right);
			}

			return left;
		}

		ITestCaseFilter ParsePrimary()
		{
			var token = Peek() ?? throw Error("unexpected end of expression");

			switch (token.Kind)
			{
				case TokenKind.OpenParen:
					_position++;
					var inner = ParseOr();
					if (Peek()?.Kind != TokenKind.CloseParen)
						throw Error("missing closing parenthesis");
					_position++;
					return inner;

				case TokenKind.Condition:
					_position++;
					return new ConditionFilter(token.Property, token.Operator, token.Value);

				default:
					throw Error($"unexpected token '{token.Kind}'");
			}
		}

		Token? Peek() =>
			_position < _tokens.Count ? _tokens[_position] : null;

		FormatException Error(string reason) =>
			new($"Invalid test filter expression '{_expression}': {reason}.");
	}

	sealed class MatchAllFilter : ITestCaseFilter
	{
		public static readonly MatchAllFilter Instance = new();

		public bool Matches(ITestCaseInfo testCase) => true;
	}

	sealed class AndFilter(ITestCaseFilter left, ITestCaseFilter right) : ITestCaseFilter
	{
		public bool Matches(ITestCaseInfo testCase) =>
			left.Matches(testCase) && right.Matches(testCase);
	}

	sealed class OrFilter(ITestCaseFilter left, ITestCaseFilter right) : ITestCaseFilter
	{
		public bool Matches(ITestCaseInfo testCase) =>
			left.Matches(testCase) || right.Matches(testCase);
	}

	sealed class ConditionFilter(string property, FilterOperator op, string value) : ITestCaseFilter
	{
		// Wildcards only apply to the equals family. A '*' in the value matches zero or
		// more characters; a value without '*' keeps the original exact-match behavior.
		readonly Regex? _wildcard =
			(op is FilterOperator.Equals or FilterOperator.NotEquals) && value.Contains('*')
				? BuildWildcardRegex(value)
				: null;

		public bool Matches(ITestCaseInfo testCase)
		{
			var actualValues = ResolveValues(testCase);

			return op switch
			{
				FilterOperator.Equals => actualValues.Any(Equals),
				FilterOperator.Contains => actualValues.Any(Contains),
				FilterOperator.NotEquals => !actualValues.Any(Equals),
				FilterOperator.NotContains => !actualValues.Any(Contains),
				_ => false,
			};
		}

		bool Equals(string? actual) =>
			actual is not null && (_wildcard is not null
				? _wildcard.IsMatch(actual)
				: string.Equals(actual, value, StringComparison.OrdinalIgnoreCase));

		bool Contains(string? actual) =>
			actual is not null && actual.Contains(value, StringComparison.OrdinalIgnoreCase);

		IEnumerable<string?> ResolveValues(ITestCaseInfo testCase)
		{
			if (property.Length == 0 || string.Equals(property, "FullyQualifiedName", StringComparison.OrdinalIgnoreCase))
				return new[] { GetFullyQualifiedName(testCase) };

			if (string.Equals(property, "DisplayName", StringComparison.OrdinalIgnoreCase))
				return new[] { testCase.DisplayName };

			if (string.Equals(property, "Name", StringComparison.OrdinalIgnoreCase))
				return new[] { testCase.TestMethodName };

			if (string.Equals(property, "ClassName", StringComparison.OrdinalIgnoreCase))
				return new[] { testCase.TestClassName };

			if (string.Equals(property, "Namespace", StringComparison.OrdinalIgnoreCase))
				return new[] { testCase.TestClassNamespace };

			// Otherwise treat the property as a trait name (case-insensitive lookup).
			foreach (var trait in testCase.Traits)
			{
				if (string.Equals(trait.Key, property, StringComparison.OrdinalIgnoreCase))
					return trait.Value;
			}

			return Array.Empty<string?>();
		}

		static string? GetFullyQualifiedName(ITestCaseInfo testCase)
		{
			var className = testCase.TestClassName;
			var methodName = testCase.TestMethodName;

			if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(methodName))
				return $"{className}.{methodName}";

			return className ?? methodName ?? testCase.DisplayName;
		}

		// Translates a wildcard value into an anchored, case-insensitive regex where
		// '*' matches zero or more characters and every other character is literal.
		// NonBacktracking guarantees linear-time matching, so a pathological filter
		// (e.g. many '*' segments) cannot trigger catastrophic backtracking.
		static Regex BuildWildcardRegex(string value)
		{
			var pattern = string.Join(".*", value.Split('*').Select(Regex.Escape));
			return new Regex(
				$"^{pattern}$",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
		}
	}
}

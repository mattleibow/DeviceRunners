using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TestCaseFilterTests
{
	static StubTestCaseInfo Case(
		string? className = "MyApp.Calculator",
		string? methodName = "Adds",
		string? displayName = null,
		params (string Key, string[] Values)[] traits)
	{
		var traitDict = new Dictionary<string, IReadOnlyList<string>>();
		foreach (var (key, values) in traits)
			traitDict[key] = values;

		return new StubTestCaseInfo
		{
			TestClassName = className,
			TestMethodName = methodName,
			DisplayName = displayName ?? $"{className}.{methodName}",
			Traits = traitDict,
		};
	}

	[Theory]
	[InlineData("FullyQualifiedName=MyApp.Calculator.Adds", true)]
	[InlineData("FullyQualifiedName=MyApp.Calculator.Subtracts", false)]
	[InlineData("FullyQualifiedName~Calculator", true)]
	[InlineData("FullyQualifiedName~Other", false)]
	[InlineData("FullyQualifiedName!=MyApp.Calculator.Adds", false)]
	[InlineData("FullyQualifiedName!~Other", true)]
	public void FullyQualifiedNameMatching(string expression, bool expected)
	{
		var filter = TestCaseFilter.Parse(expression);
		Assert.Equal(expected, filter.Matches(Case()));
	}

	[Fact]
	public void BareValueMatchesFullyQualifiedNameContains()
	{
		var filter = TestCaseFilter.Parse("Calculator");
		Assert.True(filter.Matches(Case()));
		Assert.False(filter.Matches(Case(className: "MyApp.Other")));
	}

	[Fact]
	public void NameMatchesMethodSimpleName()
	{
		var filter = TestCaseFilter.Parse("Name=Adds");
		Assert.True(filter.Matches(Case()));
		Assert.False(filter.Matches(Case(methodName: "Subtracts")));
	}

	[Fact]
	public void ClassNameAndNamespaceMatching()
	{
		var caseInfo = Case();
		Assert.True(TestCaseFilter.Parse("ClassName=MyApp.Calculator").Matches(caseInfo));
		Assert.True(TestCaseFilter.Parse("Namespace=MyApp").Matches(caseInfo));
		Assert.False(TestCaseFilter.Parse("Namespace=Other").Matches(caseInfo));
	}

	[Fact]
	public void DisplayNameMatching()
	{
		var caseInfo = Case(displayName: "Calculator adds two numbers");
		Assert.True(TestCaseFilter.Parse("DisplayName~two numbers").Matches(caseInfo));
	}

	[Fact]
	public void TraitMatching()
	{
		var caseInfo = Case(traits: ("Category", new[] { "Smoke", "Fast" }));
		Assert.True(TestCaseFilter.Parse("Category=Smoke").Matches(caseInfo));
		Assert.True(TestCaseFilter.Parse("Category=Fast").Matches(caseInfo));
		Assert.False(TestCaseFilter.Parse("Category=Slow").Matches(caseInfo));
		Assert.True(TestCaseFilter.Parse("category~Sm").Matches(caseInfo));
	}

	[Fact]
	public void AndBindsTighterThanOr()
	{
		// Adds & Category=Slow | Subtracts  =>  (Adds & Slow) | Subtracts
		var filter = TestCaseFilter.Parse("Name=Adds&Category=Slow|Name=Subtracts");

		Assert.False(filter.Matches(Case(methodName: "Adds", traits: ("Category", new[] { "Fast" }))));
		Assert.True(filter.Matches(Case(methodName: "Adds", traits: ("Category", new[] { "Slow" }))));
		Assert.True(filter.Matches(Case(methodName: "Subtracts")));
	}

	[Fact]
	public void ParenthesesOverridePrecedence()
	{
		var filter = TestCaseFilter.Parse("(Name=Adds|Name=Subtracts)&Category=Smoke");

		Assert.True(filter.Matches(Case(methodName: "Adds", traits: ("Category", new[] { "Smoke" }))));
		Assert.False(filter.Matches(Case(methodName: "Adds", traits: ("Category", new[] { "Other" }))));
		Assert.False(filter.Matches(Case(methodName: "Multiplies", traits: ("Category", new[] { "Smoke" }))));
	}

	[Fact]
	public void EscapedSpecialCharacters()
	{
		var caseInfo = Case(className: "MyApp.Calc(v2)", methodName: "Adds");
		var filter = TestCaseFilter.Parse(@"FullyQualifiedName~Calc\(v2\)");
		Assert.True(filter.Matches(caseInfo));
	}

	[Fact]
	public void EmptyOrNullExpressionMatchesEverything()
	{
		Assert.True(TestCaseFilter.Parse(null).Matches(Case()));
		Assert.True(TestCaseFilter.Parse("").Matches(Case()));
		Assert.True(TestCaseFilter.Parse("   ").Matches(Case()));
	}

	[Theory]
	[InlineData("(Name=Adds")]
	[InlineData("Name=Adds)")]
	[InlineData("Name=Adds&")]
	[InlineData("|Name=Adds")]
	public void MalformedExpressionsThrow(string expression)
	{
		Assert.Throws<FormatException>(() => TestCaseFilter.Parse(expression));
	}

	[Fact]
	public void TryParseReturnsFalseOnMalformed()
	{
		Assert.False(TestCaseFilter.TryParse("(Name=Adds", out _));
		Assert.True(TestCaseFilter.TryParse("Name=Adds", out var filter));
		Assert.True(filter.Matches(Case()));
	}

	[Theory]
	[InlineData("ClassName=MyApp.Calc*", true)]   // starts-with
	[InlineData("ClassName=*Calculator", true)]   // ends-with
	[InlineData("ClassName=*Calc*", true)]        // contains
	[InlineData("ClassName=*", true)]             // match-all
	[InlineData("ClassName=M*A*C*r", true)]       // multiple wildcard segments
	[InlineData("ClassName=*Z*Z*Z*Z*Z*Z*Z*", false)] // many segments, no match (no backtracking blow-up)
	[InlineData("ClassName=MyApp.Other*", false)]
	[InlineData("ClassName=myapp.calc*", true)]   // case-insensitive
	[InlineData("ClassName=MyApp.Calculator", true)] // exact still works (parity)
	[InlineData("ClassName=MyApp.Calc", false)]   // exact, no wildcard => no match
	[InlineData("FullyQualifiedName=MyApp.*.Adds", true)] // mid-string wildcard
	[InlineData("FullyQualifiedName=*.Adds", true)]
	[InlineData("ClassName!=MyApp.Calc*", false)] // negated wildcard match excludes
	[InlineData("ClassName!=Other*", true)]
	public void WildcardMatching(string expression, bool expected)
	{
		var filter = TestCaseFilter.Parse(expression);
		Assert.Equal(expected, filter.Matches(Case()));
	}

	[Fact]
	public void WildcardMatchingTraits()
	{
		var caseInfo = Case(traits: ("Category", new[] { "Smoke", "Fast" }));
		Assert.True(TestCaseFilter.Parse("Category=Sm*").Matches(caseInfo));
		Assert.True(TestCaseFilter.Parse("Category=*ast").Matches(caseInfo));
		Assert.False(TestCaseFilter.Parse("Category=Slo*").Matches(caseInfo));
	}

	[Fact]
	public void WildcardOnlyAppliesToEqualsFamily()
	{
		// '~' (contains) keeps treating '*' literally, so the literal substring
		// "Calc*" is never found in "MyApp.Calculator".
		var caseInfo = Case(className: "MyApp.Calculator");
		Assert.False(TestCaseFilter.Parse("ClassName~Calc*").Matches(caseInfo));
	}

	[Fact]
	public void EscapedWildcardMatchesLiteralStar()
	{
		// "\*" escapes the wildcard so it matches a literal '*' rather than any characters.
		var starCase = Case(traits: ("Category", new[] { "A*B" }));
		Assert.True(TestCaseFilter.Parse(@"Category=A\*B").Matches(starCase));

		// The same expression must NOT match a value where '*' stood in for other chars,
		// proving the star is literal (not a wildcard).
		var wideCase = Case(traits: ("Category", new[] { "AXYZB" }));
		Assert.False(TestCaseFilter.Parse(@"Category=A\*B").Matches(wideCase));

		// A bare '*' still wildcards, so it matches the "AXYZB" value.
		Assert.True(TestCaseFilter.Parse("Category=A*B").Matches(wideCase));
	}

	[Fact]
	public void EscapedWildcardMixedWithWildcard()
	{
		// "a\**" == literal 'a*' followed by a wildcard, matching anything that starts
		// with the literal "a*".
		var caseInfo = Case(traits: ("Category", new[] { "a*bcd" }));
		Assert.True(TestCaseFilter.Parse(@"Category=a\**").Matches(caseInfo));
		Assert.False(TestCaseFilter.Parse(@"Category=a\**").Matches(Case(traits: ("Category", new[] { "axbcd" }))));
	}

	[Fact]
	public void EscapedWildcardOnExactValueDoesNotWildcard()
	{
		// A fully-escaped star with no bare '*' is an exact literal match, so a value
		// that would match a wildcard ("anything") must not match here.
		var literal = Case(traits: ("Category", new[] { "*" }));
		Assert.True(TestCaseFilter.Parse(@"Category=\*").Matches(literal));
		Assert.False(TestCaseFilter.Parse(@"Category=\*").Matches(Case(traits: ("Category", new[] { "Smoke" }))));
	}

	class StubTestCaseInfo : ITestCaseInfo
	{
		public ITestAssemblyInfo TestAssembly { get; set; } = null!;
		public string DisplayName { get; set; } = "";
		public ITestResultInfo? Result => null;
		public string? TestClassName { get; set; }
		public string? TestMethodName { get; set; }
		public string? TestClassNamespace
		{
			get
			{
				if (string.IsNullOrEmpty(TestClassName))
					return null;
				var index = TestClassName!.LastIndexOf('.');
				return index > 0 ? TestClassName.Substring(0, index) : null;
			}
		}
		public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; set; } =
			new Dictionary<string, IReadOnlyList<string>>();

		public event Action<ITestResultInfo>? ResultReported
		{
			add { }
			remove { }
		}
	}
}

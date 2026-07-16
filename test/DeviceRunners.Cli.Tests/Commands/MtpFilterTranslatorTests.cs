using DeviceRunners.Cli.Commands;

namespace DeviceRunners.Cli.Tests;

public class MtpFilterTranslatorTests
{
	sealed class Filters : IMtpSimpleFilterSettings
	{
		public string[]? FilterClass { get; set; }
		public string[]? FilterNotClass { get; set; }
		public string[]? FilterMethod { get; set; }
		public string[]? FilterNotMethod { get; set; }
		public string[]? FilterNamespace { get; set; }
		public string[]? FilterNotNamespace { get; set; }
		public string[]? FilterTrait { get; set; }
		public string[]? FilterNotTrait { get; set; }
	}

	[Fact]
	public void NoFilters_ReturnsNull()
	{
		Assert.Null(MtpFilterTranslator.Translate(new Filters()));
		Assert.False(MtpFilterTranslator.HasSimpleFilters(new Filters()));
	}

	[Fact]
	public void WhitespaceOnly_IsIgnored()
	{
		var filters = new Filters { FilterClass = new[] { "  ", "" } };
		Assert.Null(MtpFilterTranslator.Translate(filters));
		Assert.False(MtpFilterTranslator.HasSimpleFilters(filters));
	}

	[Fact]
	public void SingleClass_MapsToClassNameEquals()
	{
		var filters = new Filters { FilterClass = new[] { "MyApp.Calculator" } };
		Assert.True(MtpFilterTranslator.HasSimpleFilters(filters));
		Assert.Equal("ClassName=MyApp.Calculator", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void MultipleClass_OrTogetherInParens()
	{
		var filters = new Filters { FilterClass = new[] { "A", "B" } };
		Assert.Equal("(ClassName=A|ClassName=B)", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void NotClass_Single_MapsToNotEquals()
	{
		var filters = new Filters { FilterNotClass = new[] { "A" } };
		Assert.Equal("ClassName!=A", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void NotClass_Multiple_AndTogetherInParens()
	{
		var filters = new Filters { FilterNotClass = new[] { "A", "B" } };
		Assert.Equal("(ClassName!=A&ClassName!=B)", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void Method_MapsToFullyQualifiedName()
	{
		var filters = new Filters { FilterMethod = new[] { "My.Ns.Cls.M" } };
		Assert.Equal("FullyQualifiedName=My.Ns.Cls.M", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void Namespace_MapsToNamespace()
	{
		var filters = new Filters { FilterNamespace = new[] { "My.Ns" } };
		Assert.Equal("Namespace=My.Ns", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void Trait_SplitsNameAndValue()
	{
		var filters = new Filters { FilterTrait = new[] { "Category=Fast" } };
		Assert.Equal("Category=Fast", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void NotTrait_MapsToNotEquals()
	{
		var filters = new Filters { FilterNotTrait = new[] { "Category=Slow" } };
		Assert.Equal("Category!=Slow", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void DifferentKinds_AndTogether()
	{
		var filters = new Filters
		{
			FilterClass = new[] { "A", "B" },
			FilterTrait = new[] { "Cat=Fast" },
		};
		Assert.Equal("(ClassName=A|ClassName=B) & Cat=Fast", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void InclusiveAndExclusive_Combine()
	{
		var filters = new Filters
		{
			FilterClass = new[] { "Calc" },
			FilterNotMethod = new[] { "Calc.Slow" },
		};
		Assert.Equal("ClassName=Calc & FullyQualifiedName!=Calc.Slow", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void Wildcards_ArePreserved()
	{
		var filters = new Filters { FilterClass = new[] { "Calc*", "*Helper" } };
		Assert.Equal("(ClassName=Calc*|ClassName=*Helper)", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void StructuralCharacters_AreEscaped()
	{
		// Parentheses, ampersands and pipes in names must be escaped so they are
		// treated literally by the on-device grammar; '*' is left intact.
		var filters = new Filters { FilterClass = new[] { "A(b)&c|d" } };
		Assert.Equal(@"ClassName=A\(b\)\&c\|d", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void TraitValueWithEquals_KeepsOperatorOnFirstEquals()
	{
		var filters = new Filters { FilterTrait = new[] { "Cat=a=b" } };
		Assert.Equal(@"Cat=a\=b", MtpFilterTranslator.Translate(filters));
	}

	[Fact]
	public void ValidateTraits_AcceptsWellFormedTraits()
	{
		var filters = new Filters
		{
			FilterTrait = new[] { "Category=Fast" },
			FilterNotTrait = new[] { "Category=Slow" },
		};
		Assert.Null(MtpFilterTranslator.ValidateTraits(filters));
	}

	[Fact]
	public void ValidateTraits_RejectsTraitWithoutSeparator()
	{
		var filters = new Filters { FilterTrait = new[] { "Category" } };
		Assert.NotNull(MtpFilterTranslator.ValidateTraits(filters));
	}

	[Fact]
	public void ValidateTraits_RejectsTraitWithEmptyName()
	{
		var filters = new Filters { FilterNotTrait = new[] { "=Fast" } };
		Assert.NotNull(MtpFilterTranslator.ValidateTraits(filters));
	}

	[Fact]
	public void ValidateTraits_IgnoresNonTraitFilters()
	{
		var filters = new Filters { FilterClass = new[] { "Calc" } };
		Assert.Null(MtpFilterTranslator.ValidateTraits(filters));
	}
}

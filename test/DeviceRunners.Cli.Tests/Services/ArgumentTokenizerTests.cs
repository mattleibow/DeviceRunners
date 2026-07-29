using DeviceRunners.Cli.Services;

using Xunit;

namespace DeviceRunners.Cli.Tests;

public class ArgumentTokenizerTests
{
	[Fact]
	public void EmptyInputProducesNoArguments()
	{
		Assert.Empty(ArgumentTokenizer.Tokenize(null));
		Assert.Empty(ArgumentTokenizer.Tokenize(""));
		Assert.Empty(ArgumentTokenizer.Tokenize("   "));
	}

	[Fact]
	public void SplitsOnWhitespace()
	{
		var args = ArgumentTokenizer.Tokenize("--enable-unsafe-swiftshader --enable-unsafe-webgpu");

		Assert.Equal(["--enable-unsafe-swiftshader", "--enable-unsafe-webgpu"], args);
	}

	[Fact]
	public void CollapsesRepeatedWhitespace()
	{
		var args = ArgumentTokenizer.Tokenize("  --a \t  --b\n--c ");

		Assert.Equal(["--a", "--b", "--c"], args);
	}

	[Fact]
	public void DoubleQuotesGroupAValueContainingSpaces()
	{
		var args = ArgumentTokenizer.Tokenize("--a \"--host-resolver-rules=MAP a.test 127.0.0.1\" --b");

		Assert.Equal(["--a", "--host-resolver-rules=MAP a.test 127.0.0.1", "--b"], args);
	}

	[Fact]
	public void SingleQuotesGroupAValueContainingSpaces()
	{
		var args = ArgumentTokenizer.Tokenize("--a '--rules=MAP a.test 127.0.0.1'");

		Assert.Equal(["--a", "--rules=MAP a.test 127.0.0.1"], args);
	}

	[Fact]
	public void QuotesMayStartPartWayThroughAnArgument()
	{
		var args = ArgumentTokenizer.Tokenize("--host-resolver-rules=\"MAP a.test 127.0.0.1\"");

		Assert.Equal(["--host-resolver-rules=MAP a.test 127.0.0.1"], args);
	}

	[Fact]
	public void EscapedQuoteInsideDoubleQuotesIsLiteral()
	{
		var args = ArgumentTokenizer.Tokenize("--a=\"say \\\"hi\\\"\"");

		Assert.Equal(["--a=say \"hi\""], args);
	}

	[Fact]
	public void BackslashOutsideQuotesIsNotAnEscape()
	{
		// Windows paths must survive without doubling.
		var args = ArgumentTokenizer.Tokenize(@"--user-data-dir=C:\temp\profile");

		Assert.Equal([@"--user-data-dir=C:\temp\profile"], args);
	}

	[Fact]
	public void BackslashInsideSingleQuotesIsNotAnEscape()
	{
		var args = ArgumentTokenizer.Tokenize(@"'--dir=C:\temp\a b'");

		Assert.Equal([@"--dir=C:\temp\a b"], args);
	}

	[Fact]
	public void AnEmptyQuotedSectionStillProducesAnArgument()
	{
		var args = ArgumentTokenizer.Tokenize("--a \"\" --b");

		Assert.Equal(["--a", "", "--b"], args);
	}

	[Fact]
	public void UnbalancedQuoteIsReported()
	{
		// Silently dropping the rest of the line would be far harder to diagnose than
		// a message naming the offending value.
		var ex = Assert.Throws<FormatException>(() => ArgumentTokenizer.Tokenize("--a \"--b"));

		Assert.Contains("Unbalanced", ex.Message);
	}
}

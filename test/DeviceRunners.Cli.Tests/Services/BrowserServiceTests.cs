using DeviceRunners.Cli.Services;

using Xunit;

namespace DeviceRunners.Cli.Tests;

public class BrowserServiceTests
{
	// LaunchAsync starts a real browser, so exercise the argument assembly through the
	// same helper it uses rather than launching Chrome from a unit test.
	static List<string> BuildArgs(IEnumerable<string>? extra, bool headless = true) =>
		BrowserService.BuildLaunchArguments("/tmp/profile", extra, headless);

	[Fact]
	public void NoExtraArgumentsKeepsTheBaseSwitchesOnly()
	{
		var args = BuildArgs(null);

		Assert.Contains("--remote-debugging-port=0", args);
		Assert.Contains("--user-data-dir=/tmp/profile", args);
		Assert.Contains("--headless=new", args);
		Assert.Equal("about:blank", args[^1]);
	}

	[Fact]
	public void ExtraArgumentsAreAppended()
	{
		var args = BuildArgs(["--enable-unsafe-webgpu", "--use-webgpu-adapter=swiftshader"]);

		Assert.Contains("--enable-unsafe-webgpu", args);
		Assert.Contains("--use-webgpu-adapter=swiftshader", args);
	}

	[Fact]
	public void ExtraArgumentsComeAfterEverythingTheCliSets()
	{
		// Chrome keeps only the last occurrence of most switches, so a caller must be
		// able to override anything the CLI sets itself.
		var args = BuildArgs(["--headless=old"]);

		Assert.True(args.IndexOf("--headless=old") > args.IndexOf("--headless=new"));
	}

	[Fact]
	public void ExtraArgumentsComeBeforeTheTargetUrl()
	{
		// A positional value after the URL would be treated as a second page to open.
		var args = BuildArgs(["--enable-unsafe-webgpu"]);

		Assert.True(args.IndexOf("--enable-unsafe-webgpu") < args.IndexOf("about:blank"));
	}

	[Fact]
	public void ExtraArgumentOrderIsPreserved()
	{
		var args = BuildArgs(["--first", "--second", "--third"]);

		Assert.True(args.IndexOf("--first") < args.IndexOf("--second"));
		Assert.True(args.IndexOf("--second") < args.IndexOf("--third"));
	}

	[Fact]
	public void BlankExtraArgumentsAreIgnored()
	{
		// Defensive: an empty entry would otherwise become a stray empty token on the
		// command line.
		var args = BuildArgs(["", "   ", "--enable-unsafe-webgpu"]);

		Assert.DoesNotContain(args, string.IsNullOrWhiteSpace);
		Assert.Contains("--enable-unsafe-webgpu", args);
	}

	[Fact]
	public void AnArgumentContainingSpacesStaysASingleEntry()
	{
		// The shell has already tokenised the command line, so a quoted value arrives as
		// one entry and must stay that way. Chrome would otherwise treat the trailing
		// part as an extra page to open and refuse to start ("Multiple targets are not
		// supported in headless mode").
		var args = BuildArgs(["--host-resolver-rules=MAP a.test 127.0.0.1"]);

		Assert.Contains("--host-resolver-rules=MAP a.test 127.0.0.1", args);
	}

	[Fact]
	public void HeadedModeOmitsTheHeadlessSwitch()
	{
		var args = BuildArgs(null, headless: false);

		Assert.DoesNotContain("--headless=new", args);
	}
}

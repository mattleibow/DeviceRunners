using DeviceRunners.Cli.Commands;

namespace DeviceRunners.Cli.Tests;

public class ParseLoggerTests
{
    [Fact]
    public void SimpleNameOnly_ReturnsNameAndNullFileName()
    {
        var (name, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx");

        Assert.Equal("trx", name);
        Assert.Null(logFileName);
    }

    [Fact]
    public void NameIsCaseInsensitive()
    {
        var (name, _) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("TRX");

        Assert.Equal("trx", name);
    }

    [Fact]
    public void WithLogFileName_ReturnsNameAndFileName()
    {
        var (name, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;LogFileName=my-results.trx");

        Assert.Equal("trx", name);
        Assert.Equal("my-results.trx", logFileName);
    }

    [Fact]
    public void LogFileName_IsCaseInsensitive()
    {
        var (_, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;logfilename=custom.trx");

        Assert.Equal("custom.trx", logFileName);
    }

    [Fact]
    public void MultipleParameters_ExtractsLogFileName()
    {
        var (name, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;Verbosity=detailed;LogFileName=output.trx");

        Assert.Equal("trx", name);
        Assert.Equal("output.trx", logFileName);
    }

    [Fact]
    public void UnknownParameters_AreIgnored()
    {
        var (name, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("txt;SomeOther=value");

        Assert.Equal("txt", name);
        Assert.Null(logFileName);
    }

    [Fact]
    public void EmptyLogFileName_ReturnsNull()
    {
        var (_, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;LogFileName=");

        Assert.Null(logFileName);
    }

    [Fact]
    public void WhitespaceLogFileName_ReturnsNull()
    {
        var (_, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;LogFileName=  ");

        Assert.Null(logFileName);
    }

    [Fact]
    public void TrailingSemicolon_IsHandled()
    {
        var (name, logFileName) = BaseTestCommand<WindowsTestCommand.Settings>.ParseLogger("trx;LogFileName=file.trx;");

        Assert.Equal("trx", name);
        Assert.Equal("file.trx", logFileName);
    }
}

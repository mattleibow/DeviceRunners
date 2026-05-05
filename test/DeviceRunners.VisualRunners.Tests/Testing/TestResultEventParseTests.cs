using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TestResultEventParseTests
{
	[Fact]
	public void Parse_NullOrEmpty_ReturnsNull()
	{
		Assert.Null(TestResultEvent.Parse(null!));
		Assert.Null(TestResultEvent.Parse(""));
		Assert.Null(TestResultEvent.Parse("   "));
	}

	[Fact]
	public void Parse_InvalidJson_ReturnsNull()
	{
		Assert.Null(TestResultEvent.Parse("not json at all"));
	}

	[Fact]
	public void Parse_ValidBeginEvent_ReturnsEvent()
	{
		var json = """{"type":"begin","message":"Starting","timestamp":"2026-01-01T00:00:00Z"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("begin", evt.Type);
		Assert.Equal("Starting", evt.Message);
		Assert.Equal("2026-01-01T00:00:00Z", evt.Timestamp);
	}

	[Fact]
	public void Parse_ValidResultEvent_ReturnsEvent()
	{
		var json = """{"type":"result","displayName":"MyTest","assembly":"test.dll","status":"Passed","duration":"00:00:01.5000000"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("result", evt.Type);
		Assert.Equal("MyTest", evt.DisplayName);
		Assert.Equal("test.dll", evt.Assembly);
		Assert.Equal("Passed", evt.Status);
		Assert.Equal("00:00:01.5000000", evt.Duration);
	}

	[Fact]
	public void Parse_ValidEndEvent_ReturnsEvent()
	{
		var json = """{"type":"end","timestamp":"2026-01-01T00:01:00Z"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("end", evt.Type);
	}

	[Fact]
	public void ToString_ProducesValidJson_ThatParseCanRoundTrip()
	{
		var evt = TestResultEvent.Begin("round-trip");
		var json = evt.ToString();

		var parsed = TestResultEvent.Parse(json);
		Assert.NotNull(parsed);
		Assert.Equal(TestResultEvent.TypeBegin, parsed.Type);
		Assert.Equal("round-trip", parsed.Message);
	}

	[Fact]
	public void ToString_OmitsNullProperties()
	{
		var evt = TestResultEvent.Begin();
		var json = evt.ToString();

		Assert.DoesNotContain("\"message\"", json);
		Assert.DoesNotContain("\"displayName\"", json);
		Assert.DoesNotContain("\"assembly\"", json);
	}
}

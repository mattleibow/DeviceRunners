using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

class NullMessageSink : LongLivedMarshalByRefObject, IMessageSink
{
	public static readonly NullMessageSink Instance = new();

	public bool OnMessage(IMessageSinkMessage message) => true;
}

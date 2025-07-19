namespace DeviceRunners.UIAutomation;

public class Base64StringInMemoryFile : IInMemoryFile
{
	private readonly string _data;

	public Base64StringInMemoryFile(string data) =>
		_data = data ?? throw new ArgumentNullException(nameof(data));

	public Stream ToStream() => new MemoryStream(Convert.FromBase64String(_data));

	public byte[] ToByteArray() => Convert.FromBase64String(_data);

	public string ToBase64String() => _data;
}

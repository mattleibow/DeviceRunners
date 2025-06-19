namespace DeviceRunners.UIAutomation;

public class ByteArrayInMemoryFile : IInMemoryFile
{
	private readonly byte[] _data;

	public ByteArrayInMemoryFile(byte[] data) =>
		_data = data ?? throw new ArgumentNullException(nameof(data));

	public Stream ToStream() => new MemoryStream(_data);

	public byte[] ToByteArray() => _data;

	public string ToBase64String() => Convert.ToBase64String(_data);
}

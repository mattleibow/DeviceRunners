namespace DeviceRunners.VisualRunners;

public class FileResultChannel : TextWriterResultChannel
{
	readonly string _filePath;

	public FileResultChannel(FileResultChannelOptions options)
		: base(options.Formatter ?? throw new ArgumentNullException(nameof(options), "Formatter is required."))
	{
		_filePath = options.FilePath ?? throw new ArgumentNullException(nameof(options), "FilePath is required.");
	}

	protected override TextWriter CreateWriter()
	{
		var dir = Path.GetDirectoryName(_filePath);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		return new StreamWriter(_filePath, append: false);
	}
}

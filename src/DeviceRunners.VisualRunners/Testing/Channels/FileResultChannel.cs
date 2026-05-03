namespace DeviceRunners.VisualRunners;

class FileResultChannel(FileResultChannelOptions options)
	: TextWriterResultChannel(options.Formatter ?? throw new ArgumentNullException(nameof(options), "Formatter is required."))
{
	readonly string _filePath = options.FilePath ?? throw new ArgumentNullException(nameof(options), "FilePath is required.");

	protected override TextWriter CreateWriter()
	{
		var dir = Path.GetDirectoryName(_filePath);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		return new StreamWriter(_filePath, append: false);
	}
}

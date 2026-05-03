namespace DeviceRunners.VisualRunners;

class FileResultChannel(FileResultChannelOptions options)
	: TextWriterResultChannel(options.Formatter)
{
	protected override TextWriter CreateWriter()
	{
		var dir = Path.GetDirectoryName(options.FilePath);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		return new StreamWriter(options.FilePath, append: false);
	}
}

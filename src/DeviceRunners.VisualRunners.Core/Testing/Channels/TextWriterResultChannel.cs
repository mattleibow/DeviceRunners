namespace DeviceRunners.VisualRunners;

public abstract class TextWriterResultChannel : IResultChannel
{
	readonly IResultChannelFormatter _formatter;

	readonly object _locker = new();

	TextWriter? _writer;

	public TextWriterResultChannel(IResultChannelFormatter formatter)
	{
		_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
	}

	public bool IsOpen => _writer is not null;

	protected abstract TextWriter CreateWriter();

	public Task<bool> OpenChannel(string? message = null)
	{
		lock (_locker)
		{
			_writer = CreateWriter();

			_formatter.BeginTestRun(_writer, message);
		
			_writer.Flush();
		}

		return Task.FromResult(true);
	}

	public void RecordResult(ITestResultInfo testResult)
	{
		lock (_locker)
		{
			if (_writer is null)
				return;

			_formatter.RecordResult(testResult);

			_writer.Flush();
		}
	}

	public Task CloseChannel()
	{
		lock (_locker)
		{
			if (_writer is not null)
			{
				_formatter.EndTestRun();

				_writer.Flush();
				_writer.Dispose();

				_writer = null;
			}
		}

		return Task.CompletedTask;
	}
}

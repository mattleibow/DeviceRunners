using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Xunit.Runner.Devices;

public class DiagnosticsViewModel : AbstractBaseViewModel
{
	readonly ITestRunner _runner;

	string _messagesString = "";

	public DiagnosticsViewModel(ITestRunner runner)
	{
		_runner = runner;
		_runner.DiagnosticMessageRecieved += OnDiagnosticMessageRecieved;

		Messages.CollectionChanged += OnDiagnosticMessagesCollectionChanged;

		ClearMessagesCommand = new Command(Clear);
	}

	public ICommand ClearMessagesCommand { get; }

	public ObservableCollection<string> Messages { get; } = new();

	public string MessagesString
	{
		get => _messagesString;
		private set => Set(ref _messagesString, value);
	}

	public void PostDiagnosticMessage(string message)
	{
		Messages.Add(message);
	}

	public void Clear()
	{
		Messages.Clear();
	}

	void OnDiagnosticMessageRecieved(object? sender, string message)
	{
		PostDiagnosticMessage(message);
	}

	void OnDiagnosticMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		// TODO: optimize
		MessagesString = string.Join(Environment.NewLine, Messages);
	}
}

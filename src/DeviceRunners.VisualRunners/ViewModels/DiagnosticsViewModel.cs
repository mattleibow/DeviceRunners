using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace DeviceRunners.VisualRunners;

public class DiagnosticsViewModel : AbstractBaseViewModel
{
	readonly IDiagnosticsManager _diagnosticsManager;

	string _messagesString = "";

	public DiagnosticsViewModel(IDiagnosticsManager diagnosticsManager)
	{
		_diagnosticsManager = diagnosticsManager;
		_diagnosticsManager.DiagnosticMessageReceived += OnDiagnosticMessageReceived;

		Messages.CollectionChanged += OnDiagnosticMessagesCollectionChanged;

		ClearMessagesCommand = new Command(ClearExecute);
	}

	public ICommand ClearMessagesCommand { get; }

	public ObservableCollection<string> Messages { get; } = new();

	public string MessagesString
	{
		get => _messagesString;
		private set => Set(ref _messagesString, value);
	}

	void ClearExecute()
	{
		Messages.Clear();
	}

	void OnDiagnosticMessageReceived(object? sender, string message)
	{
		Messages.Add(message);
		Console.WriteLine(message);
	}

	void OnDiagnosticMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		// TODO: optimize
		MessagesString = string.Join(Environment.NewLine, Messages);
	}
}

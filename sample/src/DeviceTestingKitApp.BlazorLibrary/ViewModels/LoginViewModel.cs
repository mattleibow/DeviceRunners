using System.ComponentModel;
using System.Windows.Input;

namespace DeviceTestingKitApp.ViewModels;

/// <summary>
/// Simple ViewModel for testing binding patterns — Blazor equivalent
/// of MAUI's TestViewModel used in UITests.
/// </summary>
public class LoginViewModel : INotifyPropertyChanged
{
	string? _username;
	string? _password;

	public event PropertyChangedEventHandler? PropertyChanged;
	public event Action? LoginExecuted;

	public string? Username
	{
		get => _username;
		set { _username = value; PropertyChanged?.Invoke(this, new(nameof(Username))); PropertyChanged?.Invoke(this, new(nameof(CanLogin))); }
	}

	public string? Password
	{
		get => _password;
		set { _password = value; PropertyChanged?.Invoke(this, new(nameof(Password))); PropertyChanged?.Invoke(this, new(nameof(CanLogin))); }
	}

	public bool CanLogin => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

	public ICommand LoginCommand => new RelayCommand(() => LoginExecuted?.Invoke());

	class RelayCommand(Action action) : ICommand
	{
		public event EventHandler? CanExecuteChanged;
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => action();
	}
}

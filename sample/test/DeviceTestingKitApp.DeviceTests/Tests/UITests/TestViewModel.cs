using System.Windows.Input;

namespace DeviceTestingKitApp.DeviceTests;

public class TestViewModel : BindableObject
{
	string? _username;
	string? _password;

	public TestViewModel()
	{
		LoginCommand = new Command(OnLogin, CanLogin);
	}

	public string? Username
	{
		get => _username;
		set
		{
			_username = value;
			OnPropertyChanged();
			((Command)LoginCommand).ChangeCanExecute();
		}
	}

	public string? Password
	{
		get => _password;
		set
		{
			_password = value;
			OnPropertyChanged();
			((Command)LoginCommand).ChangeCanExecute();
		}
	}

	public ICommand LoginCommand { get; }

	bool CanLogin() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

	void OnLogin()
	{
		// ...
	}
}

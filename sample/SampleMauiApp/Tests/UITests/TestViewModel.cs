using System.Windows.Input;

namespace SampleMauiApp;

public class TestViewModel : BindableObject
{
	string? _username;
	string? _password;

	public TestViewModel()
	{
		LoginCommand = new Command(OnLogin);
	}

	public string? Username
	{
		get => _username;
		set
		{
			_username = value;
			OnPropertyChanged();
		}
	}

	public string? Password
	{
		get => _password;
		set
		{
			_password = value;
			OnPropertyChanged();
		}
	}

	public ICommand LoginCommand { get; }

	void OnLogin()
	{
		// ...
	}
}

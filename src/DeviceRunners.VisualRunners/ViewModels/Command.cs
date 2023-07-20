using System.Windows.Input;

namespace DeviceRunners.VisualRunners;

sealed class Command<T> : Command
{
	public Command(Action<T?> execute)
		: base(o => ExecuteIfValid(o, execute))
	{
		_ = execute ?? throw new ArgumentNullException(nameof(execute));
	}

	public Command(Action<T?> execute, Func<T?, bool> canExecute)
		: base(o => ExecuteIfValid(o, execute), o => IsValidParameter(o) && canExecute((T?)o))
	{
		_ = execute ?? throw new ArgumentNullException(nameof(execute));
		_ = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
	}

	static void ExecuteIfValid(object? o, Action<T?> execute)
	{
		if (IsValidParameter(o))
			execute((T?)o);
	}

	static bool IsValidParameter(object? o)
	{
		// The parameter isn't null, so we don't have to worry whether null is a valid option
		if (o is not null)
			return o is T;

		var t = typeof(T);

		// The parameter is null. Is T Nullable?
		if (Nullable.GetUnderlyingType(t) != null)
			return true;

		// Not a Nullable, if it's a value type then null is not valid
		return !t.IsValueType;
	}
}

class Command : ICommand
{
	readonly Func<object?, bool>? _canExecute;
	readonly Action<object?> _execute;

	public Command(Action<object?> execute)
	{
		_execute = execute ?? throw new ArgumentNullException(nameof(execute));
	}

	public Command(Action execute)
		: this(o => execute())
	{
		_ = execute ?? throw new ArgumentNullException(nameof(execute));
	}

	public Command(Action<object?> execute, Func<object?, bool> canExecute)
		: this(execute)
	{
		_canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
	}

	public Command(Action execute, Func<bool> canExecute)
		: this(o => execute(), o => canExecute())
	{
		_ = execute ?? throw new ArgumentNullException(nameof(execute));
		_ = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
	}

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter)
	{
		if (_canExecute is not null)
			return _canExecute(parameter);
		return true;
	}

	public void Execute(object? parameter)
	{
		_execute(parameter);
	}

	public void ChangeCanExecute()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}

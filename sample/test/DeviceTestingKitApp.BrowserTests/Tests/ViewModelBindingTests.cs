using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.BrowserTests;

/// <summary>
/// Tests that verify ViewModel patterns work correctly in WASM.
/// Blazor equivalent of MAUI's UITests — tests ViewModel binding, 
/// ICommand, and INotifyPropertyChanged without MAUI-specific APIs.
/// </summary>
public class ViewModelBindingTests
{
	[Fact]
	public void PopulatedUsernameAndPasswordEnableLogin()
	{
		var vm = new LoginViewModel();
		vm.Username = "testuser";
		vm.Password = "P@55w0rd";

		Assert.True(vm.CanLogin);
	}

	[Fact]
	public void EmptyPasswordDisablesLogin()
	{
		var vm = new LoginViewModel();
		vm.Username = "testuser";
		vm.Password = "P@55w0rd";
		vm.Password = "";

		Assert.False(vm.CanLogin);
	}

	[Fact]
	public void PropertyChangedFires()
	{
		var vm = new LoginViewModel();
		var changedProperties = new List<string?>();
		vm.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

		vm.Username = "test";

		Assert.Contains(nameof(LoginViewModel.Username), changedProperties);
		Assert.Contains(nameof(LoginViewModel.CanLogin), changedProperties);
	}

	[Fact]
	public void CommandExecutes()
	{
		var vm = new LoginViewModel();
		vm.Username = "user";
		vm.Password = "pass";

		bool executed = false;
		vm.LoginExecuted += () => executed = true;
		vm.LoginCommand.Execute(null);

		Assert.True(executed);
	}

	[Theory]
	[InlineData("hello", "HELLO")]
	[InlineData("woRld", "WORLD")]
	public void TextTransformUpperCase(string input, string expected)
	{
		// Blazor equivalent of MAUI's EntryHandler text transform test
		Assert.Equal(expected, input.ToUpperInvariant());
	}

	[Theory]
	[MemberData(nameof(GetTransformData))]
	public void ComplexTheoryWithMemberData(string input, string expected)
	{
		Assert.Equal(expected, input.ToUpperInvariant());
	}

	public static IEnumerable<object[]> GetTransformData()
	{
		yield return ["hello", "HELLO"];
		yield return ["woRld", "WORLD"];
	}

	[Theory]
	[ClassData(typeof(TransformTestData))]
	public void ComplexTheoryWithClassData(TransformData data)
	{
		Assert.Equal(data.Expected, data.Input.ToUpperInvariant());
	}

	public record TransformData(string Input, string Expected);

	class TransformTestData : IEnumerable<object[]>
	{
		readonly List<object[]> _data =
		[
			[new TransformData("hello", "HELLO")],
			[new TransformData("woRld", "WORLD")],
		];

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}



using Bunit;
using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

public class LoginComponentTests : Bunit.TestContext
{
	[Fact]
	public void PopulatedUsernameAndPasswordEnableLogin()
	{
		var vm = new LoginViewModel
		{
			Username = "mattleibow",
			Password = "P@55w0rd"
		};

		var cut = RenderComponent<LoginComponent>(p => p.Add(c => c.ViewModel, vm));

		var loginButton = cut.Find("#loginButton");
		Assert.False(loginButton.HasAttribute("disabled"));
	}

	[Fact]
	public void EmptyUsernameDisablesLogin()
	{
		var vm = new LoginViewModel
		{
			Username = "",
			Password = "P@55w0rd"
		};

		var cut = RenderComponent<LoginComponent>(p => p.Add(c => c.ViewModel, vm));

		Assert.True(cut.Find("#loginButton").HasAttribute("disabled"));
	}

	[Fact]
	public void RemovingPasswordDisablesLogin()
	{
		var vm = new LoginViewModel
		{
			Username = "mattleibow",
			Password = "P@55w0rd"
		};

		var cut = RenderComponent<LoginComponent>(p => p.Add(c => c.ViewModel, vm));
		Assert.False(cut.Find("#loginButton").HasAttribute("disabled"));

		vm.Password = "";
		cut.Render();

		Assert.True(cut.Find("#loginButton").HasAttribute("disabled"));
	}

	[Fact]
	public void ClickingLoginButtonInvokesCommand()
	{
		var vm = new LoginViewModel
		{
			Username = "mattleibow",
			Password = "P@55w0rd"
		};
		bool executed = false;
		vm.LoginExecuted += () => executed = true;

		var cut = RenderComponent<LoginComponent>(p => p.Add(c => c.ViewModel, vm));
		cut.Find("#loginButton").Click();

		Assert.True(executed);
	}
}

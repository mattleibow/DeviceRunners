using System.Collections;

namespace SampleMauiApp;

class TestThing
{
	public int Number;
}

public class TestDataGenerator : IEnumerable<object[]>
{
	private readonly List<object[]> _data = new List<object[]>
	{
		new object[] {new TestThing{Number=1}},
		new object[] {new TestThing{Number=2}}
	};

	public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class UITests
{
	[UIFact]
	public async Task SuccessfulTest()
	{
		Routing.RegisterRoute("testing", typeof(TestPage));

		await Shell.Current.GoToAsync("testing");

		var page = Shell.Current.CurrentPage;
		var l = page.IsLoaded;
		if (!l)
			page.Loaded += (s, e) =>
			{
				l = true;
			};

		await Shell.Current.GoToAsync("..");
	}

	[UITheory]
	[ClassData(typeof(TestDataGenerator))]
	public async Task SuccessfulTestTTT(int something)
	{
		Routing.RegisterRoute("testing" + something, typeof(TestPage));

		await Shell.Current.GoToAsync("testing" + something);

		var page = Shell.Current.CurrentPage;
		var l = page.IsLoaded;
		if (!l)
			page.Loaded += (s, e) =>
			{
				l = true;
			};

		await Shell.Current.GoToAsync("..");
	}
}

There are 3 types of tests:

* [Plain Unit Tests](types-of-tests.md#plain-unit-tests)
* [On-Device Tests](types-of-tests.md#on-device-testing-using-devicerunners)
* [UI Automation Tests](types-of-tests.md#ui-automation-tests)

## Plain Unit Tests

We have many tests that are just testing code that is not on a device. It all runs on the host. For example, our XAML unit tests. These tests are written using Xunit and run in VS/CLI and do _not_ have any app related code. 

I do this in: https://github.com/mattleibow/DeviceRunners/blob/main/sample/SampleXunitTestProject/UnitTests.cs

For example, you might use this to test to see if your VM and/or XAML page updates when some event happens. For a more concrete example, you could have a page that shows a list of monkeys, a refresh button and a loading indicator. When your page is ready, you can write a plain test to trigger the refresh command and then you can observe the xaml loads correctly, the indicator is shown and then when data is loaded you can observe the items:

**MonkeysPageTests.cs**

```cs
TestDataSource _data;
MonkeysPage _page;
ViewModel _vm;

public MonkeysPageTests()
{
    // TestDataSource is a special source that blocks until released...
    _data = new TestDataSource();

    // setup
    _page = new MonkeysPage();
    _vm = new ViewModel(_data);
    _page.BindingContext = _vm;
}

[Fact]
public void TestLoadingWorksCorrectly()
{
    // test the initial state
    Assert.True(_page.FindControl("RefreshButton").IsEnabled);
    Assert.False(_page.FindControl("LoadingPopup").IsVisible);
    Assert.Empty(((IVisualTreeElement)_page.FindControl("MonkeysList")).GetChildren());

    // trigger a refresh
    _vm.RefreshCommand.Execute();

    // test the loading state
    Assert.False(_page.FindControl("RefreshButton").IsEnabled);
    Assert.True(_page.FindControl("LoadingPopup").IsVisible);
    Assert.Empty(((IVisualTreeElement)_page.FindControl("MonkeysList")).GetChildren());

    // allow the loading to return
    _data.Continue();

    // test the finalstate
    Assert.True(_page.FindControl("RefreshButton").IsEnabled);
    Assert.False(_page.FindControl("LoadingPopup").IsVisible);
    Assert.NotEmpty(((IVisualTreeElement)_page.FindControl("MonkeysList")).GetChildren());
}
```

## On-Device Testing (using DeviceRunners)

On-device testing is very much like the plain tests, except instead of running on the host/dev machine, it runs on the device. This is what the DeviceRunners repository solves. These tests are run in the context of a mobile app - in the current state we just have a .NET Maui app. This is _not_ your shipping app, but rather a special test runner app. It provides a visual runner shell as well as some hooks to run from the CLI using XHarness. I have lots more info on the wiki: https://github.com/mattleibow/DeviceRunners/wiki

For plain tests running in the context of a device, I have an example: https://github.com/mattleibow/DeviceRunners/blob/main/sample/SampleMauiApp/Tests/UnitTests.cs

The reason this test run is better than plain tests is that it runs closer to the intended target. It is just as fast (or very close). Another benefit is that native controls will be instantiated so you can detect a crash or hang due to UI thread operations or some other situation.

The current test runner is using the [.NET MAUI Shell](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/) as a host, so you will have to work in the context of that. If there is demand, we can always create a more "you bring your own app" runner as the MAUI part of the runner is super thin.

For UI-based tests, I have an example: https://github.com/mattleibow/DeviceRunners/blob/main/sample/SampleMauiApp/Tests/UITests/TestPageUITests.cs
And for the setup, I have simple code: https://github.com/mattleibow/DeviceRunners/blob/main/sample/SampleMauiApp/Tests/UITests/UITests.cs

If we follow on from the example above, we can run the same test on the device, but maybe for the setup you would first push the page:

**MonkeysPageTests.cs**

```cs
TestDataSource _data;
ViewModel _vm;
MonkeysPage _page;

public MonkeysPageTests()
{
    // TestDataSource is a special source that blocks until released...
    _data = new TestDataSource();

    // seup
    _vm = new ViewModel(_data);
}

public async Task InitializeAsync()
{
    // register route and navigate to test page
    Routing.RegisterRoute("uitests", typeof(MonkeysPage));
    await Shell.Current.GoToAsync("uitests");

    // get the page
    _page = (MonkeysPage)Shell.Current.CurrentPage;

    // connect
    _page.BindingContext = _vm;
}

public async Task DisposeAsync()
{
    _page = null!;

    // navigate back and unregister route
    await Shell.Current.GoToAsync("..");
    Routing.UnRegisterRoute("uitests");
}

[Fact]
public void TestLoadingWorksCorrectly()
{
    // ... the same test as above ...
}
```

## UI Automation Tests

The third way of writing device tests is to use the UI automation provided by various tools or platforms. For example, you would write a test following the way a human would interact. For example, Appium.

This tends to be a bit slower, but does offer some advantages: the app being tested is the final app.

I am still learning how I would write tests in this way, but here is some pseudo code:

```cs
// assume the app is running and is on the correct page

// test the initial state
var refresh = await App.FindElementMarked("RefreshButton");
Assert.NotNull(refresh);
var loading = await App.FindElementMarked("LoadingPopup");
Assert.Null(loading);
var firstlistItem = await App.FindElementMarked("ListItem");
Assert.Null(firstlistItem);

// start the data load
await refresh.Tap();

// data is now being loaded...

// waiting for the element will throw if it does not appear
loading = await App.WaitForElementMarked("LoadingPopup");

// waiting for the element to disappear will throw if it stays visible
await App.WaitForNoElementMarked("LoadingPopup");

// wait until the list has items
firstlistItem = await App.WaitForElementMarked("ListItem");
```

## Sample Page & View Model

**ViewModel.cs**

```cs
class ViewModel
{
    IDataSource _data;

    public ViewModel(IDataSource data)
    {
        _data = data;
        RefreshCommand = new(DoRefresh, () => IsBusy);
    }

    public bool IsBusy { get; set; }
    public Command RefreshCommand { get; }
    public ObservableCollection<IDataItem> Items { get; } = new();

    async void DoRefresh()
    {
        IsBusy = true;

        var loadedData = await _data.LoadDataAsync();
        Items.Clear();
        Items.AddRange(loadedData);

        IsBusy = false;
    }
}
```

**MonkeysPage.xaml**

```xaml
<ContentPage>
    <Grid RowDefinitions="Auto,*">
        <Button Text="Refresh" Command="{Binding RefreshCommand}" AutomationId="RefreshButton" />
        <CollectionView ItemsSource="{Binding Items}" Grid.Row="1" AutomationId="MonkeysList">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Label Text="{Binding Name}" AutomationId="ListItem" />
                </DataTemplate>
            <CollectionView.ItemTemplate>
        </CollectionView>
        <ActivityIndicator IsVisible="{Binding IsBusy}" Grid.RowSpan="2" AutomationId="LoadingPopup" />
    <Grid>
</ContentPage>
```
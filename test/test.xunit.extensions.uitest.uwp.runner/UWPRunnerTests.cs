using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xunit;



public class UWPRunnerTests
{
    [UIFact]
    public async void DoSomethingOnUIThread()
    {
        var ele = new SearchBox();
        ele.Visibility = Visibility.Collapsed;

        ele.QueryText = "foo";

        Assert.Equal("foo", ele.QueryText);

        await Task.Delay(20);

        ele.QueryText = "bar";
        Assert.Equal("bar", ele.QueryText);
    }

    [UITheory]
    [InlineData("foo")]
    [InlineData("FooBar")]
    [InlineData("Bar")]
    public void UITheory(string value)
    {
        var ele = new SearchBox();
        ele.Visibility = Visibility.Collapsed;
        ele.QueryText = value;


        Assert.Equal(3, ele.QueryText.Length);
    }

    [Fact]
    public void SomethingNotOnUIThreadThrows()
    {
        Assert.Throws<Exception>(() =>
                                 {
                                     var ele = new SearchBox();
                                     ele.Visibility = Visibility.Collapsed;
                                 });
        
    }

}

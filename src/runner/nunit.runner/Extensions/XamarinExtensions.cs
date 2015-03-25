using NUnit.Framework.Interfaces;
using Xamarin.Forms;

namespace NUnit.Runner.Extensions
{
    internal static class XamarinExtensions
    {
        /// <summary>
        /// Gets the color to display for the test status
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Color Color(this ITestResult result)
        {
            switch (result.ResultState.Status)
            {
                case TestStatus.Passed:
                    return Xamarin.Forms.Color.Green;
                case TestStatus.Skipped:
                    return Xamarin.Forms.Color.Yellow;
                case TestStatus.Failed:
                    if (result.ResultState == ResultState.Failure)
                        return Xamarin.Forms.Color.Red;
                    if (result.ResultState == ResultState.NotRunnable)
                        return Xamarin.Forms.Color.FromRgb(255, 106, 0);  // Dark Red

                    return Xamarin.Forms.Color.FromRgb(170, 0, 0); // Dark Red
                    
                case TestStatus.Inconclusive:
                default:
                    return Xamarin.Forms.Color.Gray;
            }
        }
    }
}
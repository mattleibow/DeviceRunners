namespace DeviceTestingKitApp.Formatters;

/// <summary>
/// Formats counter values for display — Blazor equivalent of MAUI's CounterValueConverter.
/// </summary>
public static class CounterValueFormatter
{
    public static string Format(int count) => count switch
    {
        0 => "Click me!",
        1 => $"Clicked {count} time",
        _ => $"Clicked {count} times"
    };
}

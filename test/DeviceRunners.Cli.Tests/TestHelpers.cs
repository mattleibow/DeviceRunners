using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public static class TestHelpers
{
    public static bool IsValidJson(string jsonString)
    {
        try
        {
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
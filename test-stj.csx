using System.Text.Json;

var obj = new { Name = "テスト🎉émojis", Status = "passed" };
var json = JsonSerializer.Serialize(obj);
Console.WriteLine(json);
Console.WriteLine("All ASCII: " + json.All(c => c < 128));

using System.Text.Json;

namespace Shared.Utils
{
    public class JsonManagerSerialize
    {
        public static string Serialize<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            return JsonSerializer.Serialize(obj, options);
        }

        public static T? Deserialize<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
using System.Text;
using System.Text.Json;

namespace Catan.Shared.Networking.Serialization;

public static class JsonMessageSerializer
{
    public static byte[] Serialize<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var payload = Encoding.UTF8.GetBytes(json);

        var lengthPrefix = BitConverter.GetBytes(payload.Length);
        return lengthPrefix.Concat(payload).ToArray();
    }

    public static T Deserialize<T>(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}

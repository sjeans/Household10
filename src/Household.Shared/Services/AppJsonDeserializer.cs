using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Household.Shared.Services;

public sealed class AppJsonDeserializer : IAppJsonDeserializer
{
    private readonly JsonSerializer _jsonSerializer;

    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    public AppJsonDeserializer()
    {
        _jsonSerializer = JsonSerializer.Create(Settings);
    }

    public async Task<Result<T>> TryDeserializeAsync<T>(Stream streamContent, CancellationToken cancellationToken)
    {
        try
        {
            T? result = await DeserializeAsync<T>(streamContent, cancellationToken);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            string typeName = typeof(T).Name;
            return Result<T>.Failure($"Type: {typeName}; Error: {ex.Message}");
        }
    }

    public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken)
    {
        // leaveOpen: true prevents disposing the underlying stream if you manage it outside
        using StreamReader reader = new(stream, leaveOpen: true);
        using JsonTextReader jsonReader = new(reader);

        // JsonTextReader has no async deserialization, so read async at the StreamReader level
        await jsonReader.ReadAsync(cancellationToken);

        T? result = _jsonSerializer.Deserialize<T>(jsonReader);

        if (result is null)
            throw new JsonSerializationException($"Failed to deserialize {typeof(T).Name}");

        return result;
    }

    public Task<T> DeserializeAsync<T>(string json, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            using StringReader reader = new StringReader(json);
            using JsonTextReader jsonReader = new JsonTextReader(reader);

            T? result = _jsonSerializer.Deserialize<T>(jsonReader);

            if (result is null)
                throw new JsonSerializationException($"Failed to deserialize {typeof(T).Name}");

            return result; // safe now, compiler sees result is T
        }, cancellationToken);
    }
}

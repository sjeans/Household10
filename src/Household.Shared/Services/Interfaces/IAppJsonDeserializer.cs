using Household.Shared.Helpers;

namespace Household.Shared.Services.Interfaces;

public interface IAppJsonDeserializer
{
    Task<Result<T>> TryDeserializeAsync<T>(Stream streamContent, CancellationToken cancellationToken);
    Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);
    Task<T> DeserializeAsync<T>(string json, CancellationToken cancellationToken);
}

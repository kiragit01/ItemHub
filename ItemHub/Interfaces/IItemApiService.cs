using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IItemApiService
{
    Task<Result<bool>> TogglePublishItemAsync(Guid itemId);
    bool DeleteImage(Guid itemId, string fileName);
    List<string?> GetSavedImages(Guid itemId);
}

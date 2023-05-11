using Blazored.LocalStorage;

namespace BlazorAuth;

public class ClientStorageService : IClientStorageService {
    private ILocalStorageService StorageService { get; }
    private string ItemsPrefix { get; }

    public ClientStorageService(ILocalStorageService storageService, string itemsPrefix) {
        StorageService = storageService;
        ItemsPrefix = itemsPrefix;
    }

    public async Task SetItemAsync(string key, object value) {
        await StorageService.SetItemAsync($"{ItemsPrefix}.{key}", value);
    }

    public async Task<T> GetItemAsync<T>(string key) {
        var complexKey = $"{ItemsPrefix}.{key}";
        return await StorageService.GetItemAsync<T>(complexKey);
    }

    public async Task DeleteItemAsync(string key) {
        await StorageService.RemoveItemAsync($"{ItemsPrefix}.{key}");
    }
}
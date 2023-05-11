namespace BlazorAuth;

public interface IClientStorageService {
    Task SetItemAsync(string key, object value);
    Task<T> GetItemAsync<T>(string key);
    Task DeleteItemAsync(string key);
}

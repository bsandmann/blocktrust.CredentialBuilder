namespace Blocktrust.CredentialBuilder.Client.Services;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using FluentResults;
using Helpers;
using Microsoft.JSInterop;

public class StorageService : IStorageService
{
    private readonly ILocalStorageService _blazoredLocalStorage;

    public StorageService(ILocalStorageService blazoredLocalStorage)
    {
        _blazoredLocalStorage = blazoredLocalStorage;
    }

    public async Task Clear()
    {
        await _blazoredLocalStorage.ClearAsync();
    }

    public async Task RemoveItem<T>()
    {
        var tName = typeof(T).Name.ToUpperInvariant();
        await _blazoredLocalStorage.RemoveItemAsync(tName);
    }

    public async Task<Result<T>> GetItem<T>()
    {
        var tName = typeof(T).Name.ToUpperInvariant();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        
        var localStorageString = await _blazoredLocalStorage.GetItemAsStringAsync(tName);
        if (string.IsNullOrEmpty(localStorageString))
        {
            return Result.Ok();
        }

        T? deserializedObject;
        try
        {
            deserializedObject = JsonSerializer.Deserialize<T>(localStorageString.ToString(), options);
        }
        catch (Exception e)
        {
            return Result.Fail($"Failed to deserialize: {e.Message}");
        }

        if (deserializedObject is null)
        {
            //
            return Result.Fail($"null?");
        }

        return Result.Ok(deserializedObject);
    }

    public async Task<Result> SetItem<T>(T t)
    {
        string jsonString;
        var tName = typeof(T).Name.ToUpperInvariant();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };
        try
        {
            jsonString = JsonSerializer.Serialize(t, options);
        }
        catch (Exception e)
        {
            return Result.Fail($"Failed to serialize: {e.Message}");
        }

        try
        {
            await _blazoredLocalStorage.SetItemAsStringAsync(tName, jsonString);
        }
        catch (Exception e)
        {
            return Result.Fail($"Error writing to chrome.storage.local with key '{tName}': {e.Message}");
        }

        return Result.Ok();
    }
}
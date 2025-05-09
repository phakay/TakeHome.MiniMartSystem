﻿using System.Text;
using System.Text.Json;

namespace MiniMart.Infrastructure.Services
{
    public abstract class BaseApiClient
    {
        private readonly HttpClient _httpClient;
        private static JsonSerializerOptions _jsonSerializerOptions => new() { PropertyNameCaseInsensitive = true };

        public BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions) ?? 
                throw new InvalidOperationException("Unable to Deserialize content");
        }

        protected async Task<T> PostAsync<T>(string url, object payload)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions) ?? 
                throw new InvalidOperationException("Unable to Deserialize content");
        }
    }
}



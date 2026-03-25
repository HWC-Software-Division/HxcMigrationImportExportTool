using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Services
{
    public class XbykApiService
    {
        private readonly HttpClient _http;

        public XbykApiService(string baseUrl, string apiKey)
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> CreateContentTypeAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/migrate/content-type", content);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> ImportLocalStringsAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/migrate/local-string", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"LocalString API failed. Status: {(int)response.StatusCode}, Response: {responseBody}");
            }

            return responseBody;
        }
    }
}
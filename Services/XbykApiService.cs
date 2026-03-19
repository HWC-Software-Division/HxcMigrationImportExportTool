using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Http; 
using System.Text.Json;

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
    }
}

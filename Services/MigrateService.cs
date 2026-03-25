using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Services
{
    public class MigrateService
    {
        private readonly XbykApiService _api;

        public MigrateService(XbykApiService api)
        {
            _api = api;
        }

        private object MapToContentType(K13PageType pt)
        {
            return new
            {
                name = pt.DisplayName,
                codeName = pt.ClassName,
                fields = pt.Fields.Select(f => new
                {
                    name = f.Column,
                    dataType = MapDataType(f.DataType),
                    isRequired = f.Required,
                    size = GetFieldSize(f),
                    defaultValue = f.DefaultValue,
                    fieldType = MapFormControl(f),
                    caption = f.Caption ?? f.Column,
                    dataSource = f.DataSource
                }).ToList()
            };
        }

        private object MapToLocalString(K13ResourceString resource)
        {
            return new
            {
                id = resource.Id,
                key = resource.Key,
                description = resource.Description,
                values = (resource.Values ?? new Dictionary<string, string>())
                    .Select(x => new
                    {
                        language = x.Key,
                        value = x.Value
                    })
                    .ToList()
            };
        }

        private string MapDataType(string type)
        {
            return type?.ToLower() switch
            {
                "text" => "text",
                "longtext" => "richText",
                "integer" => "number",
                "longinteger" => "number",
                "double" => "number",
                "decimal" => "number",
                "boolean" => "boolean",
                "guid" => "guid",
                "datetime" => "dateTime",
                "date" => "date",
                "file" => "mediaFiles",
                "attachment" => "mediaFiles",
                "pages" => "pages",
                "taxonomy" => "taxonomy",
                _ => "text"
            };
        }

        private string MapFieldType(string type)
        {
            return type?.ToLower() switch
            {
                "text" => "textbox",
                "longtext" => "textarea",
                "boolean" => "checkbox",
                "integer" => "textbox",
                "datetime" => "datetime",
                _ => "textbox"
            };
        }

        private string MapFormControl(K13Field f)
        {
            var control = f.FormControl?.ToLower();

            return control switch
            {
                "media selection" => "media",
                "file selector" => "media",
                "image selector" => "media",
                "textbox" => "textbox",
                "textarea" => "textarea",
                "dropdownlist" => "dropdown",
                "checkbox" => "checkbox",
                _ => "textbox"
            };
        }

        private int GetFieldSize(K13Field f)
        {
            if (f.Size > 0)
            {
                return f.Size;
            }

            return f.DataType.ToLower() switch
            {
                "text" => 200,
                "longtext" => 1000,
                "integer" => 0,
                _ => 200
            };
        }

        public async Task<(int success, int fail, int skip)> MigratePageTypesAsync(List<K13PageType> pageTypes)
        {
            int success = 0;
            int fail = 0;
            int skip = 0;

            foreach (var pt in pageTypes)
            {
                try
                {
                    var payload = MapToContentType(pt);
                    var result = await _api.CreateContentTypeAsync(payload);

                    if (result.Contains("Already exists"))
                    {
                        skip++;
                    }
                    else if (result.Contains("created"))
                    {
                        success++;
                        Logger.Log($"Created: {pt.ClassName}");
                    }
                    else
                    {
                        fail++;
                        Logger.Log($"Failed: {pt.ClassName}");
                    }
                }
                catch (Exception ex)
                {
                    fail++;
                    Logger.Log($"Error: {pt.ClassName} - {ex.Message}");
                }
            }

            return (success, fail, skip);
        }

        public async Task<(int success, int fail)> MigrateLocalStringsAsync(List<K13ResourceString> resources)
        {
            int success = 0;
            int fail = 0;

            if (resources == null || resources.Count == 0)
            {
                return (0, 0);
            }

            try
            {
                var payload = resources
                    .Select(MapToLocalString)
                    .ToList();

                var resultJson = await _api.ImportLocalStringsAsync(payload);

                Logger.Log("LocalString API response:");
                Logger.Log(resultJson);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = System.Text.Json.JsonSerializer.Deserialize<LocalStringBatchApiResponse>(resultJson, options);

                if (result != null)
                {
                    success = result.TotalKeysProcessed;
                    fail = result.Errors?.Count ?? 0;
                }
                else
                {
                    fail = resources.Count;
                }
            }
            catch (Exception ex)
            {
                fail = resources.Count;
                Logger.Log($"Error sending local strings: {ex}");
            }

            return (success, fail);
        }
    }
}
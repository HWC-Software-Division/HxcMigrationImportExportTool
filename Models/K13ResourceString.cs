using System;
using System.Collections.Generic;
using System.Text;

namespace HxcMigrationImportExportTool.Models
{
    public class K13ResourceString
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, string> Values { get; set; } = new();
    }

    public class K13ResourceTranslation
    {
        public int TranslationId { get; set; }
        public int StringId { get; set; }
        public int CultureId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class K13Culture
    {
        public int Id { get; set; }
        public string CodeName { get; set; } = string.Empty;
    }

    public class LocalStringBatchApiResponse
    {
        public int TotalKeysProcessed { get; set; }
        public int TotalTranslationsProcessed { get; set; }
        public int TotalTranslationsCreated { get; set; }
        public int TotalTranslationsUpdated { get; set; }
        public List<LocalStringApiItemResult> Items { get; set; } = new();
        public List<LocalStringApiError> Errors { get; set; } = new();
    }

    public class LocalStringApiItemResult
    {
        public string Key { get; set; } = string.Empty;
        public int KeyItemId { get; set; }
        public bool KeyCreated { get; set; }
        public int TranslationsProcessed { get; set; }
        public int TranslationsCreated { get; set; }
        public int TranslationsUpdated { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    public class LocalStringApiError
    {
        public string Key { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

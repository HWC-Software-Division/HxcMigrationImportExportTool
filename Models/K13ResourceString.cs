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
}

using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HxcMigrationImportExportTool.Parsers
{
    public class ResourceStringParser
    {
        public List<K13ResourceString> Parse(string xmlPath)
        {
            var result = new List<K13ResourceString>();

            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                Logger.Log("ResourceStringParser: xmlPath is null or empty");
                return result;
            }

            if (!File.Exists(xmlPath))
            {
                Logger.Log($"ResourceStringParser: file not found => {xmlPath}");
                return result;
            }

            Logger.Log($"ResourceStringParser: loading resource XML => {xmlPath}");

            var resourceDoc = XDocument.Load(xmlPath);

            var rootFolder = GetSearchRootFolder(xmlPath);
            var objectTranslationPath = Directory
                .GetFiles(rootFolder, "objecttranslation.xml.export", SearchOption.AllDirectories)
                .FirstOrDefault();

            Logger.Log($"ResourceStringParser: search root => {rootFolder}");

            if (string.IsNullOrWhiteSpace(objectTranslationPath) || !File.Exists(objectTranslationPath))
            {
                Logger.Log("ResourceStringParser: objecttranslation.xml.export not found");
            }
            else
            {
                Logger.Log($"ResourceStringParser: object translation XML found => {objectTranslationPath}");
            }

            var cultures = LoadCultures(objectTranslationPath);
            var cultureMap = cultures.ToDictionary(x => x.Id, x => x.CodeName);

            Logger.Log($"ResourceStringParser: culture count => {cultures.Count}");

            var resourceStrings = resourceDoc
                .Descendants()
                .Where(x => x.Name.LocalName == "cms_resourcestring")
                .Select(x => new K13ResourceString
                {
                    Id = ToInt(x.Elements().FirstOrDefault(e => e.Name.LocalName == "StringID")?.Value),
                    Key = x.Elements().FirstOrDefault(e => e.Name.LocalName == "StringKey")?.Value ?? string.Empty,
                    Description = x.Elements().FirstOrDefault(e => e.Name.LocalName == "StringDescription")?.Value
                })
                .Where(x => x.Id > 0 && !string.IsNullOrWhiteSpace(x.Key))
                .ToList();

            Logger.Log($"ResourceStringParser: resources found => {resourceStrings.Count}");

            var resourceLookup = resourceStrings.ToDictionary(x => x.Id, x => x);

            var translations = resourceDoc
                .Descendants()
                .Where(x => x.Name.LocalName == "cms_resourcetranslation")
                .Select(x => new K13ResourceTranslation
                {
                    TranslationId = ToInt(x.Elements().FirstOrDefault(e => e.Name.LocalName == "TranslationID")?.Value),
                    StringId = ToInt(x.Elements().FirstOrDefault(e => e.Name.LocalName == "TranslationStringID")?.Value),
                    CultureId = ToInt(x.Elements().FirstOrDefault(e => e.Name.LocalName == "TranslationCultureID")?.Value),
                    Value = x.Elements().FirstOrDefault(e => e.Name.LocalName == "TranslationText")?.Value ?? string.Empty
                })
                .Where(x => x.StringId > 0)
                .ToList();

            Logger.Log($"ResourceStringParser: translations found => {translations.Count}");

            foreach (var translation in translations)
            {
                if (!resourceLookup.TryGetValue(translation.StringId, out var resourceString))
                {
                    Logger.Log($"ResourceStringParser: TranslationStringID not found => {translation.StringId}");
                    continue;
                }

                string cultureCode;
                if (!cultureMap.TryGetValue(translation.CultureId, out cultureCode!))
                {
                    cultureCode = $"culture-{translation.CultureId}";
                    Logger.Log($"ResourceStringParser: culture fallback used => {cultureCode}");
                }

                if (resourceString.Values == null)
                {
                    resourceString.Values = new Dictionary<string, string>();
                }

                resourceString.Values[cultureCode] = translation.Value;
            }

            result = resourceStrings;

            Logger.Log($"ResourceStringParser: total parsed => {result.Count}");

            return result;
        }

        private List<K13Culture> LoadCultures(string? objectTranslationPath)
        {
            var result = new List<K13Culture>();

            if (string.IsNullOrWhiteSpace(objectTranslationPath))
            {
                return result;
            }

            if (!File.Exists(objectTranslationPath))
            {
                return result;
            }

            try
            {
                var doc = XDocument.Load(objectTranslationPath);

                result = doc
                    .Descendants()
                    .Where(x => x.Name.LocalName == "objecttranslation")
                    .Where(x =>
                        string.Equals(
                            x.Elements().FirstOrDefault(e => e.Name.LocalName == "ClassName")?.Value,
                            "cms_culture",
                            StringComparison.OrdinalIgnoreCase))
                    .Select(x => new K13Culture
                    {
                        Id = ToInt(x.Elements().FirstOrDefault(e => e.Name.LocalName == "ID")?.Value),
                        CodeName = x.Elements().FirstOrDefault(e => e.Name.LocalName == "CodeName")?.Value ?? string.Empty
                    })
                    .Where(x => x.Id > 0 && !string.IsNullOrWhiteSpace(x.CodeName))
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"ResourceStringParser: LoadCultures error => {ex.Message}");
            }

            return result;
        }

        private string GetSearchRootFolder(string resourceXmlPath)
        {
            var fileInfo = new FileInfo(resourceXmlPath);
            var current = fileInfo.Directory;

            while (current != null)
            {
                var dataFolder = Path.Combine(current.FullName, "Data");
                if (Directory.Exists(dataFolder))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return fileInfo.DirectoryName ?? AppDomain.CurrentDomain.BaseDirectory;
        }

        private int ToInt(string? value)
        {
            return int.TryParse(value, out var parsed) ? parsed : 0;
        }
    }
}
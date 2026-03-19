using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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

            XDocument resourceDoc;
            try
            {
                resourceDoc = LoadSanitizedXml(xmlPath);
            }
            catch (Exception ex)
            {
                Logger.Log($"ResourceStringParser: failed to load resource XML => {ex.Message}");
                throw;
            }

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
                var doc = LoadSanitizedXml(objectTranslationPath);

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

        private XDocument LoadSanitizedXml(string path)
        {
            string raw;

            using (var reader = new StreamReader(path, detectEncodingFromByteOrderMarks: true))
            {
                raw = reader.ReadToEnd();
            }

            Logger.Log($"ResourceStringParser: loading file => {path}");
            Logger.Log($"ResourceStringParser: raw length => {raw.Length}");

            // ลบตัวที่เจอบ่อยแบบตรง ๆ ก่อน
            raw = raw.Replace("\u000B", string.Empty);

            var cleaned = RemoveInvalidXmlChars(raw);

            Logger.Log($"ResourceStringParser: cleaned length => {cleaned.Length}");
            Logger.Log($"ResourceStringParser: index of 0x0B after clean => {cleaned.IndexOf('\u000B')}");

            var settings = new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore
            };

            using var stringReader = new StringReader(cleaned);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            return XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace);
        }

        private string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            var removedCount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                // ลบทิ้งตรง ๆ สำหรับตัวที่ทำให้ล่มบ่อย
                if (ch == '\u0000' || ch == '\u0001' || ch == '\u0002' || ch == '\u0003' ||
                    ch == '\u0004' || ch == '\u0005' || ch == '\u0006' || ch == '\u0007' ||
                    ch == '\u0008' || ch == '\u000B' || ch == '\u000C' ||
                    ch == '\u000E' || ch == '\u000F' || ch == '\u0010' || ch == '\u0011' ||
                    ch == '\u0012' || ch == '\u0013' || ch == '\u0014' || ch == '\u0015' ||
                    ch == '\u0016' || ch == '\u0017' || ch == '\u0018' || ch == '\u0019' ||
                    ch == '\u001A' || ch == '\u001B' || ch == '\u001C' || ch == '\u001D' ||
                    ch == '\u001E' || ch == '\u001F')
                {
                    removedCount++;

                    if (removedCount <= 20)
                    {
                        Logger.Log($"ResourceStringParser: removed control char 0x{((int)ch):X4}");
                    }

                    continue;
                }

                if (char.IsHighSurrogate(ch))
                {
                    if (i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    {
                        int codePoint = char.ConvertToUtf32(ch, text[i + 1]);

                        if (IsValidXmlCodePoint(codePoint))
                        {
                            sb.Append(ch);
                            sb.Append(text[i + 1]);
                        }
                        else
                        {
                            removedCount++;

                            if (removedCount <= 20)
                            {
                                Logger.Log($"ResourceStringParser: removed invalid surrogate code point 0x{codePoint:X}");
                            }
                        }

                        i++;
                        continue;
                    }

                    removedCount++;
                    continue;
                }

                if (char.IsLowSurrogate(ch))
                {
                    removedCount++;
                    continue;
                }

                if (XmlConvert.IsXmlChar(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    removedCount++;

                    if (removedCount <= 20)
                    {
                        Logger.Log($"ResourceStringParser: removed invalid char 0x{((int)ch):X4}");
                    }
                }
            }

            Logger.Log($"ResourceStringParser: total invalid XML chars removed = {removedCount}");

            return sb.ToString();
        }

        private bool IsValidXmlCodePoint(int codePoint)
        {
            return codePoint == 0x9
                   || codePoint == 0xA
                   || codePoint == 0xD
                   || (codePoint >= 0x20 && codePoint <= 0xD7FF)
                   || (codePoint >= 0xE000 && codePoint <= 0xFFFD)
                   || (codePoint >= 0x10000 && codePoint <= 0x10FFFF);
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
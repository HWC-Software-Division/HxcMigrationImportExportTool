using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Parsers
{
    public class PageTypeParser
    {
        public List<K13PageType> Parse(string xmlPath)
        {
            var result = new List<K13PageType>();

            var doc = XDocument.Load(xmlPath);

            var types = doc.Descendants()
                .Where(x => x.Name.LocalName == "DocumentType");

            foreach (var t in types)
            {
                var pageType = new K13PageType
                {
                    ClassName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassName")?.Value,

                    DisplayName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassDisplayName")?.Value,

                    TableName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassTableName")?.Value
                };

                result.Add(pageType);
            }

            return result;
        }
    }
}

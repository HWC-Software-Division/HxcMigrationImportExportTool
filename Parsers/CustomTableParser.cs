using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Parsers
{
    public class CustomTableParser
    {
        public List<K13CustomTable> Parse(string xmlPath)
        {
            var result = new List<K13CustomTable>();

            var doc = XDocument.Load(xmlPath);

            var tables = doc.Descendants().Where(x => x.Name.LocalName == "CustomTable");

            foreach (var t in tables)
            {
                var table = new K13CustomTable
                {
                    TableName = t.Element("ClassTableName")?.Value,
                    DisplayName = t.Element("ClassDisplayName")?.Value
                };

                result.Add(table);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Parsers
{
    public class ResourceStringParser
    {
        public List<K13ResourceString> Parse(string xmlPath)
        {
            var result = new List<K13ResourceString>();

            var doc = XDocument.Load(xmlPath);

            var strings = doc.Descendants().Where(x => x.Name.LocalName == "ResourceString");

            foreach (var s in strings)
            {
                var item = new K13ResourceString
                {
                    Key = s.Element("StringKey")?.Value,
                    Value = s.Element("StringText")?.Value
                };

                result.Add(item);
            }

            return result;
        }
    }
}

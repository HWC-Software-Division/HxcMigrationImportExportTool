using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Services;

namespace HxcMigrationImportExportTool.Parsers
{
    public class PageTypeParser
    {
        public List<K13PageType> Parse(string xmlPath)
        {
            Logger.Log($"Start parsing PageType XML : {xmlPath}");

            var result = new List<K13PageType>();

            var doc = XDocument.Load(xmlPath);

            Logger.Log("XML Loaded successfully");
            /* DEBUG XML STRUCTURE */
            foreach (var node in doc.Descendants().Take(30))
            {
                Logger.Log($"XML Node : {node.Name.LocalName}");
            }

            var types = doc.Descendants().Where(x => x.Name.LocalName == "cms_class");

            Logger.Log($"cms_class nodes found : {types.Count()}");


            foreach (var t in types)
            {
                var isDocType = t.Elements().FirstOrDefault(e => e.Name.LocalName == "ClassIsDocumentType")?.Value;

                if (!string.Equals(isDocType, "true", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pageType = new K13PageType
                {
                    ClassName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassName")?.Value,

                    DisplayName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassDisplayName")?.Value,

                    TableName = t.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "ClassTableName")?.Value
                };

                //ClassFormDefinition
                var formDefinition = t.Elements().FirstOrDefault(e => e.Name.LocalName == "ClassFormDefinition")?.Value;

                if (!string.IsNullOrEmpty(formDefinition))
                {
                    var formXml = XDocument.Parse(formDefinition);

                    var fields = formXml.Descendants()
                        .Where(x => x.Name.LocalName == "field");

                    foreach (var f in fields)
                    {
                        var field = new K13Field
                        {
                            Column = f.Attribute("column")?.Value,
                            DataType = f.Attribute("columntype")?.Value ?? ""
                        };

                        pageType.Fields.Add(field);

                        Logger.Log("Field XML:");
                        Logger.Log(f.ToString());

                        Logger.Log($"Field detected : {field.Column} ({field.DataType})");
                    }
                }

                Logger.Log($"PageType detected : {pageType.ClassName}");

                result.Add(pageType);
            }

            Logger.Log($"Total PageTypes parsed : {result.Count}");

            return result;
        }
    }
}

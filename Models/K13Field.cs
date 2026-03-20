using System;
using System.Collections.Generic;
using System.Text;

namespace HxcMigrationImportExportTool.Models
{
    public class K13Field
    {
        public string? Column { get; set; }

        public string? DataType { get; set; }

        public bool Required { get; set; }
        public int Size { get; set; }
        public string? DefaultValue { get; set; }
        public string? FormControl { get; set; }
        public string? Caption { get; set; }

        public string? DataSource { get; set; }
    }
}

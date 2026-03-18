using System;
using System.Collections.Generic;
using System.Text;

namespace HxcMigrationImportExportTool.Models
{
    public class SelectItem<T>
    {
        public bool IsSelected { get; set; }

        public T Data { get; set; }
    }
}

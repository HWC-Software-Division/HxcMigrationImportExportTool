using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Compression;

namespace HxcMigrationImportExportTool.Services
{
    public class ZipService
    {
        public static string Extract(string zipPath)
        {
            var folder = Path.Combine(
                Path.GetTempPath(),
                "k13_" + Guid.NewGuid()
            );

            ZipFile.ExtractToDirectory(zipPath, folder);

            return folder;
        }
    }
}

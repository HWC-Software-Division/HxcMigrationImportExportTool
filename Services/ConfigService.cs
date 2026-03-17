using HxcMigrationImportExportTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HxcMigrationImportExportTool.Services
{
    public static class ConfigService
    {
        private static string filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DBSetting.json"
        );

        public static void Save(DbConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        public static DbConfig? Load()
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<DbConfig>(json);
        }
    }
}

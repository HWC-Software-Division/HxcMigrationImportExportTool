using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.IO;

namespace HxcMigrationImportExportTool.Services
{
    public static class Logger
    {
        private static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public static void Log(string message)
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            var file = Path.Combine(
                LogFolder,
                $"log_{DateTime.Now:yyyyMMdd}.txt"
            );

            var logLine = $"{DateTime.Now:HH:mm:ss} | {message}";

            File.AppendAllText(file, logLine + Environment.NewLine);
        }
    }
}

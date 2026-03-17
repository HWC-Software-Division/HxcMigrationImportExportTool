using System;
using System.Collections.Generic;
using System.Text;

namespace HxcMigrationImportExportTool.Models
{
    public class DbConfig
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string GetConnectionString()
        {
            return $"Server={Server};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True;";
        }
    }
}

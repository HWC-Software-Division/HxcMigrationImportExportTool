using System;
using System.Collections.Generic;
using System.Text;

using System.Data.SqlClient;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Services
{
    public class MigrateService
    {
        private readonly string _connStr;

        public MigrateService(string connStr)
        {
            _connStr = connStr;
        }

        public void InsertPageType(K13PageType pt)
        {
            using var conn = new SqlConnection(_connStr);
            conn.Open();
             
            var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM CMS_Class WHERE ClassName = @name",
                conn);

            checkCmd.Parameters.AddWithValue("@name", pt.ClassName);

            int exists = (int)checkCmd.ExecuteScalar();

            if (exists > 0)
            {
                return; // ข้าม
            }

            var cmd = new SqlCommand(@"
                INSERT INTO CMS_Class (
                    ClassName,
                    ClassDisplayName,
                    ClassTableName,
                    ClassGUID,
                    ClassIsDocumentType,
                    ClassIsCoupledClass,
                    ClassType
                )
                VALUES (
                    @name,
                    @display,
                    @table,
                    NEWID(),
                    1,
                    1,
                    'Content'
                )
                ", conn);

             cmd.Parameters.AddWithValue("@name", pt.ClassName);
             cmd.Parameters.AddWithValue("@display", pt.DisplayName ?? "");
             cmd.Parameters.AddWithValue("@table", pt.TableName ?? "");

             cmd.ExecuteNonQuery();
        }
    }
               
}

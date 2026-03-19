using System;
using System.Collections.Generic;
using System.Text;

using System.Data.SqlClient;
using HxcMigrationImportExportTool.Models;

namespace HxcMigrationImportExportTool.Services
{
    public class MigrateService
    {
        //private readonly string _connStr; 

        //public MigrateService(string connStr)
        //{
        //    _connStr = connStr;
        //}

        //public void InsertPageType(K13PageType pt)
        //{
        //    using var conn = new SqlConnection(_connStr);
        //    conn.Open();

        //    var checkCmd = new SqlCommand(
        //        "SELECT COUNT(*) FROM CMS_Class WHERE ClassName = @name",
        //        conn);

        //    checkCmd.Parameters.AddWithValue("@name", pt.ClassName);

        //    int exists = (int)checkCmd.ExecuteScalar();

        //    if (exists > 0)
        //    {
        //        return; // ข้าม
        //    }

        //    var cmd = new SqlCommand(@"
        //        INSERT INTO CMS_Class (
        //            ClassName,
        //            ClassDisplayName,
        //            ClassTableName,
        //            ClassGUID,
        //            ClassIsDocumentType,
        //            ClassIsCoupledClass,
        //            ClassType
        //        )
        //        VALUES (
        //            @name,
        //            @display,
        //            @table,
        //            NEWID(),
        //            1,
        //            1,
        //            'Content'
        //        )
        //        ", conn);

        //     cmd.Parameters.AddWithValue("@name", pt.ClassName);
        //     cmd.Parameters.AddWithValue("@display", pt.DisplayName ?? "");
        //     cmd.Parameters.AddWithValue("@table", pt.TableName ?? "");

        //     cmd.ExecuteNonQuery();
        //} 

        private readonly XbykApiService _api;

        public MigrateService(XbykApiService api)
        {
            _api = api;
        }
         

        //Mapping Function
        private object MapToContentType(K13PageType pt)
        {
            return new
            {
                name = pt.DisplayName,
                codeName = pt.ClassName,

                fields = pt.Fields.Select(f => new
                {
                    name = f.Column,
                    dataType = MapDataType(f.DataType),
                     
                    isRequired = false,
                    size = 200,
                    fieldType = MapFieldType(f.DataType),
                    caption = f.Column
                }).ToList()
            };
        }

        private string MapDataType(string type)
        {
            return type?.ToLower() switch
            {
                "text" => "text",
                "longtext" => "richText",
                "integer" => "number",
                "longinteger" => "number",
                "double" => "number",
                "decimal" => "number",
                "boolean" => "boolean",
                "guid" => "guid",
                "datetime" => "dateTime",
                "date" => "date",

                // advanced (ยังไม่ต้องทำก็ได้)
                "file" => "mediaFiles",
                "attachment" => "mediaFiles",
                "pages" => "pages",
                "taxonomy" => "taxonomy",
                _ => "text" // fallback กันพัง
            };
        }

        private string MapFieldType(string type)
        {
            return type?.ToLower() switch
            {
                "text" => "textbox",
                "longtext" => "textarea",
                "boolean" => "checkbox",
                "integer" => "textbox",
                "datetime" => "datetime",
                _ => "textbox"
            };
        }

        public async Task<(int success, int fail, int skip)> MigratePageTypesAsync(List<K13PageType> pageTypes)
        {
            int success = 0;
            int fail = 0;
            int skip = 0;

            foreach (var pt in pageTypes)
            {
                try
                { 
                    // mapping
                    var payload = MapToContentType(pt);

                    // create
                    var result = await _api.CreateContentTypeAsync(payload);

                    if (result.Contains("Already exists"))
                    {
                        skip++;
                    }
                    else if (result.Contains("created"))
                    {
                        success++;
                        Logger.Log($"Created: {pt.ClassName}");
                    }
                    else
                    {
                        fail++;
                        Logger.Log($"Failed: {pt.ClassName}");
                    }
                     
                }
                catch (Exception ex)
                {
                    fail++;
                    Logger.Log($"Error: {pt.ClassName} - {ex.Message}");
                }
            }

            return (success, fail, skip);
        }

    }

}

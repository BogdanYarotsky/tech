using System.Data;
using System.Text;
using DataLoader.Services;
using Microsoft.Data.SqlClient;

var tables = await Processor.GetNormalizedReportsTablesFromCsv([2021, 2022, 2023, 2024]);

var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");
using var connection = new SqlConnection(connectionString);
connection.Open();
using var tx = connection.BeginTransaction();

var options =
    SqlBulkCopyOptions.KeepIdentity
    | SqlBulkCopyOptions.TableLock; // for speed

void BulkWrite(DataTable data, string tableName)
{
    using var bulkCopy = new SqlBulkCopy(connection, options, tx);
    bulkCopy.DestinationTableName = tableName;
    foreach (DataColumn column in data.Columns)
    {
        bulkCopy.ColumnMappings.Add(
            column.ColumnName, column.ColumnName);
    }
    bulkCopy.WriteToServer(data);
}

var truncateSql = new StringBuilder();
foreach (var table in new[] { "ReportsTags", "Reports", "Countries", "Tags", "TagTypes" })
{
    truncateSql.AppendLine($"TRUNCATE TABLE dbo.{table};");
}

try
{
    using var truncateCmd = new SqlCommand(truncateSql.ToString(), connection, tx);
    truncateCmd.ExecuteNonQuery();

    BulkWrite(tables.TagTypes, "TagTypes");
    BulkWrite(tables.Tags, "Tags");
    BulkWrite(tables.Countries, "Countries");
    BulkWrite(tables.Reports, "Reports");
    BulkWrite(tables.ReportsTags, "ReportsTags");
    tx.Commit();
}
catch
{
    tx.Rollback();
    throw;
}
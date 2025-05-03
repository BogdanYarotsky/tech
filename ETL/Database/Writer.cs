using System.Data;
using Microsoft.Data.SqlClient;

namespace DataLoader.Database
{
    public class Writer
    {
        internal static async Task BulkCopyToCleanedTablesAsync(
            string connectionString, DataTable[] tables, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = connection.BeginTransaction();

            var options =
                SqlBulkCopyOptions.KeepIdentity
                | SqlBulkCopyOptions.TableLock; // for speed

            async Task WriteAsync(DataTable dt)
            {
                using var bulkCopy = new SqlBulkCopy(connection, options, transaction);
                bulkCopy.DestinationTableName = dt.TableName;
                foreach (DataColumn column in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(
                        column.ColumnName, column.ColumnName);
                }
                Console.WriteLine("Writing to table " + bulkCopy.DestinationTableName);
                await bulkCopy.WriteToServerAsync(dt, cancellationToken);
            }

            try
            {
                // var truncateSql = string.Join(";", tables.Reverse().Select(t => $"TRUNCATE TABLE {t.TableName}"));
                // Console.WriteLine(truncateSql);
                // using var truncateCmd = new SqlCommand(truncateSql, connection, transaction);
                // await truncateCmd.ExecuteNonQueryAsync(cancellationToken);
                foreach (var table in tables)
                {
                    await WriteAsync(table);
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
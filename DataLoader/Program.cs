using System.Data;
using DataLoader;

var reports = SurveysCsvReader.ReadReports();
var tables = TableMapper.MapToTables(reports);
Console.WriteLine(tables.ReportsTags.Rows.Count);

// var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");
// using var dbConnection = new SqlConnection(connectionString);
// dbConnection.Open();

// // ensure tables are created and empty
// // using var dbCommand = new SqlCommand("", dbConnection);
// // dbCommand.ExecuteNonQuery();

// using var tx = dbConnection.BeginTransaction();
// try
// {
//     using var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity, tx);
//     bulkCopy.DestinationTableName = "dbo.NewTable";
//     var dt = new DataTable();
//     dt.Columns.AddRange([new("UserID")]);
//     dt.Rows.Add(666);
//     dt.Rows.Add(777);
//     bulkCopy.WriteToServer(dt);
//     tx.Commit();
// }
// catch
// {
//     tx.Rollback();
//     throw;
// }
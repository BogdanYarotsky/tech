using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using DataLoader;

var sw = Stopwatch.StartNew();

var tables = new SurveyTables();

{
    using var buffer = new BlockingCollection<Report>();
    var consumeTask = Task.Run(() => TableMapper.AggregateTables(buffer, tables));
    SurveysCsvReader.ReadReports(buffer);
    buffer.CompleteAdding();
    await consumeTask;
}


Console.WriteLine(sw.Elapsed);
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
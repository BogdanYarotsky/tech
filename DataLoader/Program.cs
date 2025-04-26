using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using DataLoader;
using Microsoft.Data.SqlClient;


var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");
await using var dbConnection = new SqlConnection(connectionString);
await using var dbCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.NewTable", dbConnection);
await dbConnection.OpenAsync();
var result = await dbCommand.ExecuteScalarAsync();
Console.WriteLine(result);


// var rows = SurveysReader.ReadRowsFromCsv();

// var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
// using var dbConnection = new SqlConnection(connectionString);

// // spin up docker con
// using var bulkCopy = new SqlBulkCopy(dbConnection)
// {
//     DestinationTableName = "todo"
// };

// dbConnection.Open();
// bulkCopy.WriteToServer(new DataTable());


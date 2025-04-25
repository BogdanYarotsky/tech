// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
await using var dbConnection = new SqlConnection(connectionString);
await using var dbCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.NewTable", dbConnection);
await dbConnection.OpenAsync();
var result = await dbCommand.ExecuteScalarAsync();
Console.WriteLine(result);
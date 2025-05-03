using System.Data;
using System.Text;
using DataLoader.Database;
using DataLoader.Services;
using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("DB_CONN_STR");
ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    cts.Cancel();
    args.Cancel = true;
};
try
{
    // ensure migrations are there already
    Migrator.EnsureDatabaseInGoodShape(connectionString, cts.Token);
    if (args.Contains("--migrate-only"))
    {
        return;
    }
    var tables = await Processor.GetSalaryReportsSqlTablesAsync(cts.Token);
    Writer.BulkCopyToCleanedTablesAsync(connectionString, tables, cts.Token);
}
catch (OperationCanceledException)
{
}

return;
// var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using DataLoader;
using Microsoft.Data.SqlClient;


// var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");
// await using var dbConnection = new SqlConnection(connectionString);
// await using var dbCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.NewTable", dbConnection);
// await dbConnection.OpenAsync();
// var result = await dbCommand.ExecuteScalarAsync();
// Console.WriteLine(result);


var reports = SurveysReader.ReadReportsFromCsv();

var tags = reports
    .SelectMany(r => r.Tags)
    .Distinct()
    .Select((t, i) => (t, i + 1))
    .ToDictionary();

var countries = reports
    .Select(r => r.Country)
    .Distinct()
    .Select((c, i) => (c, i + 1))
    .ToDictionary();

DataTable tagsTable = new();
tagsTable.Columns.AddRange([
    new("TagID"), new("TagName"), new("TagType")
]);
foreach (var (tag, id) in tags)
    tagsTable.Rows.Add(id, tag.Name, tag.Type);

DataTable countriesTable = new();
countriesTable.Columns.AddRange([
    new("CountryID"), new("CountryName")
]);
foreach (var (name, id) in countries)
    countriesTable.Rows.Add(id, name);

DataTable reportsTable = new();
reportsTable.Columns.AddRange([
    new("ReportID"), new("CountryID"), new("Year"), new("Experience"), new("YearlySalaryUsd")
]);

DataTable junctionTable = new();
junctionTable.Columns.AddRange([
    new("ReportID"), new("TagID")
]);

foreach (var (reportId, report) in reports.Index())
{
    var countryId = countries[report.Country];
    reportsTable.Rows.Add(reportId, countryId, report);

}

var connectionString = Environment.GetEnvironmentVariable("LOCALDB_URL");
using var dbConnection = new SqlConnection(connectionString);
dbConnection.Open();

// ensure tables are created and empty
// using var dbCommand = new SqlCommand("", dbConnection);
// dbCommand.ExecuteNonQuery();

using var tx = dbConnection.BeginTransaction();
try
{
    using var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.KeepIdentity, tx);
    bulkCopy.DestinationTableName = "dbo.NewTable";
    var dt = new DataTable
    {
        Columns = { new("") }

    };
    dt.Columns.AddRange([new("UserID")]);
    dt.Rows.Add(666);
    dt.Rows.Add(777);
    bulkCopy.WriteToServer(dt);
    tx.Commit();
}
catch
{
    tx.Rollback();
    throw;
}
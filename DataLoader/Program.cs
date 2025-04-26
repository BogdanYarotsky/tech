using System.Collections.Concurrent;
using System.Collections.Frozen;
using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
await using var dbConnection = new SqlConnection(connectionString);
await using var dbCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.NewTable", dbConnection);
await dbConnection.OpenAsync();
var result = await dbCommand.ExecuteScalarAsync();
Console.WriteLine(result);

var tagsAliases = new Dictionary<string, string>
{
    {"Amazon Web Services (AWS)", "AWS"},
    {".NET Core / .NET 5", ".NET (5+)"},
    {".NET Framework (1.0 - 4.8)", ".NET Framework"},
    {"ASP.NET CORE", "ASP.NET Core"},
    {"Dynamodb", "DynamoDB"},
    {"React.js", "React"}
}.ToFrozenDictionary();

ConcurrentBag<int> bag = [];

Parallel.For(2021, 2025, year =>
{

});


static bool IsNA(string? s) => string.IsNullOrWhiteSpace(s) || s == "NA";

record Tag(string Name, TagType Type);

enum TagType
{
    None,
    DevType,
    Language,
    Database,
    Platform,
    WebFramework,
    MiscTech,
    Tools,
    CollabTools,
}

record ProcessedRow(
    string Country,
    int YearsCoding,
    int YearlySalaryUsd,
    IReadOnlySet<Tag> Tags);
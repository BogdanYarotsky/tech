using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using DataLoader.Database;
using DataLoader.Services;
using Microsoft.Data.SqlClient;


using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    cts.Cancel();
    args.Cancel = true;
};

var processor = new Processor<List<int[]>>(new TransformerToJson());
var rows = await processor.TransformSalaryReportsAsync(cts.Token);
var json = JsonSerializer.Serialize(rows);
File.WriteAllText("reports.json", json);

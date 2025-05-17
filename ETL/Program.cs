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

var processor = Processor.Create(new MinimalTransformer());
var output = await processor.TransformSalaryReportsAsync(cts.Token);
File.WriteAllBytes("test.bin", output);



using System.Data;
using System.Threading.Channels;

namespace DataLoader.Services;
public static class Processor
{
    public static async Task<DataTable[]> GetSalaryReportsSqlTablesAsync(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Report>(new()
        {
            SingleReader = true,
            SingleWriter = false
        });
        var consumerTask = Consumer.GetSqlTablesAsync(channel.Reader, cancellationToken);
        Producer.ReadSalaryReportsInParallel(channel.Writer, cancellationToken);
        channel.Writer.Complete();
        return await consumerTask;
    }
}
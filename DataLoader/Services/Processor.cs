using System.Threading.Channels;
using DataLoader.Models;

namespace DataLoader.Services;
public static class Processor
{
    public static async Task<NormalizedReportTables> GetNormalizedSalaryReportsAsync()
    {
        var channel = Channel.CreateUnbounded<Report>(new()
        {
            SingleReader = true,
            SingleWriter = false
        });
        var consumerTask = Consumer.GetNormalizedTablesAsync(channel.Reader);
        await Producer.ReadSalaryReporsAsync(channel.Writer);
        channel.Writer.Complete();
        return await consumerTask;
    }
}
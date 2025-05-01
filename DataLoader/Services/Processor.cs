using System.Collections.Concurrent;
using DataLoader.Models;

namespace DataLoader.Services;
public static class Processor
{
    public static async Task<NormalizedReports> GetNormalizedReportsTablesFromCsv(int[] years)
    {
        using var buffer = new BlockingCollection<Report>();
        var consumer = Task.Run(() => Consumer.AggregateNormalizedReports(buffer));
        Producer.ReadCsvReportsForYears(years, buffer);
        buffer.CompleteAdding();
        return await consumer;
    }
}
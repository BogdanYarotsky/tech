using System.Data;
using System.Threading.Channels;

namespace DataLoader.Services;

public interface ITransformer<T>
{
    Task<T> TransformAsync(ChannelReader<Report> reader, CancellationToken cancellationToken);
}

public class Processor<T>(ITransformer<T> transformer)
{
    private readonly ITransformer<T> transformer = transformer;
    public async Task<T> TransformSalaryReportsAsync(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Report>(new()
        {
            SingleWriter = false,
            SingleReader = true,
        });
        var consumerTask = transformer.TransformAsync(channel.Reader, cancellationToken);
        Producer.ReadSalaryReportsInParallel(channel.Writer, cancellationToken);
        channel.Writer.Complete();
        return await consumerTask;
    }
}
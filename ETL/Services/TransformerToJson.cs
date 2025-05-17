using System.Collections.Frozen;
using System.Threading.Channels;
using DataLoader;
using DataLoader.Services;

public class MinimalTransformer : ITransformer<byte[]>
{
    public async Task<byte[]> TransformAsync(ChannelReader<Report> reader, CancellationToken cancellationToken)
    {
        var tagTypesCount = Enum
            .GetValues<TagType>()
            .Length; // todo - const


        Dictionary<string, int> tagNameToId = [];
        Dictionary<string, int> CountryNameToId = [];

        // todo - read all reports first, order by salary, then minimize

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        await foreach (var r in reader.ReadAllAsync(cancellationToken))
        {
            // first 4 values for report are always fixed
            writer.Write((byte)r.Year);
            writer.Write((byte)r.YearsCoding);
            writer.Write((byte)GetId(CountryNameToId, r.Country));
            writer.Write(r.YearlySalaryUsd);

            // then comes tags count
            writer.Write((byte)r.Tags.Count);
            // now indicies of all tags for this report
            foreach (var tag in r.Tags)
            {
                writer.Write((ushort)GetId(tagNameToId, tag.Name)); // ushort
            }
        }

        Console.WriteLine(tagNameToId.Count);
        Console.WriteLine(CountryNameToId.Count);

        return stream.ToArray();
    }

    static int GetId(Dictionary<string, int> dict, string key)
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }
        return dict[key] = dict.Count;
    }
}
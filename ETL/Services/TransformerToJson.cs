using System.Collections.Frozen;
using System.Threading.Channels;
using DataLoader;
using DataLoader.Services;

public class TransformerToJson : ITransformer<List<int[]>>
{
    public async Task<List<int[]>> TransformAsync(ChannelReader<Report> reader, CancellationToken cancellationToken)
    {
        var tagTypesCount = Enum
            .GetValues<TagType>()
            .Length; // todo - const

        var rows = new List<int[]>();

        Dictionary<string, int> tagNameToId = [];
        Dictionary<string, int> CountryNameToId = [];

        int GetId(Dictionary<string, int> dict, string key)
        {
            if (tagNameToId.TryGetValue(key, out var value))
            {
                return value;
            }
            return tagNameToId[key] = tagNameToId.Count;
        }

        await foreach (var r in reader.ReadAllAsync(cancellationToken))
        {
            var row = new int[4 + r.Tags.Count];
            row[0] = GetId(CountryNameToId, r.Country);
            row[1] = r.Year;
            row[2] = r.YearsCoding;
            row[3] = r.YearlySalaryUsd;
            for (var i = 0; i < r.Tags.Count; i++)
            {
                row[i + 4] = GetId(tagNameToId, r.Tags[i].Name);
            }
            var tagsIds = r.Tags.Select(t => GetId(tagNameToId, t.Name)).ToArray();
            rows.Add(row);
        }

        return rows;
    }
}
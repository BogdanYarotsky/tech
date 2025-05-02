using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Threading.Channels;
using DataLoader.Models;

namespace DataLoader.Services;

public static class Consumer
{
    internal static async Task<NormalizedReportTables> GetNormalizedTablesAsync(
        ChannelReader<Report> reader)
    {
        NormalizedReportTables tables = new();

        var tagTypeIds = Enum
            .GetValues<TagType>()
            .ToFrozenDictionary(
                e => e,
                e => (int)e + 1
            );

        foreach (var (tagType, tagTypeId) in tagTypeIds)
        {
            tables.TagTypes.Rows.Add(tagTypeId, tagType.ToString());
        }

        Dictionary<string, int> countryIds = [];
        Dictionary<Tag, int> tagIds = [];

        int nextCountryId = 1;
        int nextTagId = 1;
        int reportId = 1;

        await foreach (var report in reader.ReadAllAsync())
        {
            if (!countryIds.TryGetValue(report.Country, out int countryId))
            {
                countryId = nextCountryId++;
                countryIds.Add(report.Country, countryId);
                tables.Countries.Rows.Add(countryId, report.Country);
            }

            tables.Reports.Rows.Add(
                reportId,
                countryId,
                report.Year,
                report.YearsCoding,
                report.YearlySalaryUsd);

            foreach (var tag in report.Tags)
            {
                if (!tagIds.TryGetValue(tag, out int tagId))
                {
                    tagId = nextTagId++;
                    tagIds.Add(tag, tagId);
                    var tagTypeId = tagTypeIds[tag.Type];
                    tables.Tags.Rows.Add(tagId, tag.Name, tagTypeId);
                }

                tables.ReportsTags.Rows.Add(reportId, tagId);
            }

            reportId++;
        }
        return tables;
    }
}
using System.Collections.Concurrent;
using System.Collections.Frozen;
using DataLoader.Models;

namespace DataLoader.Services;

public static class Consumer
{
    internal static NormalizedReports AggregateNormalizedReports(BlockingCollection<Report> buffer)
    {
        NormalizedReports tables = new();

        var tagTypeIds = Enum
            .GetValues<TagType>()
            .ToFrozenDictionary(
                e => e,
                e => (int)e + 1
            );

        foreach (var (tagTypeName, tagTypeId) in tagTypeIds)
        {
            tables.TagTypes.Rows.Add(tagTypeId, tagTypeName);
        }

        Dictionary<string, int> countryIds = [];
        Dictionary<Tag, int> tagIds = [];

        int nextCountryId = 1;
        int nextTagId = 1;
        int reportId = 1;

        foreach (var report in buffer.GetConsumingEnumerable())
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
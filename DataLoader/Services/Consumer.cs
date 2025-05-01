using System.Collections.Concurrent;
using DataLoader.Models;

namespace DataLoader.Services;

public static class Consumer
{
    internal static NormalizedReports AggregateNormalizedReports(BlockingCollection<Report> buffer)
    {
        Dictionary<string, int> countryIds = [];
        Dictionary<Tag, int> tagIds = [];

        int nextCountryId = 1;
        int nextTagId = 1;
        int reportId = 1;

        NormalizedReports tables = new();
        foreach (var report in buffer.GetConsumingEnumerable())
        {
            if (!countryIds.TryGetValue(report.Country, out int countryId))
            {
                countryId = nextCountryId++;
                countryIds.Add(report.Country, countryId);
                tables.Countries.Rows.Add(countryId, report.Country);
            }

            tables.Reports.Rows.Add(
                reportId++,
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
                    tables.Tags.Rows.Add(tagId, tag.Name, tag.Type);
                }

                tables.ReportsTags.Rows.Add(reportId, tagId);
            }
        }
        return tables;
    }
}
using DataLoader;

public static class TableMapper
{
    public static SurveyTables MapToTables(Report[] reports)
    {
        SurveyTables tables = new();

        var countryIds = reports
            .Select(r => r.Country)
            .Distinct()
            .Select((c, i) => (c, i + 1))
            .ToDictionary();

        foreach (var (name, id) in countryIds)
            tables.Countries.Rows.Add(id, name);

        var tagIds = reports
            .SelectMany(r => r.Tags)
            .Distinct()
            .Select((t, i) => (t, i + 1))
            .ToDictionary();

        foreach (var (tag, id) in tagIds)
            tables.Tags.Rows.Add(id, tag.Name, tag.Type);

        foreach (var (reportIndex, report) in reports.Index())
        {
            var reportId = reportIndex + 1;
            var countryId = countryIds[report.Country];

            tables.Reports.Rows.Add(
                reportId,
                countryId,
                report.Year,
                report.YearsCoding,
                report.YearlySalaryUsd);

            foreach (var tag in report.Tags)
            {
                var tagId = tagIds[tag];
                tables.ReportsTags.Rows.Add(reportId, tagId);
            }
        }

        return tables;
    }
}

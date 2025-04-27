namespace DataLoader;

public static class TableMapper
{
    public static SurveyTables MapToTables(ReadOnlySpan<Report> reports)
    {
        SurveyTables tables = new();

        Dictionary<string, int> countryIds = [];
        Dictionary<Tag, int> tagIds = [];

        int nextCountryId = 1;
        int nextTagId = 1;

        for (int i = 0; i < reports.Length; i++)
        {
            var report = reports[i];
            var reportId = i + 1;

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
                    tables.Tags.Rows.Add(tagId, tag.Name, tag.Type);
                }

                tables.ReportsTags.Rows.Add(reportId, tagId);
            }
        }

        return tables;
    }
}
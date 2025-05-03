using System.Collections.Frozen;
using System.Data;
using System.Threading.Channels;

namespace DataLoader.Services;

public static class Consumer
{
    internal static async Task<DataTable[]> GetSqlTablesAsync(ChannelReader<Report> reader, CancellationToken cancellationToken)
    {
        var tagTypes = CreateTable("TagTypes", [
            ("TagTypeID", typeof(int)),
            ("TagTypeName", typeof(string))
        ]);

        var tags = CreateTable("Tags", [
            ("TagID", typeof(int)),
            ("TagName", typeof(string)),
            ("TagTypeID", typeof(int))
        ]);

        var countries = CreateTable("Countries", [
            ("CountryID", typeof(int)),
            ("CountryName", typeof(string))
        ]);

        var reports = CreateTable("Reports", [
            ("ReportID", typeof(int)),
            ("CountryID", typeof(int)),
            ("Year", typeof(int)),
            ("YearsCoding", typeof(int)),
            ("YearlySalaryUSD", typeof(int))
        ]);

        var reportsTags = CreateTable("ReportsTags", [
            ("ReportID", typeof(int)),
            ("TagID", typeof(int))
        ]);

        var tagTypeIds = Enum
            .GetValues<TagType>()
            .ToFrozenDictionary(
                e => e,
                e => (int)e + 1
            );

        foreach (var (tagType, tagTypeId) in tagTypeIds)
        {
            tagTypes.Rows.Add(tagTypeId, tagType.ToString());
        }

        Dictionary<string, int> countryIds = [];
        Dictionary<Tag, int> tagIds = [];

        int nextCountryId = 1;
        int nextTagId = 1;
        int reportId = 1;

        await foreach (var report in reader.ReadAllAsync(cancellationToken))
        {
            if (!countryIds.TryGetValue(report.Country, out int countryId))
            {
                countryId = nextCountryId++;
                countryIds.Add(report.Country, countryId);
                countries.Rows.Add(countryId, report.Country);
            }

            reports.Rows.Add(
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
                    tags.Rows.Add(tagId, tag.Name, tagTypeId);
                }

                reportsTags.Rows.Add(reportId, tagId);
            }

            reportId++;
        }

        return [tagTypes, tags, countries, reports, reportsTags];
    }

    private static DataTable CreateTable(string name, (string Name, Type Type)[] columns)
    {
        DataTable dt = new(name);

        foreach (var c in columns)
            dt.Columns.Add(c.Name, c.Type);

        return dt;
    }
}
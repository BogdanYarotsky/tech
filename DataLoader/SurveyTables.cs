using System.Data;

namespace DataLoader;

public record SurveyTables
{
    public DataTable Countries { get; } = Table(["CountryID", "CountryName"]);
    public DataTable Tags { get; } = Table(["TagID", "TagName", "TagType"]);

    public DataTable Reports { get; } = Table([
        "ReportID", "CountryID", "Year", "YearsCoding", "YearlySalaryUSD"
    ]);

    public DataTable ReportsTags { get; } = Table(["ReportID", "TagID"]);

    private static DataTable Table(string[] columns)
    {
        DataTable dt = new();

        foreach (var c in columns)
            dt.Columns.Add(c);

        return dt;
    }
}
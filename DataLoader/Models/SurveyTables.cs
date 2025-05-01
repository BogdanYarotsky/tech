using System.Data;

namespace DataLoader.Models;

public record NormalizedReports
{
    public DataTable Countries { get; } = Table(["CountryID", "CountryName"]);
    public DataTable Tags { get; } = Table(["TagID", "TagName", "TagTypeID"]);
    public DataTable TagTypes { get; } = Table(["TagTypeID", "TagTypeName"]);
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
WITH GermanReports AS (
    SELECT r.ReportID, r.YearlySalaryUSD
    FROM Reports r
    JOIN Countries c ON c.CountryID = r.CountryID
    WHERE CountryName = 'Germany'
),

PopularGermanTags AS (
    SELECT rt.TagID, COUNT(*) Responses FROM ReportsTags rt
    JOIN GermanReports r ON r.ReportID = rt.ReportID
    GROUP BY TagID
    HAVING COUNT(*) > 1200
)

SELECT DISTINCT t.TagName AS Tech,
    PERCENTILE_DISC(0.5) WITHIN GROUP (ORDER BY r.YearlySalaryUSD)
        OVER (PARTITION BY t.TagName) AS Median, pgt.Responses, tt.TagTypeName

FROM GermanReports r
JOIN ReportsTags rt ON rt.ReportID = r.ReportID
JOIN PopularGermanTags pgt ON pgt.TagID = rt.TagID
JOIN Tags t ON t.TagID = rt.TagID
JOIN TagTypes tt ON tt.TagTypeID = t.TagTypeID

ORDER BY Median DESC;
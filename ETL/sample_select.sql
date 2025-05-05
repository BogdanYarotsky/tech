
SELECT DISTINCT TagName AS Role,
    PERCENTILE_DISC(0.5) WITHIN GROUP (ORDER BY YearlySalaryUSD)
        OVER (PARTITION BY TagName) AS Median

FROM Reports
JOIN Countries ON Countries.CountryID = Reports.CountryID
JOIN ReportsTags ON ReportsTags.ReportID = Reports.ReportID
JOIN Tags ON Tags.TagID = ReportsTags.TagID
JOIN TagTypes ON TagTypes.TagTypeID = Tags.TagTypeID
WHERE 

CountryName = 'Germany' 
AND TagTypeName = 'DevType' 
-- AND YearsCoding = 4

ORDER BY Median DESC;
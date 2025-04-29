using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using CsvHelper;

namespace DataLoader;

public static class SurveysCsvReader
{
    public static void ReadReports(BlockingCollection<Report> buffer)
    {
        var tagsAliases = new Dictionary<string, string>
        {
            {"Amazon Web Services (AWS)", "AWS"},
            {".NET Core / .NET 5", ".NET (5+)"},
            {".NET Framework (1.0 - 4.8)", ".NET Framework"},
            {"ASP.NET CORE", "ASP.NET Core"},
            {"Dynamodb", "DynamoDB"},
            {"React.js", "React"},
            {"Webstorm", "WebStorm"},
            {".NET (5+)", ".NET"},
            {".NET (5+) ", ".NET"},
            {"AngularJS", "Angular.js"},
            {"ASP.NET Core ", "ASP.NET Core"},
            {"Bash/Shell (all shells)", "Bash/Shell"},
            {"Couch DB", "CouchDB"},
            {"Digital Ocean", "DigitalOcean"},
            {"Goland", "GoLand"},
            {"Google Cloud", "Google Cloud Platform"},
            {"IBM Cloud Or Watson", "IBM Cloud or Watson"},
            {"IntelliJ", "IntelliJ IDEA"},
            {"IPython/Jupyter", "IPython"},
            {"Linode, now Akamai", "Linode"},
            {"LISP", "Lisp"},
            {"Matlab", "MATLAB"},
            {"Neo4J", "Neo4j"},
            {"Netbeans", "NetBeans"},
            {"Oracle Cloud Infrastructure (OCI)", "Oracle Cloud Infrastructure"},
            {"PHPStorm", "PhpStorm"},
            {"Rad Studio (Delphi, C++ Builder)", "RAD Studio (Delphi, C++ Builder)"},
            {"Scikit-learn", "Scikit-Learn"}
        }.ToFrozenDictionary();

        var typeOverwrites = new Dictionary<string, TagType>
        {
            {"Xamarin", TagType.MiscTech},
            {"Supabase", TagType.Platform},
            {"Deno", TagType.Tools},
            {"Firebase", TagType.Platform},
            {"Node.js", TagType.WebFramework},
            {"Spring", TagType.MiscTech}
        }.ToFrozenDictionary();

        Parallel.For(2021, 2025, year =>
        {
            using var reader = new StreamReader($"/Users/byar/dev/tech/DataLoader/surveys/{year}.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                if (!csv.GetField("MainBranch")!.StartsWith("I am a dev"))
                    continue;

                if (!csv.GetField("Employment")!.EndsWith("full-time"))
                    continue;

                var salary = csv.GetField("ConvertedCompYearly");
                if (!int.TryParse(salary, out var goodSalary))
                {
                    continue;
                }

                var yearsCoding = csv.GetField("YearsCodePro")!;
                if (!int.TryParse(yearsCoding, out var goodYears))
                {
                    if (yearsCoding.StartsWith("More"))
                    {
                        goodYears = 51;
                    }
                    else if (!yearsCoding.StartsWith("Less"))
                    {
                        continue;
                    }
                }

                var country = csv.GetField("Country")!;
                if (country == "Republic of Korea")
                {
                    country = "South Korea";
                }
                else if (country == "The former Yugoslav Republic of Macedonia")
                {
                    country = "Republic of North Macedonia";
                }

                var tags = new List<Tag>();
                void AddTags(string columnName, TagType type)
                {
                    var values = csv.GetField(columnName);
                    if (IsNA(values)) return;
                    foreach (var value in values!.Split(";"))
                    {
                        var resolvedValue = tagsAliases.TryGetValue(value, out var alias)
                            ? alias : value;

                        var resolvedType = typeOverwrites.TryGetValue(resolvedValue, out var overwrite)
                            ? overwrite : type;

                        var tag = new Tag(resolvedValue, resolvedType);

                        tags.Add(tag);
                    }
                }
                AddTags("LanguageHaveWorkedWith", TagType.Language);
                AddTags("DatabaseHaveWorkedWith", TagType.Database);
                AddTags("PlatformHaveWorkedWith", TagType.Platform);
                AddTags("WebframeHaveWorkedWith", TagType.WebFramework);
                AddTags("MiscTechHaveWorkedWith", TagType.MiscTech);
                AddTags("ToolsTechHaveWorkedWith", TagType.Tools);
                AddTags("NEWCollabToolsHaveWorkedWith", TagType.CollabTools);
                var report = new Report(country, year, goodYears, goodSalary, tags);
                buffer.Add(report);
            }
        });
    }

    private static bool IsNA(string? s)
        => string.IsNullOrWhiteSpace(s) || s == "NA";
}
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Threading.Channels;
using CsvHelper;

namespace DataLoader.Services;

public static class Producer
{
    private class CsvRow
    {
        public string? MainBranch { get; set; }
        public string? Employment { get; set; }
        public string? ConvertedCompYearly { get; set; }
        public string? YearsCodePro { get; set; }
        public string? Country { get; set; }
        public string? DevType { get; set; }
        public string? LanguageHaveWorkedWith { get; set; }
        public string? DatabaseHaveWorkedWith { get; set; }
        public string? PlatformHaveWorkedWith { get; set; }
        public string? WebframeHaveWorkedWith { get; set; }
        public string? MiscTechHaveWorkedWith { get; set; }
        public string? ToolsTechHaveWorkedWith { get; set; }
        public string? NEWCollabToolsHaveWorkedWith { get; set; }
    }

    public static async Task ReadSalaryReporsAsync(ChannelWriter<Report> writer)
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

        var correctTagTypes = new Dictionary<string, TagType>
        {
            {"Deno", TagType.Tools},
            {"Node.js", TagType.Tools},
            {"Supabase", TagType.Platform},
            {"Firebase", TagType.Platform},
            {"Xamarin", TagType.MiscTech},
            {"Spring", TagType.MiscTech}
        }.ToFrozenDictionary();

        var tasks = Enumerable.Range(2021, 4).Select(async year =>
        {
            using var reader = new StreamReader($"surveys/{year}.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            await foreach (var row in csv.GetRecordsAsync<CsvRow>())
            {
                if (row.ConvertedCompYearly == "NA")
                {
                    continue;
                }

                var YearlySalaryUsd = int.Parse(row.ConvertedCompYearly!);

                if (row.YearsCodePro == "NA")
                {
                    continue;
                }

                if (!int.TryParse(row.YearsCodePro, out var yearsCoding))
                {
                    if (row.YearsCodePro.StartsWith("More"))
                    {
                        yearsCoding = 51;
                    }
                    else if (!row.YearsCodePro.StartsWith("Less"))
                    {
                        continue;
                    }
                }

                var country = row.Country switch
                {
                    "Republic of Korea" => "South Korea",
                    "The former Yugoslav Republic of Macedonia" => "Republic of North Macedonia",
                    _ => row.Country
                };

                var tags = new List<Tag>();
                void AddTags(string? values, TagType type)
                {
                    if (string.IsNullOrWhiteSpace(values))
                        return;

                    if (values == "NA")
                        return;

                    foreach (var value in values.Split(";"))
                    {
                        var resolvedValue = tagsAliases.TryGetValue(value, out var alias)
                            ? alias : value;

                        var resolvedType = correctTagTypes.TryGetValue(resolvedValue, out var overwrite)
                            ? overwrite : type;

                        var tag = new Tag(resolvedValue, resolvedType);
                        tags.Add(tag);
                    }
                }

                AddTags(row.DevType, TagType.DevType);
                AddTags(row.LanguageHaveWorkedWith, TagType.Language);
                AddTags(row.DatabaseHaveWorkedWith, TagType.Database);
                AddTags(row.WebframeHaveWorkedWith, TagType.WebFramework);
                AddTags(row.ToolsTechHaveWorkedWith, TagType.Tools);
                AddTags(row.NEWCollabToolsHaveWorkedWith, TagType.CollabTools);
                AddTags(row.PlatformHaveWorkedWith, TagType.Platform);
                AddTags(row.MiscTechHaveWorkedWith, TagType.MiscTech);
                AddTags(row.DevType, TagType.DevType);

                var report = new Report(country!, year, yearsCoding, YearlySalaryUsd, tags);
                writer.TryWrite(report);
            }
        });

        await Task.WhenAll(tasks);
    }
}
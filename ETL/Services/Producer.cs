using System.Collections.Frozen;
using System.Globalization;
using System.Threading.Channels;
using CsvHelper;

namespace DataLoader.Services;

public static class Producer
{
    public static void ReadSalaryReportsInParallel(ChannelWriter<Report> writer, CancellationToken cancellationToken)
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
            {"Scikit-learn", "Scikit-Learn"},
            {"Cobol", "COBOL"}
        }.ToFrozenDictionary();

        var correctTagTypes = new Dictionary<string, TagType>
        {
            {"Deno", TagType.Tools},
            {"Node.js", TagType.Tools},
            {"Firebase", TagType.Platform},
            {"Xamarin", TagType.MiscTech},
            {"Spring", TagType.MiscTech},
            {"Supabase", TagType.Platform},
            {"Flow", TagType.Tools}
        }.ToFrozenDictionary();

        var columnTypes = new[]
        {
            ("DevType", TagType.DevType),
            ("LanguageHaveWorkedWith", TagType.Language),
            ("DatabaseHaveWorkedWith", TagType.Database),
            ("PlatformHaveWorkedWith", TagType.Platform),
            ("WebframeHaveWorkedWith", TagType.WebFramework),
            ("MiscTechHaveWorkedWith", TagType.MiscTech),
            ("ToolsTechHaveWorkedWith", TagType.Tools),
            ("NEWCollabToolsHaveWorkedWith", TagType.CollabTools)
        };

        Parallel.For(21, 25, year =>
        {
            string path = $"surveys/20{year}.csv";
            Console.WriteLine($"Reading survey {path}");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!csv.GetField("MainBranch")!.StartsWith("I am a dev"))
                    continue;

                if (!csv.GetField("Employment")!.EndsWith("full-time"))
                    continue;

                var convertedCompYearly = csv.GetField("ConvertedCompYearly");
                if (!int.TryParse(convertedCompYearly, out var yearlySalaryUsd))
                {
                    continue;
                }

                if (yearlySalaryUsd < 2_500 || yearlySalaryUsd > 1_000_000)
                {
                    continue;
                }

                var yearsCodePro = csv.GetField("YearsCodePro")!;
                if (!int.TryParse(yearsCodePro, out var yearsCoding))
                {
                    if (yearsCodePro.StartsWith("More"))
                    {
                        yearsCoding = 51;
                    }
                    else if (!yearsCodePro.StartsWith("Less"))
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
                    if (string.IsNullOrWhiteSpace(values))
                        return;

                    if (values == "NA")
                        return;

                    foreach (var value in values!.Split(";"))
                    {
                        var resolvedValue = tagsAliases.TryGetValue(value, out var alias)
                            ? alias : value;

                        var resolvedType = correctTagTypes.TryGetValue(resolvedValue, out var overwrite)
                            ? overwrite : type;

                        var tag = new Tag(resolvedValue, resolvedType);
                        tags.Add(tag);
                    }
                }

                AddTags("DevType", TagType.DevType);
                AddTags("LanguageHaveWorkedWith", TagType.Language);
                AddTags("DatabaseHaveWorkedWith", TagType.Database);
                AddTags("PlatformHaveWorkedWith", TagType.Platform);
                AddTags("WebframeHaveWorkedWith", TagType.WebFramework);
                AddTags("MiscTechHaveWorkedWith", TagType.MiscTech);
                AddTags("ToolsTechHaveWorkedWith", TagType.Tools);
                AddTags("NEWCollabToolsHaveWorkedWith", TagType.CollabTools);
                var report = new Report(country, year, yearsCoding, yearlySalaryUsd, tags);
                writer.TryWrite(report);
            }
            Console.WriteLine($"Finished reading survey {path}");
        });
    }
}
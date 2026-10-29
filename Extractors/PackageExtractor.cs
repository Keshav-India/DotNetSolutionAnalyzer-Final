using DotNetSolutionAnalyzer.Models;
using System.Xml.Linq;

namespace DotNetSolutionAnalyzer.Extractors;

internal sealed class PackageExtractor
{
    public List<PackageModel> Extract(string projectFilePath)
    {
        var results = new List<PackageModel>();

        if (string.IsNullOrWhiteSpace(projectFilePath) || !File.Exists(projectFilePath))
            return results;

        try
        {
            var doc = XDocument.Load(projectFilePath);

            foreach (var element in doc.Descendants().Where(e =>
                         e.Name.LocalName is "PackageReference" or "Package"))
            {
                var name = (string?)element.Attribute("Include")
                    ?? (string?)element.Attribute("Update")
                    ?? (string?)element.Attribute("id")
                    ?? "";

                var version = (string?)element.Attribute("Version")
                    ?? element.Elements().FirstOrDefault(e => e.Name.LocalName == "Version")?.Value
                    ?? (string?)element.Attribute("version")
                    ?? "";

                if (!string.IsNullOrWhiteSpace(name))
                {
                    results.Add(new PackageModel
                    {
                        Name = name,
                        Version = version,
                        Source = element.Name.LocalName
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not parse packages in '{projectFilePath}': {ex.Message}");
        }

        var packagesConfig = Path.Combine(Path.GetDirectoryName(projectFilePath) ?? "", "packages.config");
        if (File.Exists(packagesConfig))
        {
            try
            {
                var doc = XDocument.Load(packagesConfig);
                foreach (var package in doc.Descendants().Where(e => e.Name.LocalName == "package"))
                {
                    var name = (string?)package.Attribute("id") ?? "";
                    var version = (string?)package.Attribute("version") ?? "";

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        results.Add(new PackageModel
                        {
                            Name = name,
                            Version = version,
                            Source = "packages.config"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not parse packages.config: {ex.Message}");
            }
        }

        return results
            .GroupBy(p => $"{p.Name}|{p.Version}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(p => p.Name)
            .ToList();
    }
}
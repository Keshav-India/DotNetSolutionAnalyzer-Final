using DotNetSolutionAnalyzer.Models;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DotNetSolutionAnalyzer.Extractors;

internal sealed class ConfigurationExtractor
{
    public List<ConfigurationModel> Extract(string projectDirectory)
    {
        var results = new List<ConfigurationModel>();
        if (!Directory.Exists(projectDirectory)) return results;

        foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.config", SearchOption.AllDirectories)
                     .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                              && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var doc = XDocument.Load(file);
                var model = new ConfigurationModel
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    AppSettings = doc.Descendants().Where(e => e.Name.LocalName == "appSettings")
                        .Elements().Where(e => e.Name.LocalName == "add")
                        .Select(e => new KeyValueModel
                        {
                            Key = (string?)e.Attribute("key") ?? "",
                            Value = RedactIfSecret((string?)e.Attribute("value") ?? "")
                        }).ToList(),
                    ConnectionStrings = doc.Descendants().Where(e => e.Name.LocalName == "connectionStrings")
                        .Elements().Where(e => e.Name.LocalName == "add")
                        .Select(e => new ConnectionStringModel
                        {
                            Name = (string?)e.Attribute("name") ?? "",
                            ProviderName = (string?)e.Attribute("providerName") ?? "",
                            ValueRedacted = RedactConnectionString((string?)e.Attribute("connectionString") ?? "")
                        }).ToList(),
                    BindingRedirects = doc.Descendants().Where(e => e.Name.LocalName == "bindingRedirect")
                        .Select(e => $"{(string?)e.Parent?.Attribute("oldVersion")} -> {(string?)e.Attribute("newVersion")}")
                        .ToList(),
                    ConfigSectionNames = doc.Descendants().Where(e => e.Name.LocalName == "section")
                        .Select(e => (string?)e.Attribute("name") ?? "")
                        .Where(s => s.Length > 0).ToList()
                };
                results.Add(model);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not parse config '{file}': {ex.Message}");
            }
        }

        return results;
    }

    private static string RedactConnectionString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        // Preserve useful server/database/provider metadata but redact credentials.
        var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(";", parts.Select(part =>
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2) return part;
            var key = pair[0].Trim();
            if (Regex.IsMatch(key, "password|pwd|user id|uid|accountkey|access token|client secret", RegexOptions.IgnoreCase))
                return $"{key}=<REDACTED>";
            return $"{key}={pair[1]}";
        }));
    }

    private static string RedactIfSecret(string value) =>
        Regex.IsMatch(value, "password|secret|token|apikey|api_key|accesskey", RegexOptions.IgnoreCase)
            ? "<REDACTED>" : value;
}

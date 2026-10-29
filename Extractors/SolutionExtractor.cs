using DotNetSolutionAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetSolutionAnalyzer.Extractors;

internal sealed class SolutionExtractor
{
    public async Task<SolutionModel> ExtractAsync(string solutionPath)
    {
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, e) =>
            Console.Error.WriteLine($"[MSBuildWorkspace {e.Diagnostic.Kind}] {e.Diagnostic.Message}");

        var solution = await workspace.OpenSolutionAsync(solutionPath);
        var result = new SolutionModel
        {
            SolutionPath = solutionPath,
            SolutionName = Path.GetFileNameWithoutExtension(solutionPath)
        };

        var ss = solution.Projects.ToList();

        foreach (var project in solution.Projects.OrderBy(p => p.Name))
        {
            var filePath = project.FilePath ?? "";
            var projectModel = new ProjectModel
            {
                ProjectId = project.Id.Id.ToString(),
                Name = project.Name,
                ProjectFilePath = filePath,
                ProjectDirectory = string.IsNullOrWhiteSpace(filePath)
                    ? ""
                    : Path.GetDirectoryName(filePath) ?? "",
                Language = project.Language,
                AssemblyName = project.AssemblyName ?? project.Name,
                TargetFramework = GetTargetFramework(project),
                ProjectReferences = project.ProjectReferences
                    .Select(r => solution.GetProject(r.ProjectId)?.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Cast<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                AssemblyReferences = project.MetadataReferences
                    .Select(r => r.Display ?? "")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
            result.Projects.Add(projectModel);
        }

        return result;
    }

    private static string GetTargetFramework(Project project)
    {
        // MSBuildWorkspace does not expose all evaluated MSBuild properties as a simple API.
        // Read common target-framework properties from the project file as a practical fallback.
        if (string.IsNullOrWhiteSpace(project.FilePath) || !File.Exists(project.FilePath))
            return "Unknown";

        try
        {
            var document = System.Xml.Linq.XDocument.Load(project.FilePath);
            var property = document.Descendants()
                .FirstOrDefault(e => e.Name.LocalName is "TargetFramework" or "TargetFrameworks" or "TargetFrameworkVersion");
            return property?.Value.Trim() ?? "Not declared directly in project file";
        }
        catch
        {
            return "Could not read project file";
        }
    }
}

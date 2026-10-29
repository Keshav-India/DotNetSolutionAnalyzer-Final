using DotNetSolutionAnalyzer.Analysis;
using DotNetSolutionAnalyzer.Database;
using DotNetSolutionAnalyzer.Extractors;
using DotNetSolutionAnalyzer.Models;
using DotNetSolutionAnalyzer.Serialization;
using DotNetSolutionAnalyzer.TransformerServices;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Xml.Linq;

namespace DotNetSolutionAnalyzer;
internal static class Program
{
    static string solutionPath = string.Empty;
    static string outputDirectory = string.Empty;
    static string fullSolutionPath = string.Empty;
    static string fullOutputDir = string.Empty;
    // Keep MSBuild registration before loading any solution/project through MSBuild APIs.
    private static async Task<int> Main(string[] args)
    {
        try
        {
            RegisterMsBuild();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            solutionPath = config["AnalyzerSettings:SolutionPath"];
            outputDirectory = config["AnalyzerSettings:OutputPath"];

            if (string.IsNullOrWhiteSpace(solutionPath) || string.IsNullOrWhiteSpace(outputDirectory))
            {
                Console.Error.WriteLine("Missing AnalyzerSettings:SolutionPath or AnalyzerSettings:OutputPath in appsettings.json");
                return 2;
            }

            fullSolutionPath = Path.GetFullPath(solutionPath);
            fullOutputDir = Path.GetFullPath(outputDirectory);

            if (!File.Exists(fullSolutionPath))
            {
                Console.Error.WriteLine($"Solution file not found: {fullSolutionPath}");
                return 2;
            }

            Directory.CreateDirectory(fullOutputDir);

            Console.WriteLine($"Analyzing solution: {fullSolutionPath}");

            var solutionModel = await new SolutionExtractor().ExtractAsync(fullSolutionPath);

            var graph = new ProjectDependencyGraph();
            graph.CalculateBuildOrder(solutionModel);

            foreach (var project in solutionModel.Projects)
            {
                // 1. Project/package details
                project.Packages = new PackageExtractor().Extract(project.ProjectFilePath);

                // 2. Configuration and runtime dependencies
                project.Configurations = new ConfigurationExtractor().Extract(project.ProjectDirectory);

                // 3. Code analysis
                project.Code = await new CodeExtractor().ExtractAsync(project);

                // 4. API surface analysis
                project.ApiEndpoints = new ApiExtractor().Extract(project);

                // 5. Database dependency analysis
                project.DatabaseReferences = AnalyzeDatabase(project);
            }

            var outputPath = Path.Combine(fullOutputDir, "solution-analysis.json");
            await JsonExportService.WriteAsync(solutionModel, outputPath);
            await TransformToNeo4JFormatAsync();

            Console.WriteLine();
            Console.WriteLine("Analysis completed.");
            Console.WriteLine($"Projects: {solutionModel.Projects.Count}");
            Console.WriteLine($"Types: {solutionModel.Projects.Sum(p => p.Code.Types.Count)}");
            Console.WriteLine($"Methods: {solutionModel.Projects.Sum(p => p.Code.Methods.Count)}");
            Console.WriteLine($"API endpoints: {solutionModel.Projects.Sum(p => p.ApiEndpoints.Count)}");
            Console.WriteLine($"JSON: {outputPath}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Analysis failed:");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static DatabaseModel AnalyzeDatabase(ProjectModel project)
    {
        var databaseModel = new DatabaseExtractor().Extract(project);

        foreach (var config in project.Configurations)
        {
            foreach (var connection in config.ConnectionStrings)
            {
                var actualConnectionString = GetActualConnectionString(config.FilePath, connection.Name);

                if (string.IsNullOrWhiteSpace(actualConnectionString))
                    continue;

                try
                {
                    var tables = DatabaseSchemaInspector.GetTables(actualConnectionString);
                    var procedures = DatabaseSchemaInspector.GetStoredProcedures(actualConnectionString);

                    databaseModel.TableNames.AddRange(tables);
                    databaseModel.StoredProcedures.AddRange(procedures);
                    databaseModel.ConnectionStringNames.Add(connection.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not query database schema for connection '{connection.Name}': {ex.Message}");
                }
            }
        }

        databaseModel.TableNames = databaseModel.TableNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        databaseModel.StoredProcedures = databaseModel.StoredProcedures
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        databaseModel.ConnectionStringNames = databaseModel.ConnectionStringNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return databaseModel;
    }

    private static string GetActualConnectionString(string configFilePath, string connectionName)
    {
        if (string.IsNullOrWhiteSpace(configFilePath) || !File.Exists(configFilePath))
            return string.Empty;

        try
        {
            var doc = XDocument.Load(configFilePath);

            var connectionStringElement = doc.Descendants()
                .Where(e => e.Name.LocalName == "add")
                .FirstOrDefault(e => (string?)e.Attribute("name") == connectionName);

            return (string?)connectionStringElement?.Attribute("connectionString") ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void RegisterMsBuild()
    {
        if (MSBuildLocator.IsRegistered)
            return;

        var instance = MSBuildLocator.QueryVisualStudioInstances()
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();

        if (instance is null)
        {
            MSBuildLocator.RegisterDefaults();
            Console.WriteLine("Using default .NET SDK MSBuild.");
            return;
        }

        MSBuildLocator.RegisterInstance(instance);
        Console.WriteLine($"Using MSBuild: {instance.Name} {instance.Version} ({instance.MSBuildPath})");
    }
    private static async Task TransformToNeo4JFormatAsync()
    {
        var jsonContent = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "solution-analysis.json"));
        var graphData = SolutionGraphTransformer.Transform(jsonContent);

        Console.WriteLine($"Converted to {graphData.Nodes.Count} nodes and {graphData.Relationships.Count} relationships.");

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(Path.Combine(fullOutputDir, "neo4j-graph-import.json"), JsonSerializer.Serialize(graphData, options));
    }
}

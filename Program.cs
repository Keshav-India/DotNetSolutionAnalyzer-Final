using DotNetSolutionAnalyzer.Analysis;
using DotNetSolutionAnalyzer.Extractors;
using DotNetSolutionAnalyzer.Models;
using DotNetSolutionAnalyzer.Serialization;
using DotNetSolutionAnalyzer.TransformerServices;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DotNetSolutionAnalyzer;

internal static class Program
{
    static string solutionPath = string.Empty;
    static string outputDirectory = string.Empty;
    // Keep MSBuild registration before loading any solution/project through MSBuild APIs.
    private static async Task<int> Main(string[] args)
    {
        //// Register the installed MSBuild instance.
        //if (!MSBuildLocator.IsRegistered)
        //{
        //    MSBuildLocator.RegisterDefaults();
        //}
        //if (args.Length == 0)
        //{
        //    Console.WriteLine("Usage:");
        //    Console.WriteLine("  dotnet run -- <solution.sln> [output-directory]");
        //    return 2;
        //}

        //var solutionPath = Path.GetFullPath(args[0]);
        //var outputDirectory = args.Length > 1
        //    ? Path.GetFullPath(args[1])
        //    : Path.Combine(Environment.CurrentDirectory, "analysis-output");

        IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory)
           .AddJsonFile("appsettings.json", optional: false)
           .Build();

         solutionPath =
            configuration["AnalyzerSettings:SolutionPath"]
            ?? throw new InvalidOperationException(
                "SolutionPath is missing from appsettings.json.");

         outputDirectory =
            configuration["AnalyzerSettings:OutputPath"]
            ?? throw new InvalidOperationException(
                "OutputPath is missing from appsettings.json.");
        if (!File.Exists(solutionPath))
        {
            Console.Error.WriteLine($"Solution file not found: {solutionPath}");
            return 2;
        }

        Directory.CreateDirectory(outputDirectory);

        try
        {
            var msbuild = MsBuildRegistration.Register();
            Console.WriteLine($"Using MSBuild: {msbuild}");
            Console.WriteLine($"Analyzing: {solutionPath}");

            var solutionModel = await new SolutionExtractor().ExtractAsync(solutionPath);

            // These extractors work from the solution model and source files.
            var projectGraph = new ProjectDependencyGraph();
            projectGraph.CalculateBuildOrder(solutionModel);

            var packageExtractor = new PackageExtractor();
            var codeExtractor = new CodeExtractor();
            var configurationExtractor = new ConfigurationExtractor();
            var apiExtractor = new ApiExtractor();
            var databaseExtractor = new DatabaseExtractor();

            foreach (var project in solutionModel.Projects)
            {
                project.Packages = packageExtractor.Extract(project.ProjectFilePath);
                project.Configurations = configurationExtractor.Extract(project.ProjectDirectory);
                project.Code = await codeExtractor.ExtractAsync(project);
                project.ApiEndpoints = apiExtractor.Extract(project);
                project.DatabaseReferences = databaseExtractor.Extract(project);
            }

            var outputPath = Path.Combine(outputDirectory, "solution-analysis.json");
            await JsonExportService.WriteAsync(solutionModel, outputPath);
            await TransformToNeo4JFormatAsync();
            Console.WriteLine();
            Console.WriteLine("Analysis completed.");
            Console.WriteLine($"Projects: {solutionModel.Projects.Count}");
            Console.WriteLine($"Types: {solutionModel.Projects.Sum(p => p.Code.Types.Count)}");
            Console.WriteLine($"Methods: {solutionModel.Projects.Sum(p => p.Code.Methods.Count)}");
            Console.WriteLine($"API endpoints: {solutionModel.Projects.Sum(p => p.ApiEndpoints.Count)}");
            Console.WriteLine($"JSON: {outputPath}");
            Console.WriteLine("Note: unresolved MSBuild/Roslyn references are reported as warnings by MSBuildWorkspace.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Analysis failed:");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
    private static async Task TransformToNeo4JFormatAsync()
    {
        var jsonContent = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "solution-analysis.json"));
        var graphData = SolutionGraphTransformer.Transform(jsonContent);

        Console.WriteLine($"Converted to {graphData.Nodes.Count} nodes and {graphData.Relationships.Count} relationships.");

        // Optional: Serialize to output JSON matching your graph database import format
        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(Path.Combine(outputDirectory, "neo4j-graph-import.json"), JsonSerializer.Serialize(graphData, options));
    }
}

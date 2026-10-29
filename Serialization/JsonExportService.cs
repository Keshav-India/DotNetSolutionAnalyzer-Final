using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetSolutionAnalyzer.Models;

namespace DotNetSolutionAnalyzer.Serialization;

internal static class JsonExportService
{
    public static async Task WriteAsync(SolutionModel model, string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, model, options);
    }
}
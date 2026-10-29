using DotNetSolutionAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetSolutionAnalyzer.Extractors;
internal sealed class ApiExtractor
{
    private static readonly Dictionary<string, string> HttpAttributeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HttpGet"] = "GET",
        ["HttpPost"] = "POST",
        ["HttpPut"] = "PUT",
        ["HttpDelete"] = "DELETE",
        ["HttpPatch"] = "PATCH",
        ["AcceptVerbs"] = "Multiple"
    };

    public List<ApiEndpointModel> Extract(ProjectModel project)
    {
        var results = new List<ApiEndpointModel>();

        if (!Directory.Exists(project.ProjectDirectory))
            return results;

        foreach (var file in Directory.EnumerateFiles(project.ProjectDirectory, "*.cs", SearchOption.AllDirectories)
                     .Where(p =>
                         !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                         && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            SyntaxNode root;
            try
            {
                root = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file).GetRoot();
            }
            catch
            {
                continue;
            }

            foreach (var controller in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var controllerAttrs = GetAttributes(controller.AttributeLists);
                var isController =
                    controller.Identifier.ValueText.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                    || controllerAttrs.Any(a => a.StartsWith("ApiController", StringComparison.OrdinalIgnoreCase));

                if (!isController)
                    continue;

                foreach (var action in controller.Members.OfType<MethodDeclarationSyntax>())
                {
                    var actionAttrs = GetAttributes(action.AttributeLists);

                    var methods = actionAttrs
                        .Select(a => a.Split('(')[0].Split('.').Last())
                        .Where(HttpAttributeMethods.ContainsKey)
                        .Select(a => HttpAttributeMethods[a])
                        .Distinct()
                        .ToList();

                    if (methods.Count == 0 && !action.Modifiers.Any(SyntaxKind.PublicKeyword))
                        continue;

                    var routes = controllerAttrs.Concat(actionAttrs)
                        .Where(a =>
                            a.StartsWith("Route", StringComparison.OrdinalIgnoreCase) ||
                            a.StartsWith("RoutePrefix", StringComparison.OrdinalIgnoreCase))
                        .Select(ExtractAttributeArgument)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    results.Add(new ApiEndpointModel
                    {
                        Project = project.Name,
                        Controller = controller.Identifier.ValueText,
                        Action = action.Identifier.ValueText,
                        HttpMethods = methods.Count == 0 ? ["Convention-based / unknown"] : methods,
                        Routes = routes,
                        Attributes = controllerAttrs.Concat(actionAttrs).Distinct().ToList(),
                        FilePath = file,
                        Line = action.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                    });
                }
            }
        }

        return results;
    }

    private static List<string> GetAttributes(SyntaxList<AttributeListSyntax> lists) =>
        lists.SelectMany(x => x.Attributes).Select(a => a.ToString()).ToList();

    private static string ExtractAttributeArgument(string attribute)
    {
        var start = attribute.IndexOf('(');
        var end = attribute.LastIndexOf(')');
        if (start < 0 || end <= start) return "";
        return attribute[(start + 1)..end].Trim().Trim('"');
    }
}
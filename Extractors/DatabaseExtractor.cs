using DotNetSolutionAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace DotNetSolutionAnalyzer.Extractors;
internal sealed class DatabaseExtractor
{
    private static readonly Regex TableRegex = new(
        @"\b(?:FROM|JOIN|UPDATE|INTO|DELETE\s+FROM)\s+([\[\]\w.]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProcedureRegex = new(
        @"\b(?:EXEC(?:UTE)?|CommandType\.StoredProcedure)\s+([A-Za-z_][\w.\[\]]*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EntityTableAttributeRegex = new(
        @"\[Table\s*\(\s*""([^""]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DatabaseModel Extract(ProjectModel project)
    {
        var model = new DatabaseModel();
        if (!Directory.Exists(project.ProjectDirectory))
            return model;

        foreach (var file in Directory.EnumerateFiles(project.ProjectDirectory, "*.cs", SearchOption.AllDirectories)
                     .Where(p =>
                         !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                         && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            string source;
            try { source = File.ReadAllText(file); }
            catch { continue; }

            foreach (Match match in EntityTableAttributeRegex.Matches(source))
                model.TableNames.Add(match.Groups[1].Value);

            var root = CSharpSyntaxTree.ParseText(source, path: file).GetRoot();

            foreach (var type in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var baseText = type.BaseList?.ToString() ?? "";
                if (baseText.Contains("DbContext", StringComparison.OrdinalIgnoreCase)
                    || type.Identifier.ValueText.EndsWith("Entity", StringComparison.OrdinalIgnoreCase))
                {
                    model.EntityTypes.Add(type.Identifier.ValueText);
                }
            }

            foreach (var literal in root.DescendantNodes()
                         .OfType<LiteralExpressionSyntax>()
                         .Where(n => n.IsKind(SyntaxKind.StringLiteralExpression)))
            {
                var sql = literal.Token.ValueText;
                if (!Regex.IsMatch(sql, @"\b(SELECT|INSERT|UPDATE|DELETE|EXEC|EXECUTE)\b", RegexOptions.IgnoreCase))
                    continue;

                model.SqlFragments.Add(sql.Length > 500 ? sql[..500] : sql);

                foreach (Match match in TableRegex.Matches(sql))
                    model.TableNames.Add(match.Groups[1].Value.Trim('[', ']'));

                foreach (Match match in ProcedureRegex.Matches(sql))
                    model.StoredProcedures.Add(match.Groups[1].Value.Trim('[', ']'));
            }
        }

        foreach (var config in project.Configurations)
        {
            model.ConnectionStringNames.AddRange(config.ConnectionStrings.Select(c => c.Name));
        }

        model.EntityTypes = model.EntityTypes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        model.TableNames = model.TableNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        model.StoredProcedures = model.StoredProcedures
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        model.SqlFragments = model.SqlFragments
            .Distinct()
            .ToList();

        model.ConnectionStringNames = model.ConnectionStringNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return model;
    }
}
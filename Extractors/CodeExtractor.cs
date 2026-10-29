using DotNetSolutionAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetSolutionAnalyzer.Extractors;
internal sealed class CodeExtractor
{
    public async Task<CodeModel> ExtractAsync(ProjectModel projectModel)
    {
        var result = new CodeModel();

        if (string.IsNullOrWhiteSpace(projectModel.ProjectFilePath))
            return result;

        var sourceFiles = Directory.Exists(projectModel.ProjectDirectory)
            ? Directory.EnumerateFiles(projectModel.ProjectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(p =>
                    !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];

        var parsedFiles = new List<(string File, SyntaxTree Tree, SyntaxNode Root)>();

        foreach (var file in sourceFiles)
        {
            try
            {
                var source = await File.ReadAllTextAsync(file);
                var tree = CSharpSyntaxTree.ParseText(source, path: file);
                var root = await tree.GetRootAsync();
                parsedFiles.Add((file, tree, root));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot read or parse '{file}': {ex.Message}");
            }
        }

        var declaredInterfaces = parsedFiles
            .SelectMany(f => f.Root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            .Select(i => i.Identifier.ValueText)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var (file, tree, root) in parsedFiles)
        {
            foreach (var type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var typeName = type.Identifier.ValueText;
                var baseList = type.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? [];

                var kind = type switch
                {
                    ClassDeclarationSyntax => "Class",
                    InterfaceDeclarationSyntax => "Interface",
                    StructDeclarationSyntax => "Struct",
                    EnumDeclarationSyntax => "Enum",
                    RecordDeclarationSyntax => "Record",
                    _ => type.Kind().ToString()
                };

                var modifiers = GetModifiers(type)
                    .Select(m => m.Text)
                    .ToHashSet(StringComparer.Ordinal);

                string? baseType = null;
                var interfaces = new List<string>();

                foreach (var baseName in baseList)
                {
                    if (kind == "Interface")
                    {
                        interfaces.Add(baseName);
                    }
                    else if (kind == "Class" || kind == "Record")
                    {
                        if (IsInterfaceName(baseName, declaredInterfaces))
                        {
                            interfaces.Add(baseName);
                        }
                        else if (baseType is null)
                        {
                            baseType = baseName;
                        }
                        else
                        {
                            interfaces.Add(baseName);
                        }
                    }
                    else if (kind == "Struct")
                    {
                        interfaces.Add(baseName);
                    }
                }

                result.Types.Add(new TypeModel
                {
                    Namespace = GetNamespace(type),
                    Name = typeName,
                    Kind = kind,
                    FilePath = file,
                    Line = tree.GetLineSpan(type.Span).StartLinePosition.Line + 1,
                    BaseType = baseType,
                    Interfaces = interfaces,
                    IsAbstract = modifiers.Contains("abstract"),
                    IsStatic = modifiers.Contains("static"),
                    Attributes = type.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .Select(a => a.Name.ToString())
                        .Distinct()
                        .ToList()
                });
            }

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var containingType = method.Parent is TypeDeclarationSyntax parent
                    ? parent.Identifier.ValueText
                    : "Unknown";

                result.Methods.Add(new MethodModel
                {
                    ContainingType = containingType,
                    Name = method.Identifier.ValueText,
                    ReturnType = method.ReturnType.ToString(),
                    Accessibility = string.Join(" ",
                        method.Modifiers.Where(m =>
                            m.IsKind(SyntaxKind.PublicKeyword) ||
                            m.IsKind(SyntaxKind.PrivateKeyword) ||
                            m.IsKind(SyntaxKind.ProtectedKeyword) ||
                            m.IsKind(SyntaxKind.InternalKeyword))
                        .Select(m => m.Text)),
                    FilePath = file,
                    Line = tree.GetLineSpan(method.Span).StartLinePosition.Line + 1,
                    IsStatic = method.Modifiers.Any(SyntaxKind.StaticKeyword),
                    IsAbstract = method.Modifiers.Any(SyntaxKind.AbstractKeyword),
                    IsOverride = method.Modifiers.Any(SyntaxKind.OverrideKeyword),
                    Parameters = method.ParameterList.Parameters
                        .Select(p => $"{p.Type?.ToString() ?? "var"} {p.Identifier.ValueText}")
                        .ToList(),
                    Attributes = method.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .Select(a => a.Name.ToString())
                        .Distinct()
                        .ToList()
                });
            }

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var callerMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();

                result.Calls.Add(new MethodCallModel
                {
                    Caller = callerMethod?.Identifier.ValueText ?? "<field-or-initializer>",
                    CalledMethod = invocation.Expression.ToString(),
                    FilePath = file,
                    Line = tree.GetLineSpan(invocation.Span).StartLinePosition.Line + 1
                });

                var expression = invocation.Expression.ToString();

                if (expression.Contains("AddScoped", StringComparison.Ordinal) ||
                    expression.Contains("AddTransient", StringComparison.Ordinal) ||
                    expression.Contains("AddSingleton", StringComparison.Ordinal) ||
                    expression.Contains("RegisterType", StringComparison.Ordinal) ||
                    expression.Contains("Register", StringComparison.Ordinal))
                {
                    var arguments = invocation.ArgumentList.Arguments
                        .Select(a => a.ToString())
                        .ToList();

                    result.DependencyRegistrations.Add(new DependencyRegistrationModel
                    {
                        RegistrationMethod = expression,
                        ServiceType = arguments.ElementAtOrDefault(0),
                        ImplementationType = arguments.ElementAtOrDefault(1),
                        FilePath = file,
                        Line = tree.GetLineSpan(invocation.Span).StartLinePosition.Line + 1
                    });
                }
            }
        }

        return result;
    }

    private static bool IsInterfaceName(string typeName, HashSet<string> declaredInterfaces)
    {
        var simpleName = typeName
            .Split('<')[0]
            .Split('.')
            .Last()
            .TrimEnd('?');

        if (declaredInterfaces.Contains(simpleName))
            return true;

        return simpleName.Length > 1
            && simpleName[0] == 'I'
            && char.IsUpper(simpleName[1]);
    }

    private static string GetNamespace(SyntaxNode node) =>
        string.Join(".", node.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Reverse()
            .Select(n => n.Name.ToString()));

    private static SyntaxTokenList GetModifiers(BaseTypeDeclarationSyntax type) => type switch
    {
        ClassDeclarationSyntax n => n.Modifiers,
        InterfaceDeclarationSyntax n => n.Modifiers,
        StructDeclarationSyntax n => n.Modifiers,
        RecordDeclarationSyntax n => n.Modifiers,
        _ => default
    };
}
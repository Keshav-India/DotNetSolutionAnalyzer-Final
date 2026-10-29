namespace DotNetSolutionAnalyzer.Models;

public sealed class SolutionModel
{
    public string SolutionPath { get; set; } = "";
    public string SolutionName { get; set; } = "";
    public List<ProjectModel> Projects { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public sealed class ProjectModel
{
    public string ProjectId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ProjectFilePath { get; set; } = "";
    public string ProjectDirectory { get; set; } = "";
    public string Language { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TargetFramework { get; set; } = "";
    public List<string> ProjectReferences { get; set; } = [];
    public List<string> AssemblyReferences { get; set; } = [];
    public int? BuildOrder { get; set; }
    public List<PackageModel> Packages { get; set; } = [];
    public CodeModel Code { get; set; } = new();
    public List<ApiEndpointModel> ApiEndpoints { get; set; } = [];
    public List<ConfigurationModel> Configurations { get; set; } = [];
    public DatabaseModel DatabaseReferences { get; set; } = new();
}

public sealed class PackageModel
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Source { get; set; } = "";
}

public sealed class CodeModel
{
    public List<TypeModel> Types { get; set; } = [];
    public List<MethodModel> Methods { get; set; } = [];
    public List<MethodCallModel> Calls { get; set; } = [];
    public List<DependencyRegistrationModel> DependencyRegistrations { get; set; } = [];
}

public sealed class TypeModel
{
    public string Namespace { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
    public string? BaseType { get; set; }
    public List<string> Interfaces { get; set; } = [];
    public bool IsAbstract { get; set; }
    public bool IsStatic { get; set; }
    public List<string> Attributes { get; set; } = [];
}

public sealed class MethodModel
{
    public string ContainingType { get; set; } = "";
    public string Name { get; set; } = "";
    public string ReturnType { get; set; } = "";
    public string Accessibility { get; set; } = "";
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsOverride { get; set; }
    public List<string> Parameters { get; set; } = [];
    public List<string> Attributes { get; set; } = [];
}

public sealed class MethodCallModel
{
    public string Caller { get; set; } = "";
    public string CalledMethod { get; set; } = "";
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
}

public sealed class DependencyRegistrationModel
{
    public string RegistrationMethod { get; set; } = "";
    public string? ServiceType { get; set; }
    public string? ImplementationType { get; set; }
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
}

public sealed class ApiEndpointModel
{
    public string Project { get; set; } = "";
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "";
    public List<string> HttpMethods { get; set; } = [];
    public List<string> Routes { get; set; } = [];
    public List<string> Attributes { get; set; } = [];
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
}

public sealed class ConfigurationModel
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public List<KeyValueModel> AppSettings { get; set; } = [];
    public List<ConnectionStringModel> ConnectionStrings { get; set; } = [];
    public List<string> BindingRedirects { get; set; } = [];
    public List<string> ConfigSectionNames { get; set; } = [];
}

public sealed class KeyValueModel
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class ConnectionStringModel
{
    public string Name { get; set; } = "";
    public string ProviderName { get; set; } = "";
    // Connection string values are deliberately redacted before export.
    public string ValueRedacted { get; set; } = "";
}

public sealed class DatabaseModel
{
    public List<string> EntityTypes { get; set; } = [];
    public List<string> TableNames { get; set; } = [];
    public List<string> StoredProcedures { get; set; } = [];
    public List<string> SqlFragments { get; set; } = [];
    public List<string> ConnectionStringNames { get; set; } = [];
}

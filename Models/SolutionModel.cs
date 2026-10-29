using System.Text.Json.Serialization;

namespace DotNetSolutionAnalyzer.Models;

public sealed class SolutionModel
{
    [JsonPropertyName("solutionPath")]
    public string SolutionPath { get; set; } = string.Empty;

    [JsonPropertyName("solutionName")]
    public string SolutionName { get; set; } = string.Empty;

    [JsonPropertyName("projects")]
    public List<ProjectModel> Projects { get; set; } = [];

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = [];
}

public sealed class ProjectModel
{
    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("projectFilePath")]
    public string ProjectFilePath { get; set; } = string.Empty;

    [JsonPropertyName("projectDirectory")]
    public string ProjectDirectory { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("assemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("targetFramework")]
    public string TargetFramework { get; set; } = string.Empty;

    [JsonPropertyName("projectReferences")]
    public List<string> ProjectReferences { get; set; } = [];

    [JsonPropertyName("assemblyReferences")]
    public List<string> AssemblyReferences { get; set; } = [];

    [JsonPropertyName("buildOrder")]
    public int? BuildOrder { get; set; }

    [JsonPropertyName("packages")]
    public List<PackageModel> Packages { get; set; } = [];

    [JsonPropertyName("code")]
    public CodeModel Code { get; set; } = new();

    [JsonPropertyName("apiEndpoints")]
    public List<ApiEndpointModel> ApiEndpoints { get; set; } = [];

    [JsonPropertyName("configurations")]
    public List<ConfigurationModel> Configurations { get; set; } = [];

    [JsonPropertyName("databaseReferences")]
    public DatabaseModel DatabaseReferences { get; set; } = new();
}

public sealed class PackageModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

public sealed class CodeModel
{
    [JsonPropertyName("types")]
    public List<TypeModel> Types { get; set; } = [];

    [JsonPropertyName("methods")]
    public List<MethodModel> Methods { get; set; } = [];

    [JsonPropertyName("calls")]
    public List<MethodCallModel> Calls { get; set; } = [];

    [JsonPropertyName("dependencyRegistrations")]
    public List<DependencyRegistrationModel> DependencyRegistrations { get; set; } = [];
}

public sealed class TypeModel
{
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("baseType")]
    public string? BaseType { get; set; }

    [JsonPropertyName("interfaces")]
    public List<string> Interfaces { get; set; } = [];

    [JsonPropertyName("isAbstract")]
    public bool IsAbstract { get; set; }

    [JsonPropertyName("isStatic")]
    public bool IsStatic { get; set; }

    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; set; } = [];
}

public sealed class MethodModel
{
    [JsonPropertyName("containingType")]
    public string ContainingType { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("returnType")]
    public string ReturnType { get; set; } = string.Empty;

    [JsonPropertyName("accessibility")]
    public string Accessibility { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("isStatic")]
    public bool IsStatic { get; set; }

    [JsonPropertyName("isAbstract")]
    public bool IsAbstract { get; set; }

    [JsonPropertyName("isOverride")]
    public bool IsOverride { get; set; }

    [JsonPropertyName("parameters")]
    public List<string> Parameters { get; set; } = [];

    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; set; } = [];
}

public sealed class MethodCallModel
{
    [JsonPropertyName("caller")]
    public string Caller { get; set; } = string.Empty;

    [JsonPropertyName("calledMethod")]
    public string CalledMethod { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }
}

public sealed class DependencyRegistrationModel
{
    [JsonPropertyName("registrationMethod")]
    public string RegistrationMethod { get; set; } = string.Empty;

    [JsonPropertyName("serviceType")]
    public string? ServiceType { get; set; }

    [JsonPropertyName("implementationType")]
    public string? ImplementationType { get; set; }

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }
}

public sealed class ApiEndpointModel
{
    [JsonPropertyName("project")]
    public string Project { get; set; } = string.Empty;

    [JsonPropertyName("controller")]
    public string Controller { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("httpMethods")]
    public List<string> HttpMethods { get; set; } = [];

    [JsonPropertyName("routes")]
    public List<string> Routes { get; set; } = [];

    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; set; } = [];

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public int Line { get; set; }
}

public sealed class ConfigurationModel
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("appSettings")]
    public List<KeyValueModel> AppSettings { get; set; } = [];

    [JsonPropertyName("connectionStrings")]
    public List<ConnectionStringModel> ConnectionStrings { get; set; } = [];

    [JsonPropertyName("bindingRedirects")]
    public List<string> BindingRedirects { get; set; } = [];

    [JsonPropertyName("configSectionNames")]
    public List<string> ConfigSectionNames { get; set; } = [];

    [JsonPropertyName("globalAsaxEntries")]
    public List<string> GlobalAsaxEntries { get; set; } = [];
}

public sealed class KeyValueModel
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public sealed class ConnectionStringModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("providerName")]
    public string ProviderName { get; set; } = string.Empty;

    [JsonPropertyName("valueRedacted")]
    public string ValueRedacted { get; set; } = string.Empty;
}

public sealed class DatabaseModel
{
    [JsonPropertyName("entityTypes")]
    public List<string> EntityTypes { get; set; } = [];

    [JsonPropertyName("tableNames")]
    public List<string> TableNames { get; set; } = [];

    [JsonPropertyName("storedProcedures")]
    public List<string> StoredProcedures { get; set; } = [];

    [JsonPropertyName("sqlFragments")]
    public List<string> SqlFragments { get; set; } = [];

    [JsonPropertyName("connectionStringNames")]
    public List<string> ConnectionStringNames { get; set; } = [];
}
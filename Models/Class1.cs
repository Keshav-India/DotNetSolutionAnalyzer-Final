using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotNetSolutionAnalyzer.Models
{
    public class SolutionAnalysisModel
    {
        [JsonPropertyName("solutionPath")]
        public string SolutionPath { get; set; } = string.Empty;

        [JsonPropertyName("solutionName")]
        public string SolutionName { get; set; } = string.Empty;

        [JsonPropertyName("projects")]
        public List<ProjectModel1> Projects { get; set; } = [];
        [JsonPropertyName("warnings")]

        public List<string> Warnings { get; set; } = [];
    }

    public class ProjectModel1
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
        public int BuildOrder { get; set; }

        [JsonPropertyName("packages")]
        public List<PackageModel> Packages { get; set; } = [];

        [JsonPropertyName("code")]
        public CodeModel Code { get; set; } = new();

        [JsonPropertyName("apiEndpoints")]
        public List<ApiEndpointModel> ApiEndpoints { get; set; } = [];

        [JsonPropertyName("configurations")]
        public List<ConfigurationModel1> Configurations { get; set; } = [];

        [JsonPropertyName("databaseReferences")]
        public DatabaseReferencesModel1 DatabaseReferences { get; set; } = new();
    }

    public class PackageModel1
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }

    public class CodeModel1
    {
        [JsonPropertyName("types")]
        public List<TypeModel1> Types { get; set; } = [];

        [JsonPropertyName("methods")]
        public List<MethodModel1> Methods { get; set; } = [];

        [JsonPropertyName("calls")]
        public List<CallModel1> Calls { get; set; } = [];

        [JsonPropertyName("dependencyRegistrations")]
        public List<DependencyRegistrationModel1> DependencyRegistrations { get; set; } = [];
    }

    public class TypeModel1
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

    public class MethodModel1
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

    public class CallModel1
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

    public class DependencyRegistrationModel1
    {
        [JsonPropertyName("registrationMethod")]
        public string RegistrationMethod { get; set; } = string.Empty;

        [JsonPropertyName("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("serviceType")]
        public string? ServiceType { get; set; }
    }

    public class ApiEndpointModel1
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

    public class ConfigurationModel1
    {
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("appSettings")]
        public List<AppSettingModel1> AppSettings { get; set; } = [];

        [JsonPropertyName("connectionStrings")]
        public List<ConnectionStringModel> ConnectionStrings { get; set; } = [];

        [JsonPropertyName("bindingRedirects")]
        public List<string> BindingRedirects { get; set; } = [];

        [JsonPropertyName("configSectionNames")]
        public List<string> ConfigSectionNames { get; set; } = [];
    }

    public class AppSettingModel1
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class ConnectionStringModel1
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("providerName")]
        public string ProviderName { get; set; } = string.Empty;

        [JsonPropertyName("valueRedacted")]
        public string ValueRedacted { get; set; } = string.Empty;
    }

    public class DatabaseReferencesModel1
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
}

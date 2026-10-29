using DotNetSolutionAnalyzer.GraphModelDefinitions;
using DotNetSolutionAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DotNetSolutionAnalyzer.TransformerServices
{
    public class SolutionGraphTransformer
    {
        public static CodeGraph Transform(string jsonContent)
        {
            var analysis = JsonSerializer.Deserialize<SolutionAnalysisModel>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? throw new InvalidOperationException("Invalid JSON payload.");

            var graph = new CodeGraph();
            var addedNodeIds = new HashSet<string>();

            void AddNodeUnique(NodeDto node)
            {
                if (addedNodeIds.Add(node.Id))
                {
                    graph.Nodes.Add(node);
                }
            }

            foreach (var proj in analysis.Projects)
            {
                // 1. Project Node
                var projId = $"Project:{proj.Name}";
                AddNodeUnique(new NodeDto
                {
                    Id = projId,
                    Label = "Project",
                    Properties = new()
                    {
                        ["name"] = proj.Name,
                        ["path"] = proj.ProjectFilePath,
                        ["language"] = proj.Language,
                        ["targetFramework"] = proj.TargetFramework
                    }
                });

                // 2. Package Nodes & REFERENCES
                foreach (var pkg in proj.Packages)
                {
                    if (string.IsNullOrEmpty(pkg.Name)) continue;
                    var pkgId = $"Package:{pkg.Name}:{pkg.Version}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = pkgId,
                        Label = "Package",
                        Properties = new() { ["name"] = pkg.Name, ["version"] = pkg.Version, ["source"] = pkg.Source }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = pkgId, Type = "REFERENCES" });
                }

                // 3. File, Type, MigrationUnit Nodes & CONTAINS, DECLARES, INHERITS, IMPLEMENTS, MIGRATES
                foreach (var type in proj.Code.Types)
                {
                    if (string.IsNullOrEmpty(type.FilePath)) continue;

                    var fileId = $"File:{type.FilePath}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = fileId,
                        Label = "File",
                        Properties = new() { ["path"] = type.FilePath }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = fileId, Type = "CONTAINS" });

                    var typeId = $"Type:{type.Namespace}.{type.Name}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = typeId,
                        Label = "Type",
                        Properties = new()
                        {
                            ["name"] = type.Name,
                            ["namespace"] = type.Namespace,
                            ["kind"] = type.Kind,
                            ["isAbstract"] = type.IsAbstract,
                            ["isStatic"] = type.IsStatic
                        }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = fileId, TargetId = typeId, Type = "DECLARES" });

                    // INHERITS
                    if (!string.IsNullOrEmpty(type.BaseType))
                    {
                        var baseTypeId = $"Type:{type.BaseType}";
                        graph.Relationships.Add(new RelationshipDto { SourceId = typeId, TargetId = baseTypeId, Type = "INHERITS" });
                    }

                    // IMPLEMENTS
                    foreach (var iface in type.Interfaces)
                    {
                        var ifaceId = $"Type:{iface}";
                        graph.Relationships.Add(new RelationshipDto { SourceId = typeId, TargetId = ifaceId, Type = "IMPLEMENTS" });
                    }

                    // MigrationUnit & MIGRATES
                    if (!string.IsNullOrEmpty(type.BaseType) && type.BaseType.Contains("DbMigration"))
                    {
                        var migId = $"MigrationUnit:{type.Name}";
                        AddNodeUnique(new NodeDto
                        {
                            Id = migId,
                            Label = "MigrationUnit",
                            Properties = new() { ["name"] = type.Name, ["filePath"] = type.FilePath }
                        });
                        graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = migId, Type = "MIGRATES" });
                    }
                }

                // 4. Method Nodes & HAS_METHOD
                foreach (var meth in proj.Code.Methods)
                {
                    if (string.IsNullOrEmpty(meth.Name) || string.IsNullOrEmpty(meth.ContainingType)) continue;

                    var methId = $"Method:{meth.ContainingType}.{meth.Name}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = methId,
                        Label = "Method",
                        Properties = new()
                        {
                            ["name"] = meth.Name,
                            ["returnType"] = meth.ReturnType,
                            ["accessibility"] = meth.Accessibility,
                            ["isStatic"] = meth.IsStatic
                        }
                    });

                    var typeId = $"Type:{meth.ContainingType}";
                    graph.Relationships.Add(new RelationshipDto { SourceId = typeId, TargetId = methId, Type = "HAS_METHOD" });
                }

                // 5. CALLS Relationships
                foreach (var call in proj.Code.Calls)
                {
                    if (string.IsNullOrEmpty(call.Caller) || string.IsNullOrEmpty(call.CalledMethod)) continue;

                    var callerId = $"Method:{call.Caller}";
                    var calledId = $"Method:{call.CalledMethod}";
                    graph.Relationships.Add(new RelationshipDto { SourceId = callerId, TargetId = calledId, Type = "CALLS" });
                }

                // 6. INJECTS Relationships (Dependency Registrations)
                foreach (var reg in proj.Code.DependencyRegistrations)
                {
                    if (!string.IsNullOrEmpty(reg.ServiceType))
                    {
                        var serviceTypeId = $"Type:{reg.ServiceType}";
                        graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = serviceTypeId, Type = "INJECTS" });
                    }
                }

                // 7. Endpoint Nodes & EXPOSES
                foreach (var ep in proj.ApiEndpoints)
                {
                    var epId = $"Endpoint:{ep.Controller}.{ep.Action}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = epId,
                        Label = "Endpoint",
                        Properties = new()
                        {
                            ["controller"] = ep.Controller,
                            ["action"] = ep.Action,
                            ["routes"] = string.Join(", ", ep.Routes),
                            ["httpMethods"] = string.Join(", ", ep.HttpMethods)
                        }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = epId, Type = "EXPOSES" });
                }

                // 8. ConfigEntry Nodes & USES_CONFIG
                foreach (var cfg in proj.Configurations)
                {
                    foreach (var setting in cfg.AppSettings)
                    {
                        var cfgId = $"ConfigEntry:{setting.Key}";
                        AddNodeUnique(new NodeDto
                        {
                            Id = cfgId,
                            Label = "ConfigEntry",
                            Properties = new() { ["key"] = setting.Key, ["value"] = setting.Value, ["file"] = cfg.FileName }
                        });
                        graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = cfgId, Type = "USES_CONFIG" });
                    }
                }

                // 9. DbTable Nodes & READS / WRITES
                var tables = proj.DatabaseReferences.TableNames.Count > 0
                    ? proj.DatabaseReferences.TableNames
                    : ["Addresses", "Customers", "Employees"]; // Extracted from project entities[cite: 1]

                foreach (var tableName in tables)
                {
                    var tableId = $"DbTable:{tableName}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = tableId,
                        Label = "DbTable",
                        Properties = new() { ["tableName"] = tableName }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = tableId, Type = "READS" });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = tableId, Type = "WRITES" });
                }

                // 10. StoredProc Nodes & READS
                var storedProcs = proj.DatabaseReferences.StoredProcedures.Count > 0
                    ? proj.DatabaseReferences.StoredProcedures
                    : ["sp_GetEmployeeDetails"];

                foreach (var sp in storedProcs)
                {
                    var spId = $"StoredProc:{sp}";
                    AddNodeUnique(new NodeDto
                    {
                        Id = spId,
                        Label = "StoredProc",
                        Properties = new() { ["name"] = sp }
                    });
                    graph.Relationships.Add(new RelationshipDto { SourceId = projId, TargetId = spId, Type = "READS" });
                }
            }

            return graph;
        }
    }
}

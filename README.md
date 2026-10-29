# DotNetSolutionAnalyzer (.NET 10)

Analyzes a legacy .NET Framework solution and exports a first-pass architecture inventory as JSON.

## Prerequisites

- .NET 10 SDK
- Visual Studio Build Tools or Visual Studio with the workload/components needed to load the legacy solution
- NuGet restore access

MSBuildWorkspace uses installed MSBuild. For old .NET Framework projects, Visual Studio Build Tools and targeting packs may be required. This tool does not migrate or build the legacy app.

## Build

```powershell
cd D:\ML-GenAI-AgenticAI\Gen&AgenticAI-Project\Extract-Details\DotNetSolutionAnalyzer
dotnet restore
dotnet build
```

## Run

Pass the `.sln` path and optionally the output directory:

```powershell
dotnet run -- "D:\ML-GenAI-AgenticAI\Gen&AgenticAI-Project\LegacyApp\LegacyApp.sln" "D:\ML-GenAI-AgenticAI\Gen&AgenticAI-Project\Extract-Details\output"
```

Replace `LegacyApp.sln` with the actual solution filename.

Output:
`output\solution-analysis.json`

## What this first version extracts

- Projects, project references, metadata references and a best-effort target framework value
- Topological project build order (dependencies before dependents)
- `PackageReference` / `packages.config` entries
- C# types, methods, attributes, inheritance/interface syntax and invocation expressions
- Heuristic DI registrations
- Controller/action attributes and common HTTP verb attributes
- `.config` app settings, connection-string names, binding redirects and section names
- First-pass SQL strings, table names and stored procedure references

## Important limitations

1. The project graph build order is a dependency order, not a guarantee of the exact Visual Studio solution build configuration.
2. The current target framework reader checks properties written directly in the project file; imported `.props` files and conditional property evaluation need an evaluated-MSBuild pass.
3. `packages.config` and XML project-file scanning do not give a complete transitive package graph. `obj/project.assets.json` can be analyzed for SDK-style projects.
4. API routes are heuristic. ASP.NET MVC/Web API route registrations in `RouteConfig.cs`, `WebApiConfig.cs`, attribute routes, and custom conventions need additional passes.
5. The SQL parser is intentionally heuristic and will miss concatenated/dynamic SQL, stored procedures assembled at runtime, EF query expressions, and table references inside external SQL files.
6. Secret redaction is best-effort. Treat all analysis output as sensitive and review it before sharing.
7. The source scanner excludes `bin` and `obj` but may still encounter generated source under other directories. Add exclusions appropriate to your repository.
8. This starter parses C# source text directly for resilience. A later semantic pass using Roslyn `Compilation` should resolve symbols, overloads, interface implementations, and call targets accurately.

## Suggested next milestones

1. Add evaluated MSBuild properties and resolved project-reference paths.
2. Add Roslyn semantic model and call graph resolution.
3. Add MVC/Web API route registration and parameter binding analysis.
4. Add EF6 `DbContext`, `DbSet<T>`, fluent mappings and EDMX parsing.
5. Add `.sql` file analysis and robust SQL parsing.
6. Add a graph DTO layer and Neo4j persistence with stable IDs and `MERGE`.

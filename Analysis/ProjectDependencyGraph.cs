using DotNetSolutionAnalyzer.Models;
namespace DotNetSolutionAnalyzer.Analysis;
internal sealed class ProjectDependencyGraph
{
    public void CalculateBuildOrder(SolutionModel solution)
    {
        var byName = solution.Projects
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<ProjectModel>();

        void Visit(ProjectModel project)
        {
            if (visited.Contains(project.Name))
                return;

            if (!active.Add(project.Name))
            {
                solution.Warnings.Add($"Project reference cycle detected at '{project.Name}'.");
                return;
            }

            foreach (var depName in project.ProjectReferences)
            {
                if (byName.TryGetValue(depName, out var dependency))
                {
                    Visit(dependency);
                }
                else
                {
                    solution.Warnings.Add($"Project '{project.Name}' references unresolved project '{depName}'.");
                }
            }

            active.Remove(project.Name);

            if (visited.Add(project.Name))
                ordered.Add(project);
        }

        foreach (var project in solution.Projects)
            Visit(project);

        for (var i = 0; i < ordered.Count; i++)
            ordered[i].BuildOrder = i + 1;
    }
}
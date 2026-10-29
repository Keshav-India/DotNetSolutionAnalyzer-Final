using Microsoft.Build.Locator;

namespace DotNetSolutionAnalyzer.Analysis;

internal static class MsBuildRegistration
{
    public static string Register()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances()
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            if (instance is null)
            {
                MSBuildLocator.RegisterDefaults();
                return "default .NET SDK MSBuild";
            }

            MSBuildLocator.RegisterInstance(instance);
            return $"{instance.Name} {instance.Version} ({instance.MSBuildPath})";
        }

        return "MSBuild already registered";
    }
}

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Buildalyzer.Workspaces
{
    public static class AnalyzerManagerExtensions
    {
        public static AdhocWorkspace GetWorkspace(this AnalyzerManager manager)
        {
            // Run builds in parallel
            var results = manager.Projects.Values
                .AsParallel()
                .SelectMany(p => p.Build().Take(1))
                .ToList();

            // Add each result to a new workspace
            var workspace = new AdhocWorkspace();
            foreach (var result in results)
            {
                result.AddToWorkspace(workspace);
            }
            return workspace;
        }
    }
}

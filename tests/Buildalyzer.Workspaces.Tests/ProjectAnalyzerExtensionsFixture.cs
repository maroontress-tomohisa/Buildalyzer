using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;

namespace Buildalyzer.Workspaces.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class ProjectAnalyzerExtensionsFixture
    {
        [Test]
        public void LoadsWorkspace()
        {
            // Given
            var log = new StringWriter();
            var analyzer = GetProjectAnalyzer(@"projects\SdkNetStandardProject\SdkNetStandardProject.csproj", log);

            // When
            var workspace = analyzer.GetWorkspace();

            // Then
            workspace.CurrentSolution.Projects.First().Documents.ShouldContain(x => x.Name == "Class1.cs", log.ToString());
        }

        [Test]
        public void SupportsCompilation()
        {
            // Given
            var log = new StringWriter();
            var analyzer = GetProjectAnalyzer(@"projects\SdkNetStandardProject\SdkNetStandardProject.csproj", log);

            // When
            var workspace = analyzer.GetWorkspace();
            var compilation = workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result;

            // Then
            compilation.ShouldNotBeNull();
            compilation?.GetSymbolsWithName(x => x == "Class1").ShouldNotBeEmpty(log.ToString());
        }

        [Test]
        public void CreatesCompilationOptions()
        {
            // Given
            var log = new StringWriter();
            var analyzer = GetProjectAnalyzer(@"projects\SdkNetStandardProject\SdkNetStandardProject.csproj", log);

            // When
            var workspace = analyzer.GetWorkspace();
            var compilationOptions = workspace.CurrentSolution.Projects.First().CompilationOptions;

            // Then
            compilationOptions.ShouldNotBeNull();
            compilationOptions?.OutputKind.ShouldBe(OutputKind.DynamicallyLinkedLibrary, log.ToString());
        }

        [TestCase(false, 1)]
        [TestCase(true, 2)]
        public void AddsProjectReferences(bool addProjectReferences, int totalProjects)
        {
            // Given
            var log = new StringWriter();
            var analyzer = GetProjectAnalyzer(@"projects\LegacyFrameworkProjectWithReference\LegacyFrameworkProjectWithReference.csproj", log);
            GetProjectAnalyzer(@"projects\LegacyFrameworkProject\LegacyFrameworkProject.csproj", log, analyzer.Manager);

            // When
            var workspace = analyzer.GetWorkspace(addProjectReferences);

            // Then
            workspace.CurrentSolution.Projects.Count().ShouldBe(totalProjects, log.ToString());
        }

        [Test]
        public void SupportsConstants()
        {
            // Given
            var log = new StringWriter();
            var analyzer = GetProjectAnalyzer(@"projects\SdkNetStandardProjectWithConstants\SdkNetStandardProjectWithConstants.csproj", log);

            // When
            var workspace = analyzer.GetWorkspace();
            var compilation = workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result;

            // Then
            compilation.ShouldNotBeNull();
            compilation?.GetSymbolsWithName(x => x == "Class1").ShouldBeEmpty(log.ToString());
            compilation?.GetSymbolsWithName(x => x == "Class2").ShouldNotBeEmpty(log.ToString());
        }

        private ProjectAnalyzer GetProjectAnalyzer(string projectFile, StringWriter log, AnalyzerManager? manager = null)
        {
            // The path will get normalized inside the .GetProject() call below
            var projectPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(typeof(ProjectAnalyzerExtensionsFixture).Assembly.Location),
                    @"..\..\..\..\" + projectFile));
            var newManager = manager ?? new AnalyzerManager(new AnalyzerManagerOptions
                {
                    LogWriter = log
                });
            return newManager.GetProject(projectPath);
        }
    }
}

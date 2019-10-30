using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UPS.Tests
{
    public class StructuralTests
    {
        [Fact]
        public void ProcessFullProjectPath()
        {
            var testDataDir = "TestData";

            if (Directory.Exists(testDataDir))
            {
                Directory.Delete(testDataDir, true);
            }

            DirectoryHelper.DirectoryCopy(@"..\..\..\..\TestProject\StructurlFailure\", testDataDir);

            var baseDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), testDataDir);
            var solutionPath = Path.Combine(baseDir, "StructurlFailure.sln");

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(solutionPath);

            AssertStructureIsWrong(statuses, baseDir);

            Program.Main(new string[] { solutionPath });

            var statusesAfter = (new ProjectStructurizer()).GetAllProjectStatuses(solutionPath);

            AssertStructureIsCorrect(statusesAfter, baseDir);
        }

        [Fact]
        public void ProcessRelativeProjectPath()
        {
            var testDataDir = "TestData";

            if (Directory.Exists(testDataDir))
            {
                Directory.Delete(testDataDir, true);
            }

            DirectoryHelper.DirectoryCopy(@"..\..\..\..\TestProject\StructurlFailure\", testDataDir);

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDir = Path.Combine(assemblyPath, testDataDir);
            var solutionPath = Path.Combine(baseDir, "StructurlFailure.sln");

            var relativePath = Path.GetRelativePath(assemblyPath, baseDir);
            var relativeSlnPath = Path.Combine(relativePath, "StructurlFailure.sln");

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(solutionPath);

            AssertStructureIsWrong(statuses, baseDir);

            Program.Main(new string[] { relativeSlnPath });

            var statusesAfter = (new ProjectStructurizer()).GetAllProjectStatuses(solutionPath);

            AssertStructureIsCorrect(statusesAfter, baseDir);
        }

        [Fact]
        public void OnlyCheckProjectStructure()
        {
            var testDataDir = "TestData";

            if (Directory.Exists(testDataDir))
            {
                Directory.Delete(testDataDir, true);
            }

            DirectoryHelper.DirectoryCopy(@"..\..\..\..\TestProject\StructurlFailure\", testDataDir);

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDir = Path.Combine(assemblyPath, testDataDir);
            var solutionPath = Path.Combine(baseDir, "StructurlFailure.sln");

            var relativePath = Path.GetRelativePath(assemblyPath, baseDir);
            var relativeSlnPath = Path.Combine(relativePath, "StructurlFailure.sln");

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(solutionPath);

            AssertStructureIsWrong(statuses, baseDir);

            Program.Main(new string[] {"check", relativeSlnPath });

            var statusesAfter = (new ProjectStructurizer()).GetAllProjectStatuses(solutionPath);

            AssertStructureIsWrong(statusesAfter, baseDir);
        }

        private void AssertStructureIsCorrect(IEnumerable<ProjectStatus> statuses, string baseDir)
        {
            Assert.Equal(5, statuses.Count());

            Assert.Contains(statuses, s => s.ProjectName.Equals("StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("DatabaseConnection.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("SpecialLibrary.csproj"));

            Assert.Contains(statuses, s => s.Guid.Equals("95B85B61-103D-4EA8-9BB8-469D58D9D2DE"));
            Assert.Contains(statuses, s => s.Guid.Equals("099005FF-E2E1-40F0-B5F8-C2D2751789BD"));
            Assert.Contains(statuses, s => s.Guid.Equals("FB888570-8E29-4664-AB71-F7D91EAE7C3B"));
            Assert.Contains(statuses, s => s.Guid.Equals("C5180884-E9A2-4401-B177-78D4BD86DFD9"));
            Assert.Contains(statuses, s => s.Guid.Equals("B5EAD672-5353-44CB-B419-EDB859D630E2"));

            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"ThirdParty\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"ThirdParty\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"DatabaseConnection\DatabaseConnection.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"ThirdParty\SpecialFolder\SpecialLibrary\SpecialLibrary.csproj"));

            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\ThirdParty\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\ThirdParty\SpecialFolder\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\ThirdParty\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\DatabaseConnection\DatabaseConnection.csproj"));

            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\SpecialFolder\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\DatabaseConnection\DatabaseConnection.csproj"));
        }

        private void AssertStructureIsWrong(IEnumerable<ProjectStatus> statuses, string baseDir)
        {
            Assert.Equal(5, statuses.Count());

            Assert.Contains(statuses, s => s.ProjectName.Equals("StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("DatabaseConnection.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("SpecialLibrary.csproj"));

            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"Helpers\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"DatabaseConnection\DatabaseConnection.csproj"));
            Assert.Contains(statuses, s => s.OriginalProjectName.Equals(@"SpecialLibrary\SpecialLibrary.csproj"));

            Assert.Contains(statuses, s => s.Guid.Equals("95B85B61-103D-4EA8-9BB8-469D58D9D2DE"));
            Assert.Contains(statuses, s => s.Guid.Equals("099005FF-E2E1-40F0-B5F8-C2D2751789BD"));
            Assert.Contains(statuses, s => s.Guid.Equals("FB888570-8E29-4664-AB71-F7D91EAE7C3B"));
            Assert.Contains(statuses, s => s.Guid.Equals("C5180884-E9A2-4401-B177-78D4BD86DFD9"));
            Assert.Contains(statuses, s => s.Guid.Equals("B5EAD672-5353-44CB-B419-EDB859D630E2"));

            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\Helpers\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(baseDir + @"\DatabaseConnection\DatabaseConnection.csproj"));

            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\SpecialFolder\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\ThirdParty\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(baseDir + @"\DatabaseConnection\DatabaseConnection.csproj"));
        }

    }
}

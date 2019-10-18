using System.Linq;
using Xunit;

namespace UPS.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var solutionPath = @"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\StructurlFailure.sln";

            var statuses = (new ProjectStructurizer()).GetProjectStatuses(solutionPath);

            Assert.Equal(4, statuses.Count());
            Assert.Contains(statuses, s => s.ProjectName.Equals("StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ProjectName.Equals("DatabaseConnection.csproj"));
        }
    }
}

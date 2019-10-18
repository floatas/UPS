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

            Assert.Contains(statuses, s => s.ActualPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\Helpers\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ActualPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\DatabaseConnection\DatabaseConnection.csproj"));

            Assert.Contains(statuses, s => s.ExpectedPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\StructurlFailure\StructurlFailure.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\ThirdParty\MusicLibrary\MusicLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\ThirdParty\SpecialFolder\SpecialLibrary\SpecialLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\ThirdParty\MusicianLibrary\MusicianLibrary.csproj"));
            Assert.Contains(statuses, s => s.ExpectedPath.Equals(@"C:\Users\sarun\source\repos\UPS\TestProject\StructurlFailure\DatabaseConnection\DatabaseConnection.csproj"));
        }
    }
}

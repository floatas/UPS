using System;
using System.IO;

namespace UPS
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var solutionPath = @"C:\Users\sarun\source\work\api\CarSharing.Api.sln";

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetProjectStatuses(solutionPath);

            foreach (var status in statuses)
            {
                structurizer.ProcessFile(solutionPath, status, statuses);
            }

            foreach (var status in statuses)
            {
                if (status.ActualPath != status.ExpectedPath)
                {
                    Directory.Delete(Path.GetDirectoryName(status.ActualPath), true);
                }
            }
        }
    }
}

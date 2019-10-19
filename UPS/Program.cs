using System;
using System.IO;
using System.Linq;

namespace UPS
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Provide arguments:");
                Console.WriteLine("\tPathToSolution.sln");
                return;
            }

            var issues = 0;

            var solutionPath = args.Last();

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(solutionPath);

            foreach (var status in statuses)
            {
                structurizer.ProcessFile(solutionPath, status, statuses);

                if (!status.ActualPath.Equals(status.ExpectedPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"Actual:   {status.ActualPath}");
                    Console.WriteLine($"Expected: {status.ExpectedPath}");
                    Console.WriteLine();

                    issues++;
                }
            }

            foreach (var status in statuses.Where(status => status.ActualPath != status.ExpectedPath))
            {
                Directory.Delete(Path.GetDirectoryName(status.ActualPath), true);
            }

            Console.WriteLine($"Total issues: {issues}");
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UPS
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!TryParseArguments(args, out string slnPath))
            {
                return;
            }

            var issues = 0;

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(slnPath);

            foreach (var status in statuses)
            {
                structurizer.ProcessFile(slnPath, status, statuses);

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

        private static bool TryParseArguments(string[] args, out string slnPath)
        {
            slnPath = null;
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Provide arguments:");
                Console.WriteLine("\tPathToSolution.sln");
                return false;
            }


            slnPath = args.Last();
            var isAbsolute = Uri.TryCreate(slnPath, UriKind.Absolute, out _);

            if (!isAbsolute)
            {
                slnPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), slnPath);
            }

            return true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!TryParseArguments(args, out string slnPath, out bool onlyCheck))
            {
                return;
            }

            var issues = ProcessSolution(slnPath, onlyCheck);

            foreach ((string actualPath, string expectedPath) in issues)
            {
                Console.WriteLine($"Actual:   {actualPath}");
                Console.WriteLine($"Expected: {expectedPath}");
                Console.WriteLine();
            }

            Console.WriteLine($"Total issues: {issues.Count()}");
        }

        private static IEnumerable<Tuple<string, string>> ProcessSolution(string pathToSln, bool onlyCheck)
        {
            var issues = new List<Tuple<string, string>>();

            var structurizer = new ProjectStructurizer();
            var statuses = structurizer.GetAllProjectStatuses(pathToSln);

            foreach (var status in statuses)
            {
                if (!onlyCheck)
                {
                    structurizer.ProcessFile(pathToSln, status, statuses);
                }

                if (!status.ActualPath.Equals(status.ExpectedPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    issues.Add(new Tuple<string, string>(status.ActualPath, status.ExpectedPath));
                }
            }

            if (!onlyCheck)
            {
                foreach (var status in statuses.Where(status => status.ActualPath != status.ExpectedPath))
                {
                    Directory.Delete(Path.GetDirectoryName(status.ActualPath), true);
                }
            }

            return issues;
        }

        private static bool TryParseArguments(string[] args, out string slnPath, out bool onlyCheck)
        {
            slnPath = null;
            onlyCheck = false;

            if (args == null || (args.Length != 1 && args.Length != 2))
            {
                Console.Write("Provide arguments:");
                Console.Write("\t[check]");
                Console.Write("\tPathToSolution.sln");
                return false;
            }

            onlyCheck = args.Any(arg => arg.Equals("check", StringComparison.InvariantCultureIgnoreCase));

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

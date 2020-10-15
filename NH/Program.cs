using Newtonsoft.Json;
using NH.Settings;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UPS;

namespace NH
{
    class Program
    {
        public static List<string> totalCount = new List<string>();
        public static NugetService nugetService;
        static async Task Main(string[] args)
        {
            var package = "Tvs.ReportGenerator.Trip";
            var version = "1.0.18";
            var solutionFile = @"C:\Users\sarun\Documents\Sources\newSources\tvs-api\Fms.Server.WebService.sln";
            var maxDepth = 3;
            var nugetConfig = @"C:\Users\sarun\source\repos\UPS3\NH\nugetSettings.json";

            var settings = JsonConvert.DeserializeObject<NugetSourceSettings>(File.ReadAllText(nugetConfig));
            nugetService = new NugetService(settings);
            await nugetService.InitRepositories();

            var requiredDeps = await GetDependencies(package, new NuGetVersion(version), maxDepth);

            var structurizer = new ProjectStructurizer();
            var projectPaths = structurizer.GetAllProjectStatuses(solutionFile).Select(x => x.ActualPath);

            foreach (var item in projectPaths)
                UpdateProject(item, requiredDeps);


            //I know which one changed

            Console.WriteLine(totalCount.Distinct().Count());
            Console.WriteLine();
        }

        private static void UpdateProject(string path, IEnumerable<NugetDependency> dependencies)
        {
            var proj = XDocument.Load(path);
            var nugetRefs = proj.Descendants().Where(x => x.Name.LocalName == "PackageReference").ToList();
            totalCount.AddRange(nugetRefs.Select(x => x.Attribute("Include")?.Value));
            var changed = false;

            foreach (var nugetRef in nugetRefs)
            {
                var package = nugetRef.Attribute("Include").Value;
                var version = nugetRef.Attribute("Version").Value;
                var nugetVersion = NuGetVersion.Parse(version);

                var required = dependencies.FirstOrDefault(x => x.Package == package);

                if (required != null && required.Version != nugetVersion)
                {
                    if (changed == false)
                    {
                        Console.WriteLine(Path.GetFileNameWithoutExtension(path));
                    }
                    nugetRef.Attribute("Version").SetValue(required.Version);
                    Console.WriteLine($"\t{package}:{version} -> {required.Version}");
                    changed = true;
                }
            }

            if (changed)
            {
                XmlWriterSettings xws = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true
                };
                using (XmlWriter xw = XmlWriter.Create(path, xws))
                    proj.Save(xw);
            }
        }

        private static async Task<IEnumerable<NugetDependency>> GetDependencies(string package, NuGetVersion version, int maxDepth, int depth = 0)
        {
            var rootPackage = new List<NugetDependency>
            {
                new NugetDependency
                {
                    Package = package,
                    Version = version
                }
            };

            Console.WriteLine($"{new string('\t', depth)}{package}:{version}");

            var subPackages = await nugetService.GetNugetDeps(package, version);

            if (depth < maxDepth)
            {
                foreach (var item in subPackages)
                {
                    var newDeps = await GetDependencies(item.Package, item.Version, maxDepth, depth + 1);
                    rootPackage = MergeKeepingLatest(rootPackage, newDeps.ToList());
                }
            }
            else
            {
                foreach (var item in subPackages)
                    Console.WriteLine($"{new string('\t', depth + 1) }{item.Package}:{item.Version}");

                rootPackage = MergeKeepingLatest(rootPackage, subPackages.ToList());
            }
            return rootPackage;
        }

        private static List<NugetDependency> MergeKeepingLatest(List<NugetDependency> currentList, List<NugetDependency> append)
        {
            var copy = currentList.ToList();
            foreach (var item2 in append)
            {
                var containing = copy.FirstOrDefault(x => x.Package == item2.Package);
                if (containing != null)
                {
                    if (containing.Version < item2.Version)
                    {
                        currentList.Remove(containing);
                        copy.Add(item2);
                    }
                }
                else
                {
                    copy.Add(item2);
                }
            }
            return copy;
        }
    }
}

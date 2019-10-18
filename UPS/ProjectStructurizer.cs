using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UPS
{
    public class ProjectStructurizer
    {
        public IEnumerable<ProjectStatus> GetProjectStatuses(string slnFilePath)
        {
            var fileContent = System.IO.File.ReadAllText(slnFilePath);
            var projectRegex = @"9A19103F-16F7-4668-BE54-9A1E7A4F7556.*?, ""(?<projectName>.*?)"", ""{(?<projectGuid>.*?)}""";
            var regex = new Regex(projectRegex);

            var mappings = NestingMappings(fileContent);
            var directories = DirectoryMappings(fileContent);

            var matches = regex.Matches(fileContent)
                .Select(s =>
                {
                    var name = s.Groups["projectName"].Value;
                    var guid = s.Groups["projectGuid"].Value;
                    return new ProjectStatus
                    {
                        Guid = guid,
                        ProjectName = GetFileName(name),
                        OriginalProjectName = name,
                        ActualPath = PathBasedOnSln(slnFilePath, name),
                        ExpectedPath = GetExpectedPath(slnFilePath, GetProjectnameAndHisFolder(name), guid, mappings, directories)
                    };
                });

            return matches;
        }

        private string GetProjectnameAndHisFolder(string name)
        {
            var parts = name.Split('\\');
            return System.IO.Path.Combine(parts.Skip(parts.Length - 2).ToArray());
        }

        private string GetExpectedPath(string slnFilePath, string name, string guid, Dictionary<string, string> mappings, Dictionary<string, string> directories)
        {
            var slnBaseDir = System.IO.Path.GetDirectoryName(slnFilePath);

            return System.IO.Path.Combine(slnBaseDir, GetMappingPath(guid, mappings, directories), name);
        }

        private string GetMappingPath(string guid, Dictionary<string, string> mappings, Dictionary<string, string> directories)
        {
            if (mappings.ContainsKey(guid))
            {
                var dirKey = mappings[guid];
                return System.IO.Path.Combine(GetMappingPath(dirKey, mappings, directories), directories[dirKey]);
            }

            return string.Empty;
        }

        private string GetFileName(string path)
        {
            var parts = path.Split('\\');
            return parts.Last();
        }

        private string PathBasedOnSln(string slnPath, string project)
        {
            var slnBaseDir = System.IO.Path.GetDirectoryName(slnPath);
            var expectedPath = System.IO.Path.Combine(slnBaseDir, project);
            if (System.IO.File.Exists(expectedPath))
            {
                return expectedPath;
            }
            else
            {
                return null;
            }
        }

        private Dictionary<string, string> DirectoryMappings(string slnContent)
        {
            var directoryRegex = @"2150E333-8FDC-42A3-9474-1A3956D46DE8.*?, ""(?<projectName>.*?)"", ""{(?<projectGuid>.*?)}""";
            var regex = new Regex(directoryRegex);
            var matches = regex.Matches(slnContent);

            var dict = new Dictionary<string, string>();
            foreach (Match item in matches)
            {
                dict[item.Groups["projectGuid"].Value] = item.Groups["projectName"].Value;
            }

            return dict;
        }

        private Dictionary<string, string> NestingMappings(string slnContent)
        {
            var mappingRegex = @"{(?<project>.*?)} = {(?<folder>.*?)}";
            var regex = new Regex(mappingRegex);
            var matches = regex.Matches(slnContent);

            var dict = new Dictionary<string, string>();
            foreach (Match item in matches)
            {
                dict[item.Groups["project"].Value] = item.Groups["folder"].Value;
            }

            return dict;
        }
    }
}

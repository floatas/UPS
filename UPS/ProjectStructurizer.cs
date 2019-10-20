using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UPS
{
    public class ProjectStructurizer
    {
        public const string ProjectFolderGuid = "66A26720-8FB5-11D2-AA7E-00C04F688DDE";
        public const string SolutionFolderGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
        public readonly IEnumerable<string> FolderGuids = new List<string> { ProjectFolderGuid, SolutionFolderGuid };

        public IEnumerable<ProjectStatus> GetAllProjectStatuses(string slnFilePath)
        {
            var slnContent = File.ReadAllText(slnFilePath);
            var slnFileDir = Path.GetDirectoryName(slnFilePath);
            var projectRegex = @"(""{(?<projectType>.*?)}"").*?, ""(?<projectName>.*?)"", ""{(?<projectGuid>.*?)}""";
            var regex = new Regex(projectRegex);

            var nestingMappings = GetDirectoryNestingMappings(slnContent);

            var matches = regex.Matches(slnContent);
            var directories = matches
                .Where(m => FolderGuids.Contains(m.Groups["projectType"].Value))
                .ToDictionary(key => key.Groups["projectGuid"].Value, value => value.Groups["projectName"].Value);

            var statuses = matches
                .Where(m => !FolderGuids.Contains(m.Groups["projectType"].Value))
                .Select(s =>
                {
                    var name = s.Groups["projectName"].Value;
                    var guid = s.Groups["projectGuid"].Value;
                    return new ProjectStatus
                    {
                        Guid = guid,
                        ProjectName = Path.GetFileName(name),
                        OriginalProjectName = name,
                        ActualPath = Path.Combine(slnFileDir, name),
                        ExpectedPath = Path.Combine(slnFileDir, GetSolutionFolderPath(guid, nestingMappings, directories), GetProjectnameAndHisFolder(name))
                    };
                });

            return statuses;
        }

        public void ProcessFile(string slnFilePath, ProjectStatus status, IEnumerable<ProjectStatus> statuses)
        {
            var slnFileDir = Path.GetDirectoryName(slnFilePath);
            var projectDir = Path.GetDirectoryName(status.ExpectedPath);
            var newProjectDir = Path.GetRelativePath(slnFileDir, projectDir);

            var newPath = Path.Combine(newProjectDir, status.ProjectName);
            var oldPath = status.OriginalProjectName;

            ReplaceInFile(slnFilePath, oldPath, newPath);
            UpdateReferencePaths(status.ActualPath, status.ExpectedPath, statuses);
            UpdateHintPaths(status.ActualPath, status.ExpectedPath);

            if (status.ActualPath != status.ExpectedPath)
            {
                DirectoryCopy(Path.GetDirectoryName(status.ActualPath), Path.GetDirectoryName(status.ExpectedPath));
            }
        }

        private void UpdateReferencePaths(string actualPath, string expectedPath, IEnumerable<ProjectStatus> allStatuses)
        {
            var oldProjectDir = Path.GetDirectoryName(actualPath);
            var projectDir = Path.GetDirectoryName(expectedPath);

            var projectContent = File.ReadAllText(actualPath);
            var projectReferences = @"ProjectReference Include=""(?<reference>.*?)""";
            var regex = new Regex(projectReferences);
            foreach (Match match in regex.Matches(projectContent))
            {
                var reference = match.Groups["reference"].Value;
                var referencePath = Path.Combine(oldProjectDir, reference);
                var normalized = Path.GetFullPath(referencePath);
                var existing = allStatuses.First(x => normalized == x.ActualPath);
                var existingRelativeToNewPath = Path.GetRelativePath(projectDir, existing.ExpectedPath);

                ReplaceInFile(actualPath, reference, existingRelativeToNewPath);
            }
        }

        private void UpdateHintPaths(string projectFilePath, string expectedPath)
        {
            var oldProjectDir = Path.GetDirectoryName(projectFilePath);
            var projectContent = File.ReadAllText(projectFilePath);
            var hintPaths = @"<HintPath>(?<reference>.*?)</HintPath>";
            var regex = new Regex(hintPaths);
            foreach (Match match in regex.Matches(projectContent))
            {
                var reference = match.Groups["reference"].Value;
                var referencePath = Path.Combine(oldProjectDir, reference);
                var normalized = Path.GetFullPath(referencePath);
                var existingRelativeToNewPath = Path.GetRelativePath(Path.GetDirectoryName(expectedPath), normalized);

                ReplaceInFile(projectFilePath, reference, existingRelativeToNewPath);
            }
        }

        //Thank you SO, forgot link...
        private static void DirectoryCopy(string sourceDirName, string destDirName,
                                  bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void ReplaceInFile(string file, string from, string to)
        {
            var allText = File.ReadAllText(file);
            var newText = allText.Replace(from, to);
            File.WriteAllText(file, newText);
        }

        private string GetProjectnameAndHisFolder(string name)
        {
            var parts = name.Split('\\');
            return Path.Combine(parts.Skip(parts.Length - 2).ToArray());
        }

        private string GetSolutionFolderPath(string guid, Dictionary<string, string> mappings, Dictionary<string, string> directories)
        {
            if (mappings.ContainsKey(guid))
            {
                var dirKey = mappings[guid];
                return Path.Combine(GetSolutionFolderPath(dirKey, mappings, directories), directories[dirKey]);
            }

            return string.Empty;
        }

        private Dictionary<string, string> GetDirectoryNestingMappings(string slnContent)
        {
            var mappingRegex = @"{(?<dirA>.*?)} = {(?<dirB>.*?)}";
            var regex = new Regex(mappingRegex);

            var nesting = regex.Matches(slnContent)
                .ToDictionary(key => key.Groups["dirA"].Value,
                value => value.Groups["dirB"].Value);

            return nesting;
        }
    }
}

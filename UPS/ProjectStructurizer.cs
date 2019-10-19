using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace UPS
{
    public class ProjectStructurizer
    {
        public IEnumerable<ProjectStatus> GetProjectStatuses(string slnFilePath)
        {
            var fileContent = File.ReadAllText(slnFilePath);
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

        public void ProcessFile(string slnFilePath, ProjectStatus status, IEnumerable<ProjectStatus> statuses)
        {
            var slnBaseDir = Path.GetDirectoryName(slnFilePath);
            var projectDir = Path.GetDirectoryName(status.ExpectedPath);
            var oldProjectDir = Path.GetDirectoryName(status.ActualPath);
            var newFolder = Path.GetRelativePath(slnBaseDir, projectDir);

            var newPath = Path.Combine(newFolder, status.ProjectName);
            var oldPath = status.OriginalProjectName;

            //replace in sln file
            ReplaceInFile(slnFilePath, oldPath, newPath);

            var projectContent = File.ReadAllText(status.ActualPath);
            var projectReferences = @"ProjectReference Include=""(?<reference>.*?)""";
            var regex = new Regex(projectReferences);
            var matches = regex.Matches(projectContent);
            if (matches.Any())
            {
                foreach (Match match in matches)
                {
                    var reference = match.Groups["reference"].Value;
                    var referencePath = Path.Combine(oldProjectDir, reference);
                    var normalized = Path.GetFullPath(referencePath);
                    var existing = statuses.First(x => normalized == x.ActualPath);
                    var existingRelativeToNewPath = Path.GetRelativePath(projectDir, existing.ExpectedPath);

                    ReplaceInFile(status.ActualPath, reference, existingRelativeToNewPath);
                }
            }

            UpdateHintPaths(status.ActualPath, status.ExpectedPath);
            if (status.ActualPath != status.ExpectedPath)
            {
                DirectoryCopy(Path.GetDirectoryName(status.ActualPath), Path.GetDirectoryName(status.ExpectedPath));
            }
        }

        private void UpdateHintPaths(string projectFilePath, string expectedPath)
        {
            var oldProjectDir = Path.GetDirectoryName(projectFilePath);
            var projectContent = File.ReadAllText(projectFilePath);
            var hintPaths = @"<HintPath>(?<reference>.*?)</HintPath>";
            var regex = new Regex(hintPaths);
            var matches = regex.Matches(projectContent);
            if (matches.Any())
            {
                foreach (Match match in matches)
                {
                    var reference = match.Groups["reference"].Value;
                    var referencePath = Path.Combine(oldProjectDir, reference);
                    var normalized = Path.GetFullPath(referencePath);
                    var existingRelativeToNewPath = Path.GetRelativePath(Path.GetDirectoryName(expectedPath), normalized);

                    ReplaceInFile(projectFilePath, reference, existingRelativeToNewPath);
                }
            }
        }

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

        private string GetExpectedPath(string slnFilePath, string name, string guid, Dictionary<string, string> mappings, Dictionary<string, string> directories)
        {
            var slnBaseDir = Path.GetDirectoryName(slnFilePath);

            return Path.Combine(slnBaseDir, GetMappingPath(guid, mappings, directories), name);
        }

        private string GetMappingPath(string guid, Dictionary<string, string> mappings, Dictionary<string, string> directories)
        {
            if (mappings.ContainsKey(guid))
            {
                var dirKey = mappings[guid];
                return Path.Combine(GetMappingPath(dirKey, mappings, directories), directories[dirKey]);
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
            var slnBaseDir = Path.GetDirectoryName(slnPath);
            var expectedPath = Path.Combine(slnBaseDir, project);
            if (File.Exists(expectedPath))
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

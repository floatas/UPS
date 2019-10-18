using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UPS
{
    public class ProjectStructurizer
    {
        public IEnumerable<ProjectStatus> GetProjectStatuses(string slnFile)
        {
            var fileContent = System.IO.File.ReadAllText(slnFile);
            var projectRegex = @"FAE04EC0-301F-11D3-BF4B-00C04F79EFBC.*?, ""(?<projectName>.*?)""";
            var regex = new Regex(projectRegex);
            var matches = regex.Matches(fileContent)
                .Select(m => m.Groups["projectName"].Value)
                .Select(s => new ProjectStatus { ProjectName = GetFileName(s) });

            return matches;
        }

        private string GetFileName(string path)
        {
            var parts = path.Split('\\');
            return parts.Last();
        }
    }
}

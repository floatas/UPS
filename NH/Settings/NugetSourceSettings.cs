using System.Collections.Generic;

namespace NH.Settings
{
    public class NugetSourceSettings
    {
        public IEnumerable<NugetSource> NugetRepositories { get; set; }
    }
}

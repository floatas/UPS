using NuGet.Versioning;

namespace NH
{
    public class NugetDependency
    {
        public string Package { get; set; }
        public NuGetVersion Version { get; set; }
    }
}
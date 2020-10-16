using NH.Settings;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NH
{
    public class NugetService
    {
        private NugetSourceSettings _settings;
        private List<FindPackageByIdResource> _nugetSources;
        private SourceCacheContext _cache;

        public NugetService(NugetSourceSettings settings)
        {
            _settings = settings;
            _nugetSources = new List<FindPackageByIdResource>();
        }

        public async Task InitRepositories()
        {
            foreach (var setting in _settings.NugetRepositories.OrderBy(x => x.Order))
            {
                var source = new PackageSource(setting.Source, setting.Name, true, false);
                if (setting.Credentials != null)
                    source.Credentials = new PackageSourceCredential(setting.Source, setting.Credentials.Username, setting.Credentials.Password, setting.Credentials.IsPasswordClearText, string.Empty);
                var repo = Repository.Factory.GetCoreV3(source);
                _nugetSources.Add(await repo.GetResourceAsync<FindPackageByIdResource>());
            }
        }

        public async Task<IEnumerable<NugetDependency>> GetNugetDeps(string packageName, NuGetVersion version)
        {
            var logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            FindPackageByIdDependencyInfo info = null;
            foreach (var source in _nugetSources)
            {
                info = await source.GetDependencyInfoAsync(packageName, version, _cache, logger, cancellationToken);
                if (info != null)
                    break;
            }

            if (info == null)
                return new List<NugetDependency>();

            var subPackages = info.DependencyGroups.SelectMany(x => x.Packages)
                .Select(p => new NugetDependency
                {
                    Package = p.Id,
                    Version = p.VersionRange.MinVersion,
                });

            var p = new NugetDependency
            {
                Package = info.PackageIdentity.Id,
                Version = info.PackageIdentity.Version,
            };

            var all = new List<NugetDependency>();
            all.AddRange(subPackages);
            all.Add(p);

            return all;

        }
    }
}

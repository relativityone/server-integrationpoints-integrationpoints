using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Provider;
using LanguageExt;
using LanguageExt.DataTypes.Serialisation;
using Relativity.IntegrationPoints.Contracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeRipProviderInstaller : IRipProviderInstaller
    {
        public Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<SourceProvider> providersToInstall)
        {
            Either<string, Unit> result = new Either<string, Unit>(new List<EitherData<string, Unit>>
            {
                new EitherData<string, Unit>(EitherStatus.IsLeft, Unit.Default, "Adler Sieben")
            });

            return Task.FromResult(result);
        }
    }
}

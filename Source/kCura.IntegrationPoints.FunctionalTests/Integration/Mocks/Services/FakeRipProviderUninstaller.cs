using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Provider;
using LanguageExt;
using LanguageExt.DataTypes.Serialisation;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeRipProviderUninstaller : IRipProviderUninstaller
    {
        public Task<Either<string, Unit>> UninstallProvidersAsync(int applicationID)
        {
            Either<string, Unit> result = new Either<string, Unit>(new List<EitherData<string, Unit>>
            {
                new EitherData<string, Unit>(EitherStatus.IsLeft, Unit.Default, "Adler Sieben")
            });

            return Task.FromResult(result);
        }
    }
}

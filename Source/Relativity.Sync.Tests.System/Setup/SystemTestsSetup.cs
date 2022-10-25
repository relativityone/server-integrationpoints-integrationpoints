using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;

// ReSharper disable once CheckNamespace
// No namespace applies this to the whole assembly
public class SystemTestsSetup : InstanceTestsSetup
{
    public async override Task RunBeforeAnyTests()
    {
        await base.RunBeforeAnyTests().ConfigureAwait(false);

        InstallRelativityImport();
    }

    private void InstallRelativityImport()
    {
        ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

        applicationService.InstallToLibrary(AppSettings.RelativityImportRAPPath);
    }
}

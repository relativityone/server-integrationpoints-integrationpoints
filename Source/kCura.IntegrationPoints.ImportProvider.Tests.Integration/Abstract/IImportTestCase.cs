using System.Threading.Tasks;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Abstract
{
    public interface IImportTestCase
    {
        SettingsObjects Prepare(int workspaceId);

        void Verify(int workspaceId);
    }
}

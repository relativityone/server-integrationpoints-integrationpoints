using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
    public abstract class TestCaseBase : IImportTestCase
    {
        protected abstract string[] DocumentFields { get; set; }

        public abstract void Verify(int workspaceId);

        public abstract SettingsObjects Prepare(int workspaceId);

        protected virtual SettingsObjects Prepare(int workspaceId, string resourceName, string loadFileName)
        {
            SettingsObjects objects = new SettingsObjects();
            SetEmbeddedResource(objects, resourceName);
            SetLoadFile(objects, loadFileName);
            SetWorkspaceId(objects, workspaceId);
            return objects;
        }

        private void SetEmbeddedResource(SettingsObjects objects, string resourceName)
        {
            objects.FieldMaps = EmbeddedResource.FieldMaps(resourceName);
            objects.ImportProviderSettings = EmbeddedResource.ImportProviderSettings(resourceName);
            objects.ImportSettings = EmbeddedResource.ImportSettings(resourceName);
        }

        private void SetLoadFile(SettingsObjects objects, string loadFileName)
        {
            objects.ImportProviderSettings.LoadFile = loadFileName;
        }

        private void SetWorkspaceId(SettingsObjects objects, int workspaceId)
        {
            objects.ImportSettings.CaseArtifactId = workspaceId;
            objects.ImportProviderSettings.WorkspaceId = workspaceId;
        }
    }
}

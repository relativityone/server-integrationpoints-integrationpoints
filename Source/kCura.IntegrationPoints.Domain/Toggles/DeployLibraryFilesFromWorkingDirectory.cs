using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Toggles
{
    [DefaultValue(true)]
    [Description("Deploys library files from working directory (when true) or by using RelativityFeaturePathService (old method)", "Adler Sieben")]
    internal class DeployLibraryFilesFromWorkingDirectory : IToggle
    {
    }
}

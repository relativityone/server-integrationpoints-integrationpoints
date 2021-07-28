using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Toggles
{
    [DefaultValue(false)]
    [Description("Loads 3rd party assemblies as it is required by Kubernetes (when true)", "Adler Sieben")]
    internal class LoadRequiredAssembliesInKubernetesMode : IToggle
    {
    }
}

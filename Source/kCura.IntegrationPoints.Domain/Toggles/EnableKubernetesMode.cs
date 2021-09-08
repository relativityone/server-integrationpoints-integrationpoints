using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Toggles
{
    [DefaultValue(false)]
    [Description("When true, makes the Agent compatible with Kubernetes.", "Adler Sieben")]
    internal class EnableKubernetesMode : IToggle
    {
    }
}

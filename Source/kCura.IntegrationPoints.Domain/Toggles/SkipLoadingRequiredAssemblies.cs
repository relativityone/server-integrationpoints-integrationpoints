using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Toggles
{
    [DefaultValue(true)]
    [Description("Skips required assemblies loading (when true)", "Adler Sieben")]
    internal class SkipLoadingRequiredAssemblies : IToggle
    {
    }
}

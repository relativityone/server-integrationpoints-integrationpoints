namespace kCura.IntegrationPoints.Domain.EnvironmentalVariables
{
    public interface IKubernetesMode
    {
        bool IsEnabled { get; }
    }
}

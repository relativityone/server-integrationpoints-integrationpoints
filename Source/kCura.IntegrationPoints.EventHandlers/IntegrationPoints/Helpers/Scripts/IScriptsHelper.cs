namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public interface IScriptsHelper
    {
        int GetArtifactIdByGuid(string guid);
        string GetApplicationPath();
        int GetApplicationId();
        int GetActiveArtifactId();
        string GetDestinationConfiguration();
        string GetSourceConfiguration();
        string GetSourceViewUrl();
        string GetAPIControllerName();
    }
}

using kCura.EventHandler;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public interface ICommonScriptsFactory
    {
        ICommonScripts Create(EventHandlerBase eventHandlerBase);
    }
}
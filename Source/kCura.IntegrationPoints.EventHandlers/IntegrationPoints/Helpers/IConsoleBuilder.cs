using kCura.EventHandler;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IConsoleBuilder
    {
        Console CreateConsole(ButtonStateDTO buttonState, OnClickEventDTO onClickEvents);
    }
}
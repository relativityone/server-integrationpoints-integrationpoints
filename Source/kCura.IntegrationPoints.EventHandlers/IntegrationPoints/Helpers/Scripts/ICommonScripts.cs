using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public interface ICommonScripts
    {
        IList<string> LinkedCss();
        IList<string> LinkedScripts();
        IList<string> ScriptBlocks();
    }
}

using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public class ImportProvidersScripts : CommonScripts
    {
        public ImportProvidersScripts(ScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants) : base(scriptsHelper, guidsConstants)
        {
        }

        public override IList<string> LinkedScripts()
        {
            var result = base.LinkedScripts();
            result.Add("/Scripts/moment.js");
            result.Add("/Scripts/EventHandlers/integration-points-view.js");
            return result;
        }
    }
}

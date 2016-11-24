using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
	public class DefaultProviderScripts : CommonScripts
	{
		public DefaultProviderScripts(ScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants) : base(scriptsHelper, guidsConstants)
		{
		}

		public override IList<string> LinkedScripts()
		{
			var result = base.LinkedScripts();
			result.Add("/Scripts/EventHandlers/integration-points-view.js");
			return result;
		}
	}
}
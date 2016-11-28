using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
	public class LoadFileProviderScripts : RelativityProviderScripts
	{
		public LoadFileProviderScripts(ScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants, IWorkspaceNameValidator workspaceNameValidator,
			IFolderPathInformation folderPathInformation)
			: base(scriptsHelper, guidsConstants, workspaceNameValidator, folderPathInformation)
		{
		}


		public override IList<string> LinkedScripts()
		{
			var result = base.LinkedScripts();
			result.Add("/Scripts/EventHandlers/export-details-view.js");
			return result;
		}
	}
}
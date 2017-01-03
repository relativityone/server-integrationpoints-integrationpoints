using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
	public class RelativityProviderScripts : CommonScripts
	{
		private readonly IFolderPathInformation _folderPathInformation;
		private readonly IWorkspaceNameValidator _workspaceNameValidator;

		public RelativityProviderScripts(ScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants, IWorkspaceNameValidator workspaceNameValidator,
			IFolderPathInformation folderPathInformation)
			: base(scriptsHelper, guidsConstants)
		{
			_workspaceNameValidator = workspaceNameValidator;
			_folderPathInformation = folderPathInformation;
		}

		public override IList<string> LinkedScripts()
		{
			var result = base.LinkedScripts();
			result.Add("/Scripts/moment.js");
			result.Add("/Scripts/EventHandlers/relativity-provider-view.js");
			return result;
		}

		public override IList<string> ScriptBlocks()
		{
			var result = base.ScriptBlocks();

			string errorMessage = _workspaceNameValidator.Validate(ScriptsHelper.GetSourceConfiguration());
			string folderPathInformation = _folderPathInformation.RetrieveName(ScriptsHelper.GetDestinationConfiguration());

			string block = "<script>" +
							$"IP.errorMessage = '{errorMessage}';" +
							$"IP.fieldName = '{folderPathInformation}';" +
							"</script>";

			result.Add(block);

			return result;
		}
	}
}
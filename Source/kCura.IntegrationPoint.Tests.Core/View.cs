using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class View
	{
		public static int QueryView(int workspaceId, string viewName)
		{
			using (var rsApiClient = Rsapi.CreateRsapiClient())
			{
				rsApiClient.APIOptions.WorkspaceID = workspaceId;
				var viewQuery = new Query<Relativity.Client.DTOs.View>
				{
					Condition = new TextCondition(ViewFieldNames.Name, TextConditionEnum.EqualTo, viewName)
				};
				return rsApiClient.Repositories.View.Query(viewQuery).Results[0].Artifact.ArtifactID;
			}
		}
	}
}
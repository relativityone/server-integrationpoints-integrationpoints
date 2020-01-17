using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Orchestrators;
using Relativity.Testing.Framework.Api;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.JsonLoader.FunctionalTests
{
	[SetUpFixture]
	public class SetUpFixture
	{
		public static Testing.Framework.Models.Workspace Workspace;

		public static IRelativityFacade Relativity => RelativityFacade.Instance;
		public static ApiComponent ApiComponent => Relativity.GetComponent<ApiComponent>();

		private static IOrchestrateWorkspaces _workspaceOrchestrator;

		[OneTimeSetUp]
		public async Task OneTimeSetupAsync()
		{
			Relativity.RelyOn<ApiComponent>();
			_workspaceOrchestrator = ApiComponent
				.OrchestratorFactory
				.Create<IOrchestrateWorkspaces>();
			QueryResult results;
			string templateName = ConfigurationManager.AppSettings["WorkspaceTemplateName"];

			using (IObjectManager manager = ApiComponent.ServiceFactory.GetAdminServiceProxy<IObjectManager>())
			{
				QueryRequest workspaceTemplateRequest = new QueryRequest
				{
					Condition = $"'Name' == '{templateName}'",
					Fields = new List<FieldRef> { new FieldRef { Name = "ArtifactID" } },
					ObjectType = new ObjectTypeRef { Name = "Workspace" }
				};
				results = await manager.QueryAsync(-1, workspaceTemplateRequest, 1, 1).ConfigureAwait(false);
			}
			if (results.Objects.Count > 0)
			{
				Testing.Framework.Models.Workspace template = new Testing.Framework.Models.Workspace
				{
					Name = templateName,
					ArtifactID = results.Objects[0].ArtifactID
				};
				Workspace = _workspaceOrchestrator.GetBasicWorkspace(
					new Testing.Framework.Models.Workspace
					{
						TemplateWorkspace = template
					}, 
					ensureNew: true);
			}
			else
			{
				Workspace = ApiComponent.OrchestratorFactory.Create<IOrchestrateWorkspaces>().GetBasicWorkspace(true);
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_workspaceOrchestrator.DeleteExistingWorkspace(Workspace);
			_workspaceOrchestrator.Dispose();
		}
	}
}

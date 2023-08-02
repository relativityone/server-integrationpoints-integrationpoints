using System.Configuration;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider.FunctionalTests
{
	[SetUpFixture]
	public class SetUpFixture
	{
		public static Workspace Workspace;

		public static IRelativityFacade Relativity => RelativityFacade.Instance;

		[OneTimeSetUp]
		public void OneTimeSetupAsync()
		{
			Relativity.RelyOn<CoreComponent>();
			Relativity.RelyOn<ApiComponent>();

			string templateName = ConfigurationManager.AppSettings["WorkspaceTemplateName"];

			IWorkspaceService service = Relativity.Resolve<IWorkspaceService>();

			Workspace response = service.Get(templateName);

			if (response != null)
			{
				Workspace template = new Workspace
				{
					Name = templateName,
					ArtifactID = response.ArtifactID
				};

				Workspace = service.Create(
					new Workspace
					{
						TemplateWorkspace = template
					});
			}
			else
			{
				Workspace = service.Create(
					new Workspace
					{
						Name = Randomizer.GetString()
					});
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if(Workspace != null)
			{
				Relativity.Resolve<IWorkspaceService>().Delete(Workspace.ArtifactID);
			}
		}
	}
}
using System.Configuration;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.JsonLoader.FunctionalTests
{
	[SetUpFixture]
	public class SetUpFixture
	{
		public static Workspace Workspace;

		public static IRelativityFacade Relativity => RelativityFacade.Instance;

		[OneTimeSetUp]
		public void OneTimeSetup()
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
			try
			{
				Relativity.Resolve<IWorkspaceService>().Delete(Workspace.ArtifactID);
			}
			catch { }
		}
	}
}

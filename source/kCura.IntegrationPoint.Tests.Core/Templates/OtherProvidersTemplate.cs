using System.Linq;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	public class OtherProvidersTemplate : SourceProviderTemplate
	{
		protected SourceProvider LdapProvider;

		public OtherProvidersTemplate(string workspaceName, string workspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE) 
			: base(workspaceName, workspaceTemplate)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();

			LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
		}
	}
}

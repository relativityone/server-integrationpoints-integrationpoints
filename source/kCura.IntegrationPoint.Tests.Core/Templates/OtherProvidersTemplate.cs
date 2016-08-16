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

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
		}
	}
}
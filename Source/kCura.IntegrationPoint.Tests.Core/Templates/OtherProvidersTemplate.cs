using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
    public abstract class OtherProvidersTemplate : SourceProviderTemplate
    {
        protected SourceProvider LdapProvider;

        protected OtherProvidersTemplate(
            string workspaceName,
            string workspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
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

using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;


namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ImportFromLDAPMapFieldsPage;

    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 4, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ImportFromLDAPMapFieldsPage : WorkspacePage<_>
    {
        public Button<IntegrationPointViewPage, _> Save { get; private set; }

        [FindById("source-fields")]
        public Select<_> Source { get; private set; }

        [FindByTitle("cn")]
        public Option<string, _> Cn { get; private set; }

        [FindByTitle("givenname")]
        public Option<string, _> GivenName { get; private set; }

        [FindByTitle("sn")]
        public Option<string, _> Sn { get; private set; }

        [FindById("workspace-fields")]
        public Select<_> Destination { get; private set; }

        [FindByTitle("UniqueID [Object Identifier]")]
        public Option<string, _> UniqueID { get; private set; }

        [FindByTitle("First Name [Fixed-Length Text]")]
        public Option<string, _> FirstName { get; private set; }

        [FindByTitle("Last Name [Fixed-Length Text]")]
        public Option<string, _> LastName { get; private set; }
    }
}

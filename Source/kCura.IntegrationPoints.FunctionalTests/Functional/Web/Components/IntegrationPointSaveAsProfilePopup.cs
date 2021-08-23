using Atata;
using Relativity.Testing.Framework.Web.ControlSearch;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointSaveAsProfilePopup;

	[FindByPrecedingCellContent(TargetType = typeof(Field<,>))]
	internal class IntegrationPointSaveAsProfilePopup : JQueryUIDialog<_>
	{
		[FindById("profile-name")]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
		public TextInput<_> ProfileName { get; private set; }

		public Button<IntegrationPointViewPage, _> SaveAsProfile { get; private set; }

		public Button<IntegrationPointViewPage, _> Cancel { get; private set; }
	}
}

using Atata;
using Relativity.Testing.Framework.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointSaveAsProfilePopup;

	[FindByPrecedingCellContent(TargetType = typeof(Field<,>))]
	internal class IntegrationPointSaveAsProfilePopup : JQueryUIDialog<_>
	{
		[FindById("profile-name")]
		[WaitFor(Until.Visible, TriggerEvents.BeforeAccess, AbsenceTimeout = 20)]
		public TextInput<_> ProfileName { get; private set; }

		public Button<IntegrationPointViewPage, _> SaveAsProfile { get; private set; }

		public Button<IntegrationPointViewPage, _> Cancel { get; private set; }
	}
}

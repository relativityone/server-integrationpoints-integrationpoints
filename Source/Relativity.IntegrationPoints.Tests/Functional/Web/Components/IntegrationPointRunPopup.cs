using Atata;
using Relativity.Testing.Framework.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointRunPopup;

	[FindByPrecedingCellContent(TargetType = typeof(Field<,>))]
	internal class IntegrationPointRunPopup : JQueryUIDialog<_>
	{
		public Button<IntegrationPointViewPage, _> OK { get; private set; }

		public Button<IntegrationPointViewPage, _> Cancel { get; private set; }
	}
}

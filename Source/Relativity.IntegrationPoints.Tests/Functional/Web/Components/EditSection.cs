using Atata;
using Relativity.Testing.Framework.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	[FindByPrecedingCellContent(TargetType = typeof(Field<,>))]
	[FindByPrecedingCellContentHavingColonSettings]
	[ControlDefinition(ComponentTypeName = "section", IgnoreNameEndings = "Section")]
	[FindByDataName]
	internal class EditSection<TOwner> : Control<TOwner>
		where TOwner : PageObject<TOwner>
	{
		[ControlDefinition("td[contains(@id, 'nameCell')]", ComponentTypeName = "label")]
		public ControlList<Text<TOwner>, TOwner> FieldLabels { get; private set; }
	}
}

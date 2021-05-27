using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	[PageObjectDefinition(ContainingClass = "ui-dialog", ComponentTypeName = "dialog", IgnoreNameEndings = "PopupWindow,Window,Popup,Modal,Dialog")]
	[WindowTitleElementDefinition(ContainingClass = TitleClassName)]
	public abstract class JQueryUIDialog<TOwner> : PopupWindow<TOwner>
		where TOwner : JQueryUIDialog<TOwner>
	{
		private const string TitleClassName = "ui-dialog-title";
		private const string ContentClassName = "ui-dialog-content";

		protected JQueryUIDialog(params string[] windowTitleValues)
			: base(windowTitleValues)
		{
		}

		[FindByClass(TitleClassName)]
		public Text<TOwner> DialogTitle { get; private set; }

		[FindByClass(ContentClassName)]
		public Text<TOwner> DialogContent { get; private set; }
	}
}

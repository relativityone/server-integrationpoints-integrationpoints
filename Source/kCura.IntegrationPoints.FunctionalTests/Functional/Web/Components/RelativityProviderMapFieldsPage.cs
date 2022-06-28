using Atata;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = RelativityProviderMapFieldsPage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 4, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
	[WaitForJQueryAjax(TriggerEvents.Init)]
	internal class RelativityProviderMapFieldsPage : WorkspacePage<_>
	{
		public Button<IntegrationPointViewPage, _> Save { get; private set; }

		public Button<_> MapAllFields { get; private set; }

		public Button<_> MapView { get; private set; }

		[FindByPrecedingDivContent]
		public Select2<RelativityProviderOverwrite, _> Overwrite { get; private set; }

		[FindById("s2id_overlay-field-behavior")]
		public Select2<RelativityProviderMultiSelectField, _> MultiSelectField { get; private set; }

		[FindByPrecedingDivContent]
		public RadioButtonList<YesNo, _> CopyImages { get; private set; }

		[FindByPrecedingDivContent]
		public RadioButtonList<RelativityProviderCopyNativeFiles, _> CopyNativeFiles { get; private set; }

		[FindByPrecedingDivContent]
		public RadioButtonList<YesNo, _> CopyFilesToRepository { get; private set; }

		[FindById("s2id_folderPathInformationSelect")]
		public Select2<RelativityProviderFolderPathInformation, _> PathInformation { get; private set; }

        [FindById("s2id_image-production-precedence")]
        public Select2<RelativityProviderImagePrecedence, _> ImagePrecedence { get; private set; }
	}
}

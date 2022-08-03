using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.Controls;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Interfaces;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ExportToLoadFileDestinationInformationPage;

    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 4, AbsenceTimeout = 30,
        AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ExportToLoadFileDestinationInformationPage : WorkspacePage<_>, IHasTreeItems<_>
    {
        public Button<IntegrationPointViewPage, _> Save { get; private set; }

        #region Export Detail

        [FindById("export-loadfile-checkbox")]
        public CheckBox<_> LoadFile { get; private set; }

        [FindById("export-images-checkbox")]
        public CheckBox<_> Images { get; private set; }

        [FindById("export-natives-checkbox")]
        public CheckBox<_> Natives { get; private set; }

        [FindById("export-text-fields-as-files-checkbox")]
        public CheckBox<_> TextFieldsAsFiles { get; private set; }

        [FindById("overwrite-file-checkbox")]
        public CheckBox<_> OverwriteFiles { get; private set; }

        [FindById("create-export-directory-checkbox")]
        public CheckBox<_> CreateExportFolder { get; private set; }

        [FindById("location-select")]
        public Control<_> DestinationFolder { get; private set; }

        [FindByXPath("ul[contains(@class,'jstree-container-ul')]", Visibility = Visibility.Visible)]
        public UnorderedList<TreeItemControl<_>, _> TreeItems { get; private set; }

        #endregion

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<ImageFileFormats, _> ImageFileFormat { get; private set; }

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<DataFileFormats, _> DataFileFormat { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<DataFileEncodings, _> DataFileEncoding { get; private set; }

        [FindByPrecedingDivContent]
        public RadioButtonList<FilePaths, _> FilePath { get; private set; }

        [FindById("filePathUserprefix_2")]
        public TextInput<_> UserPrefix { get; private set; }

        [FindById("include-native-files-path-checkbox")]
        public CheckBox<_> IncludeNativeFilesPath { get; private set; }

        [FindById("export-multiple-choice-fields-as-nested")]
        public CheckBox<_> ExportMultipleChoiceFieldsAsNested { get; private set; }

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<NameOutputFilesAfterOptions, _> NameOutputFilesAfter { get; private set; }

        [FindById("append-original-file-name-checkbox")]
        public CheckBox<_> AppendOriginalFileName { get; private set; }

        #region IMAGE

        [FindByPrecedingDivContent]
        public Select2<ImageFileTypes, _> FileType { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<ImagePrecedences, _> ImagePrecedence { get; private set; }

        [FindById("subdirectory-image-prefix-input")]
        public TextInput<_> SubdirectoryImagePrefix { get; private set; }

        #endregion

        #region NATIVE

        [FindById("subdirectory-native-prefix-input")]
        public TextInput<_> SubdirectoryNativePrefix { get; private set; }

        #endregion

        #region TEXT

        [FindById("subdirectory-text-prefix-input")]
        public TextInput<_> SubdirectoryTextPrefix { get; private set; }

        #endregion

        #region VOLUME

        [FindById("volume-prefix-input")]
        public TextInput<_> SubdirectoryVolumePrefix { get; private set; }

        #endregion

        public _ SetDestinationFolder(int workspaceId)
        {
            string dataTransferExportLocation = $".\\EDDS{workspaceId}\\DataTransfer\\Export";

            return DestinationFolder
                .Click()
                .SetTreeItem(dataTransferExportLocation);
        }

        public _ SetUserPrefix(string userPrefix)
        {
            return UserPrefix.Set(userPrefix);
        }
    }
}

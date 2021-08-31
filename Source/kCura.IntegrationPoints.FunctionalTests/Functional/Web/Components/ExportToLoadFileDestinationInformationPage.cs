using System.Threading;
using Atata;
using OpenQA.Selenium;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
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
    internal class ExportToLoadFileDestinationInformationPage : WorkspacePage<_>
    {
        public Button<IntegrationPointViewPage, _> Save { get; private set; }


        //Export Detail
        //Export Type
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
        public Control<_> SelectFolder { get; private set; }

        [FindByXPath("ul[contains(@class,'jstree-container-ul')]", Visibility = Visibility.Visible)]
        public UnorderedList<TreeItemControl<_>, _> TreeItems { get; private set; }

        public _ SetItem(params string[] itemNames)
        {
            var item = TreeItems[0].GetScope();
            string hierarchy = string.Empty;

            foreach (var itemName in itemNames)
            {
                string xpath = $"{hierarchy}//li[@role='treeitem']/a[.='{itemName}']";
                hierarchy = $"{xpath}/..";

                var textItem = item.FindElement(By.XPath(xpath));
                textItem.Click();
                Thread.Sleep(1000);
                item = Driver.FindElement(By.XPath(hierarchy));
            }

            return Owner;
        }

        [ControlDefinition("li[@role='treeitem']")]
        [WaitUntilOverlayMissing(TriggerEvents.BeforeClick, AppliesTo = TriggerScope.Children)]
        public class TreeItemControl<TPage> : Control<TPage>
            where TPage : PageObject<TPage>
        {
            [FindByClass("jstree-icon")]
            private Clickable<TPage> TreeIcon { get; set; }

            [FindByXPath("a")]
            public Text<TPage> Text { get; private set; }

            public UnorderedList<TreeItemControl<TPage>, TPage> Children { get; private set; }
        }

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<ImageFileFormats, _> ImageFileFormat { get; private set; }

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<DataFileFormats, _> DataFileFormat { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<DataFileEncodings, _> DataFileEncoding { get; private set; }

        public UnorderedList<RadioControl<_>, _> FilePath { get; private set; }

        public class RadioControl<TPage> : Control<TPage>
            where TPage : PageObject<TPage>
        {
            private RadioButton<TPage> radioButton { get; set; }
            public Label<TPage> label { get; private set; }
        }

        [FindById("include-native-files-path-checkbox")]
        public CheckBox<_> IncludeNativeFilesPath { get; private set; }

        [FindById("export-multiple-choice-fields-as-nested")]
        public CheckBox<_> ExportMultipleChoiceFieldsAsNested { get; private set; }

        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        [FindByPrecedingDivContent]
        public Select2<NameOutputFilesAfterOptions, _> NameOutputFilesAfter { get; private set; }

        [FindById("append-original-file-name-checkbox")]
        public CheckBox<_> AppendOriginalFileName { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<ImageFileTypes, _> FileType { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<ImagePrecedences, _> ImagePrecedence { get; private set; }

    }
}

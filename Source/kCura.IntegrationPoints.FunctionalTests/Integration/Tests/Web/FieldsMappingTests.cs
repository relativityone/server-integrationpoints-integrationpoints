using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Web
{
    public class FieldsMappingTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;

        private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("Control Number", "Control Number"),
            new Tuple<string, string>("Extracted Text", "Extracted Text"),
            new Tuple<string, string>("Title", "Title")
        };

        public override void SetUp()
        {
            base.SetUp();

            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspaceWithIntegrationPointsApp(destinationWorkspaceArtifactId);

            CreateFieldsWithSpecialCharactersAsync(SourceWorkspace);
            CreateFieldsWithSpecialCharactersAsync(_destinationWorkspace);
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public void FieldMapping_ShouldDisplayMappableFieldsCorrectOrderInSourceWorkspaceFieldList()
        {
            //Arrange
            RelativityProviderModel model = CreateRelativityProviderModel(nameof(FieldMapping_ShouldDisplayMappableFieldsCorrectOrderInSourceWorkspaceFieldList));
            model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
            model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

            //Act
            var fields = _destinationWorkspace.Fields;
            //PushToRelativityThirdPage fieldMappingPage =
            //    PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
            //List<string> fieldsFromSourceWorkspaceListBox = fieldMappingPage.GetFieldsFromSourceWorkspaceListBox();

            //await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
            //List<string> expectedSourceMappableFields =
            //    CreateFieldMapListBoxFormatFromObjectManagerFetchedList(SourceContext.WorkspaceMappableFields);

            ////Assert
            //CollectionAssert.AreEqual(fieldsFromSourceWorkspaceListBox, expectedSourceMappableFields);
        }

        private void CreateFieldsWithSpecialCharactersAsync(WorkspaceTest workspace)
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";
                var fixedLengthTextFieldRequest = new FieldTest
                {
                    ObjectTypeId = Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID,
                    Name = $"{generatedFieldName} Fixed-Length Text",
                    IsIdentifier = false
                };

                workspace.Fields.Add(fixedLengthTextFieldRequest);

                var longTextFieldRequest = new FieldTest
                {
                    ObjectTypeId = Const.LONG_TEXT_TYPE_ARTIFACT_ID,
                    Name = $"{generatedFieldName} Long Text",
                    IsIdentifier = false
                };

                workspace.Fields.Add(longTextFieldRequest);
                //fieldManager.CreateLongTextFieldAsync(workspaceId, longTextFieldRequest).ConfigureAwait(false);
                //fieldManager.CreateFixedLengthFieldAsync(workspaceId, fixedLengthTextFieldRequest).ConfigureAwait(false);
            }

            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareLongTextFieldsMapping();
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
        }

        private RelativityProviderModel CreateRelativityProviderModel(string name)
        {
            RelativityProviderModel model = new RelativityProviderModel(name)
            {
                Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
                RelativityInstance = "This Instance",
                DestinationWorkspace = $"{_destinationWorkspace.Name} - {_destinationWorkspace.ArtifactId}",
                CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No,
                FieldMapping = DefaultFieldsMapping
            };

            return model;
        }
    }
}

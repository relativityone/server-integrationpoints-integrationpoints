using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class FieldMappingsControllerTests : TestsBase
    {
        private WorkspaceFake _destinationWorkspace;
        private List<FieldFake> _fields;
        private const string _FIXED_LENGTH_TEXT_NAME = "Fixed-Length Text";
        private const string _LONG_TEXT_NAME = "Long Text";
        private const string _DESTINATION_PROVIDER_GUID = "D2E10795-3A47-47C2-A457-7E54C2A5DD90";
        private const int _SOURCE_ARTIFACT_TYPE_ID = 111;
        private const int _DESTINATION_ARTIFACT_TYPE_ID = 222;

        public override void SetUp()
        {
            base.SetUp();

            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspaceWithIntegrationPointsApp(ArtifactProvider.NextId());
            _fields = CreateFieldsWithSpecialCharactersAsync(Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID, _FIXED_LENGTH_TEXT_NAME).ToList();
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public async Task GetMappableFieldsFromSourceWorkspace_ShouldReturnAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");
            ((List<FieldFake>)SourceWorkspace.Fields).AddRange(_fields);

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromSourceWorkspace(SourceWorkspace.ArtifactId, _SOURCE_ARTIFACT_TYPE_ID);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Count.Should().Be(_fields.Count);
            fieldsDTOs.All(x => x.ClassificationLevel == ClassificationLevel.AutoMap);
            fieldsDTOs.Select(x => x.Name).ShouldBeEquivalentTo(_fields.Select(x=> x.Name));
        }

        [IdentifiedTest("390681D9-5E08-47C2-88B8-2F24D33C774F")]
        public async Task GetMappableFieldsFromSourceWorkspace_ShouldHasSuccessStatusCodeWhenReturnsEmptyList()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromSourceWorkspace(SourceWorkspace.ArtifactId, _SOURCE_ARTIFACT_TYPE_ID);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Should().BeEmpty();
        }

        [IdentifiedTest("BE420CE7-3AEA-41F9-BDC7-C381DBC3FCE4")]
        public async Task GetMappableFieldsFromDestinationWorkspace_ShouldReturnAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromDestinationWorkspace");
            ((List<FieldFake>)_destinationWorkspace.Fields).AddRange(_fields);

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromDestinationWorkspace(_destinationWorkspace.ArtifactId, _DESTINATION_ARTIFACT_TYPE_ID);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Count.Should().Be(_fields.Count);
            fieldsDTOs.All(x => x.ClassificationLevel == ClassificationLevel.AutoMap);
            fieldsDTOs.Select(x => x.Name).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("A40D169F-05E8-4A03-9B39-48E431F91579")]
        public async Task GetMappableFieldsFromDestinationWorkspace_ShouldHasSuccessStatusCodeWhenReturnsEmptyList()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromDestinationWorkspace");

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromSourceWorkspace(_destinationWorkspace.ArtifactId, _SOURCE_ARTIFACT_TYPE_ID);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Should().BeEmpty();
        }

        [IdentifiedTest("B32FC4F0-EE3C-43BA-B02A-509E8F496FFE")]
        public async Task AutoMapFields_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFields");

            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);

            // Act
            HttpResponseMessage result = await sut.AutoMapFields(request, SourceWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("0FAE85FC-227D-40DD-9028-0C434C8389FA")]
        public async Task AutoMapFields_ShouldMatchOnlyIdentifiersWhenRequired()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFields");
            _fields[0].IsIdentifier = true;

            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo , true);

            // Act
            HttpResponseMessage result = await sut.AutoMapFields(request, SourceWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(1);
            fieldsMaps.First().SourceField.DisplayName.ShouldBeEquivalentTo(_fields.First().Name);
            fieldsMaps.First().DestinationField.DisplayName.ShouldBeEquivalentTo(_fields.First().Name);
        }

        [IdentifiedTest("C9FD7D63-A6DB-480A-86C1-167250F382F2")]
        public async Task AutoMapFields_ShouldBeEmptyWhenNoIdentifiersSetWhenRequired()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFields");

            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo, true);

            // Act
            HttpResponseMessage result = await sut.AutoMapFields(request, SourceWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Should().BeEmpty();
        }

        [IdentifiedTest("80090507-41F8-4C35-9878-D263E54562C4")]
        public async Task AutoMapFieldsFromSavedSearch_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);
            SavedSearchFake savedSearch = CreateSavedSearchTest(_fields);

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId, savedSearch.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("A9E9A2B7-CA32-43DC-A655-3F3790EB4665")]
        public async Task AutoMapFieldsFromSavedSearch_ShouldHasSuccessStatusCodeWhenReturnsEmptyList()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);
            SavedSearchFake savedSearch = new SavedSearchFake();

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId, savedSearch.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Should().BeEmpty();
        }

        [IdentifiedTest("B86CAB2F-4EA6-4882-A94E-1057A2F83EF0")]
        public async Task AutoMapFieldsFromSavedSearch_ShouldReturnEmptyListWhenNoFieldsAreSetSavedSearch()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);
            SavedSearchFake savedSearch = CreateSavedSearchTest(new List<FieldFake>());

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId, savedSearch.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Should().BeEmpty();
        }

        [IdentifiedTest("60F22A32-A077-40FF-B459-9FD2B94C59F2")]
        public async Task AutoMapFieldsFromSavedSearch_ShouldReturnOnlyFieldsInSavedSearch()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);

            List<FieldFake> savedSearchFields = CreateFieldsWithSpecialCharactersAsync(Const.LONG_TEXT_TYPE_ARTIFACT_ID, _LONG_TEXT_NAME).ToList();
            savedSearchFields.AddRange(_fields);
            SavedSearchFake savedSearch = CreateSavedSearchTest(savedSearchFields);

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId, savedSearch.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("8D1BE3D3-0BC1-48B4-AFB6-D3E6F040B041")]
        public void AutoMapFieldsFromSavedSearch_ShouldThrowExceptionWhenAutomapRequestIsEmpty()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            AutomapRequest request = new AutomapRequest();
            SavedSearchFake savedSearch = CreateSavedSearchTest(_fields);

            // Act
            Func<Task> function = async () => await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId,
                    savedSearch.ArtifactId, _DESTINATION_PROVIDER_GUID);

            // Assert
            function.ShouldThrow<ArgumentNullException>();
        }

        [IdentifiedTest("80090507-41F8-4C35-9878-D263E54562C4")]
        public async Task AutoMapFieldsFromView_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromView");
            FieldsMapping.FieldInfo[] fieldsInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(fieldsInfo);
            ViewFake view = CreateViewTest(_fields);

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromView(request, SourceWorkspace.ArtifactId, view.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("A9E9A2B7-CA32-43DC-A655-3F3790EB4665")]
        public async Task AutoMapFieldsFromView_ShouldHasSuccessStatusCodeWhenReturnsEmptyList()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromView");
            FieldsMapping.FieldInfo[] fieldsInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(fieldsInfo);
            ViewFake view = new ViewFake();

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromView(request, SourceWorkspace.ArtifactId, view.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Should().BeEmpty();
        }

        [IdentifiedTest("B86CAB2F-4EA6-4882-A94E-1057A2F83EF0")]
        public async Task AutoMapFieldsFromView_ShouldReturnEmptyListWhenNoFieldsAreSetSavedSearch()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromView");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);
            ViewFake view = CreateViewTest(new List<FieldFake>());

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromView(request, SourceWorkspace.ArtifactId, view.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Should().BeEmpty();
        }

        [IdentifiedTest("60F22A32-A077-40FF-B459-9FD2B94C59F2")]
        public async Task AutoMapFieldsFromView_ShouldReturnOnlyFieldsInSavedSearch()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromView");
            FieldsMapping.FieldInfo[] documentsFieldInfo = ConvertFieldTestListToFieldInfoArray(_fields);
            AutomapRequest request = CreateAutomapRequest(documentsFieldInfo);

            List<FieldFake> savedSearchFields = CreateFieldsWithSpecialCharactersAsync(Const.LONG_TEXT_TYPE_ARTIFACT_ID, _LONG_TEXT_NAME).ToList();
            savedSearchFields.AddRange(_fields);
            ViewFake view = CreateViewTest(_fields);

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromView(request, SourceWorkspace.ArtifactId, view.ArtifactId, _DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("8D1BE3D3-0BC1-48B4-AFB6-D3E6F040B041")]
        public void AutoMapFieldsFromView_ShouldThrowExceptionWhenAutomapRequestIsEmpty()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromView");
            AutomapRequest request = new AutomapRequest();
            ViewFake view = CreateViewTest(_fields);

            // Act
            Func<Task> function = async () => await sut.AutoMapFieldsFromView(request, SourceWorkspace.ArtifactId,
                    view.ArtifactId, _DESTINATION_PROVIDER_GUID);

            // Assert
            function.ShouldThrow<ArgumentNullException>();
        }

        [IdentifiedTest("2210FA46-0247-4F80-AB0D-311713979628")]
        public async Task ValidateAsync_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/ValidateAsync");
            _fields[0].IsIdentifier = true;
            ((List<FieldFake>)SourceWorkspace.Fields).AddRange(_fields);
            ((List<FieldFake>)_destinationWorkspace.Fields).AddRange(_fields);

            IEnumerable<FieldMap> mappedFields = GetMappedFieldsWithIdentifierField();

            // Act
            HttpResponseMessage result = await sut.ValidateAsync(mappedFields, SourceWorkspace.ArtifactId, _destinationWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID);
            FieldMappingValidationResult fieldMappingValidationResult = await result.Content.ReadAsAsync<FieldMappingValidationResult>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldMappingValidationResult.InvalidMappedFields.Should().BeEmpty();
            fieldMappingValidationResult.IsObjectIdentifierMapValid.ShouldBeEquivalentTo(true);
        }

        [IdentifiedTest("9504F18E-1F8D-4CC1-833D-3B0DFA9E48E5")]
        public async Task ValidateAsync_ShouldResultInInvalidObjectIdentifierMapWhenMappedFieldsListIsEmpty()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/ValidateAsync");
            _fields[0].IsIdentifier = true;
            ((List<FieldFake>)_destinationWorkspace.Fields).AddRange(_fields);

            IEnumerable<FieldMap> mappedFields = new List<FieldMap>();

            // Act
            HttpResponseMessage result = await sut.ValidateAsync(mappedFields, SourceWorkspace.ArtifactId, _destinationWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID);
            FieldMappingValidationResult fieldMappingValidationResult = await result.Content.ReadAsAsync<FieldMappingValidationResult>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldMappingValidationResult.InvalidMappedFields.Should().BeEmpty();
            fieldMappingValidationResult.IsObjectIdentifierMapValid.ShouldBeEquivalentTo(false);
        }

        [IdentifiedTest("723249A3-A4CA-4FC4-BC81-DEC799743846")]
        public async Task ValidateAsync_ShouldResultInInvalidObjectIdentifierMapWhenMappedWhenThereAreNoFieldsInDestinationWorkspace()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/ValidateAsync");
            _fields[0].IsIdentifier = true;
            ((List<FieldFake>)SourceWorkspace.Fields).AddRange(_fields);

            IEnumerable<FieldMap> mappedFields = GetMappedFieldsWithIdentifierField();

            // Act
            HttpResponseMessage result = await sut.ValidateAsync(mappedFields, SourceWorkspace.ArtifactId, _destinationWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID);
            FieldMappingValidationResult fieldMappingValidationResult = await result.Content.ReadAsAsync<FieldMappingValidationResult>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldMappingValidationResult.InvalidMappedFields.Should().BeEmpty();
            fieldMappingValidationResult.IsObjectIdentifierMapValid.ShouldBeEquivalentTo(false);
        }

        [IdentifiedTest("093C93AA-9A4C-4ED8-AB50-131D76722176")]
        public async Task ValidateAsync_ShouldResultInInvalidObjectIdentifierMapWhenMappedWhenNoIdentifiers()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/ValidateAsync");
            ((List<FieldFake>)SourceWorkspace.Fields).AddRange(_fields);
            ((List<FieldFake>)_destinationWorkspace.Fields).AddRange(_fields);

            IEnumerable<FieldMap> mappedFields = SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();

            // Act
            HttpResponseMessage result = await sut.ValidateAsync(mappedFields, SourceWorkspace.ArtifactId, _destinationWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID);
            FieldMappingValidationResult fieldMappingValidationResult = await result.Content.ReadAsAsync<FieldMappingValidationResult>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldMappingValidationResult.InvalidMappedFields.Should().BeEmpty();
            fieldMappingValidationResult.IsObjectIdentifierMapValid.ShouldBeEquivalentTo(false);
        }

        [IdentifiedTest("79BCB223-DC6D-4D2C-B04A-2532CDFC28D3")]
        public async Task ValidateAsync_ShouldHaveInvalidMappedFieldsWhenNotAllSourceWorkspaceFieldsCanBeMappedToDestinationWorkspace()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/ValidateAsync");
            _fields[0].IsIdentifier = true;
            ((List<FieldFake>)SourceWorkspace.Fields).AddRange(_fields);
            List<FieldFake> longTextFields = CreateFieldsWithSpecialCharactersAsync(Const.LONG_TEXT_TYPE_ARTIFACT_ID, _LONG_TEXT_NAME).ToList();
            ((List<FieldFake>)_destinationWorkspace.Fields).AddRange(longTextFields);

            IEnumerable<FieldMap> mappedFields = SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();

            // Act
            HttpResponseMessage result = await sut.ValidateAsync(mappedFields, SourceWorkspace.ArtifactId, _destinationWorkspace.ArtifactId, _DESTINATION_PROVIDER_GUID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID);
            FieldMappingValidationResult fieldMappingValidationResult = await result.Content.ReadAsAsync<FieldMappingValidationResult>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldMappingValidationResult.InvalidMappedFields.Count.ShouldBeEquivalentTo(_fields.Count);
            fieldMappingValidationResult.IsObjectIdentifierMapValid.ShouldBeEquivalentTo(false);
        }

        private IEnumerable<FieldFake> CreateFieldsWithSpecialCharactersAsync(int fieldObjectTypeId, string fieldName)
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";
                var fieldTest = new FieldFake
                {
                    ObjectTypeId = fieldObjectTypeId,
                    Name = $"{generatedFieldName} {fieldName}",
                    IsIdentifier = false,
                };
                yield return fieldTest;
            }
        }

        private FieldMappingsController PrepareSut(HttpMethod method, string requestUri)
        {
            FieldMappingsController sut = Container.Resolve<FieldMappingsController>();
            sut.User = GetUserClaimsPrincipal();

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            sut.Request = request;

            return sut;
        }

        private ClaimsPrincipal GetUserClaimsPrincipal()
        {
            return new ClaimsPrincipal(new[]
                { new ClaimsIdentity(new[] { new Claim("rel_uai", User.ArtifactId.ToString())})});
        }

        private FieldsMapping.FieldInfo[] ConvertFieldTestListToFieldInfoArray(List<FieldFake> fields)
        {
            FieldsMapping.FieldInfo[] fieldsInfo = new FieldsMapping.FieldInfo[fields.Count];
            for (int i = 0; i < fields.Count; i++)
            {
                FieldFake field = fields[i];

                FieldsMapping.FieldInfo fieldInfo = new FieldsMapping.FieldInfo(
                    field.ArtifactId.ToString(),
                    field.Name,
                    field.ObjectTypeId == Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID ? _FIXED_LENGTH_TEXT_NAME : _LONG_TEXT_NAME)
                {
                    IsIdentifier = field.IsIdentifier,
                    IsRequired = false,
                };
                fieldsInfo[i] = fieldInfo;
            }

            return fieldsInfo;
        }

        private AutomapRequest CreateAutomapRequest(FieldsMapping.FieldInfo[] documentsFieldInfo, bool matchOnlyIdentifiers = false)
        {
            AutomapRequest request = new AutomapRequest
            {
                SourceFields = documentsFieldInfo,
                DestinationFields = documentsFieldInfo,
                MatchOnlyIdentifiers = matchOnlyIdentifiers
            };
            return request;
        }

        private SavedSearchFake CreateSavedSearchTest(List<FieldFake> fields)
        {
            SavedSearchFake savedSearch = new SavedSearchFake
            {
                Name = "Source Saved Search",
                Owner = "Adler Sieben",
                Artifact = { ArtifactId = ArtifactProvider.NextId() }
            };

            foreach (var field in fields)
            {
                savedSearch.Values.Add(Guid.NewGuid(), field);
            }

            SourceWorkspace.SavedSearches.Add(savedSearch);
            return savedSearch;
        }

        private ViewFake CreateViewTest(List<FieldFake> fields)
        {
            ViewFake view = new ViewFake
            {
                Name = "Source View",
                Artifact = { ArtifactId = ArtifactProvider.NextId() }
            };

            foreach (var field in fields)
            {
                view.Values.Add(Guid.NewGuid(), field);
            }

            SourceWorkspace.Views.Add(view);
            return view;
        }

        private IEnumerable<FieldMap> GetMappedFieldsWithIdentifierField()
        {
            IEnumerable<FieldMap> mappedFields =
                SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
            mappedFields.First().SourceField.IsIdentifier = true;
            mappedFields.First().FieldMapType = FieldMapTypeEnum.Identifier;
            mappedFields.First().DestinationField.IsIdentifier = true;
            return mappedFields;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class FieldMappingsControllerTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;
        private List<FieldTest> _fields;

        private const string FIXED_LENGTH_TEXT_NAME = "Fixed-Length Text";
        private const string DESTINATION_PROVIDER_GUID = "D2E10795-3A47-47C2-A457-7E54C2A5DD90";

        public override void SetUp()
        {
            base.SetUp();

            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspaceWithIntegrationPointsApp(destinationWorkspaceArtifactId);
            _fields = CreateFixedLengthTextFieldsWithSpecialCharactersAsync().ToList();
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public async Task GetMappableFieldsFromSourceWorkspace_ShouldReturnAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");
            ((List<FieldTest>)SourceWorkspace.Fields).AddRange(_fields);

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromSourceWorkspace(SourceWorkspace.ArtifactId);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Count.Should().Be(_fields.Count);
            fieldsDTOs.Select(x => x.Name).ShouldBeEquivalentTo(_fields.Select(x=> x.Name));
        }

        [IdentifiedTest("BE420CE7-3AEA-41F9-BDC7-C381DBC3FCE4")]
        public async Task GetMappableFieldsFromDestinationWorkspace_ShouldReturnAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromDestinationWorkspace");
            ((List<FieldTest>)_destinationWorkspace.Fields).AddRange(_fields);

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromDestinationWorkspace(_destinationWorkspace.ArtifactId);
            List<ClassifiedFieldDTO> fieldsDTOs = await result.Content.ReadAsAsync<List<ClassifiedFieldDTO>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsDTOs.Count.Should().Be(_fields.Count);
            fieldsDTOs.Select(x => x.Name).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("B32FC4F0-EE3C-43BA-B02A-509E8F496FFE")]
        public async Task AutoMapFields_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFields");
            ((List<FieldTest>)SourceWorkspace.Fields).AddRange(_fields);

            DocumentFieldInfo[] documentsFieldInfo = ConvertFieldTestListToDocumentFieldInfoArray(_fields);

            AutomapRequest request = new AutomapRequest
            {
                SourceFields = documentsFieldInfo,
                DestinationFields = documentsFieldInfo,
                MatchOnlyIdentifiers = false
            };

            // Act
            HttpResponseMessage result = await sut.AutoMapFields(request, SourceWorkspace.ArtifactId, DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        [IdentifiedTest("80090507-41F8-4C35-9878-D263E54562C4")]
        public async Task AutoMapFieldsFromSavedSearch_ShouldMapAllFixedLengthTextFields()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Post, "/AutoMapFieldsFromSavedSearch");
            ((List<FieldTest>)SourceWorkspace.Fields).AddRange(_fields);
            IList<SavedSearchTest> savedSearches = SourceWorkspace.SavedSearches;

            DocumentFieldInfo[] documentsFieldInfo = ConvertFieldTestListToDocumentFieldInfoArray(_fields);

            AutomapRequest request = new AutomapRequest
            {
                SourceFields = documentsFieldInfo,
                DestinationFields = documentsFieldInfo,
                MatchOnlyIdentifiers = false
            };

            // Act
            HttpResponseMessage result = await sut.AutoMapFieldsFromSavedSearch(request, SourceWorkspace.ArtifactId, savedSearches[0].ArtifactId, DESTINATION_PROVIDER_GUID);
            List<FieldMap> fieldsMaps = await result.Content.ReadAsAsync<List<FieldMap>>();

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            fieldsMaps.Count.Should().Be(_fields.Count);
            fieldsMaps.Select(x => x.SourceField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
            fieldsMaps.Select(x => x.DestinationField.DisplayName).ShouldBeEquivalentTo(_fields.Select(x => x.Name));
        }

        private DocumentFieldInfo[] ConvertFieldTestListToDocumentFieldInfoArray(List<FieldTest> fields)
        {
            DocumentFieldInfo[] documentsFieldInfo = new DocumentFieldInfo[fields.Count];
            for (int i = 0; i < fields.Count; i++)
            {
                FieldTest field = fields[i];

                DocumentFieldInfo documentFieldInfo = new DocumentFieldInfo(
                    field.ArtifactId.ToString(),
                    field.Name,
                    FIXED_LENGTH_TEXT_NAME)
                {
                    IsIdentifier = false,
                    IsRequired = false,
                };
                documentsFieldInfo[i] = documentFieldInfo;
            }

            return documentsFieldInfo;
        }

        private IEnumerable<FieldTest> CreateFixedLengthTextFieldsWithSpecialCharactersAsync()
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";
                var fixedLengthTextFieldRequest = new FieldTest
                {
                    ObjectTypeId = Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID,
                    Name = $"{generatedFieldName} {FIXED_LENGTH_TEXT_NAME}",
                    IsIdentifier = false,
                };
                yield return fixedLengthTextFieldRequest;
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
                {new ClaimsIdentity(new[] {new Claim("rel_uai", User.ArtifactId.ToString())})});
        }
    }
}

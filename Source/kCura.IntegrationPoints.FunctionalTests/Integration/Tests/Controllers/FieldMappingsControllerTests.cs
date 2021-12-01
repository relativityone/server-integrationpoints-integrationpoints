using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class FieldMappingsControllerTests : TestsBase
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
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareLongTextFieldsMapping();
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public async Task GetMappableFieldsFromSourceWorkspace_SampleTest()
        {
            // Arrange
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");
            //SourceWorkspace.Fields

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromDestinationWorkspace(SourceWorkspace.ArtifactId);

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
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

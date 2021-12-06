using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;
using Field = Relativity.Services.Objects.DataContracts.Field;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class FieldMappingsControllerTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;
        private const string FIXED_LENGTH_TEXT_NAME = "Fixed-Length Text";
        private const string LONG_TEXT_NAME = "Long Text";

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
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareLongTextFieldsMapping();
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public async Task GetMappableFieldsFromSourceWorkspace_SampleTest()
        {
            // Arrange
            IEnumerable<RelativityObject> fixedLengthTextFields =
                CreateFieldsWithSpecialCharactersAsync(FIXED_LENGTH_TEXT_NAME);
            IEnumerable<RelativityObject> longTextFields =
                CreateFieldsWithSpecialCharactersAsync(LONG_TEXT_NAME);

            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromSourceWorkspace(SourceWorkspace.ArtifactId);

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
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

        private IEnumerable<RelativityObject> CreateFieldsWithSpecialCharactersAsync(string fieldType)
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();

            var fieldObjects = new List<RelativityObject>();

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";

                fieldObjects.Add(new RelativityObject
                    {
                        ArtifactID = ArtifactProvider.NextId(),
                        Name = generatedFieldName,
                        FieldValues = new List<FieldValuePair>
                        {
                            new FieldValuePair()
                            {
                                Field = new Field()
                                {
                                    Name = "Is Identifier"
                                },
                                Value = i == 0
                            },
                            new FieldValuePair()
                            {
                                Field = new Field()
                                {
                                    Name = "Field Type",
                                },
                                Value = fieldType
                            }
                        }
                    }
                );
            }
            return fieldObjects;
        }

        private ClaimsPrincipal GetUserClaimsPrincipal()
        {
            return new ClaimsPrincipal(new[]
                {new ClaimsIdentity(new[] {new Claim("rel_uai", User.ArtifactId.ToString())})});
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
                    IsIdentifier = false,
                };

                workspace.Fields.Add(fixedLengthTextFieldRequest);

                var longTextFieldRequest = new FieldTest
                {
                    ObjectTypeId = Const.LONG_TEXT_TYPE_ARTIFACT_ID,
                    Name = $"{generatedFieldName} Long Text",
                    IsIdentifier = false,
                };

                workspace.Fields.Add(longTextFieldRequest);
                //fieldManager.CreateLongTextFieldAsync(workspaceId, longTextFieldRequest).ConfigureAwait(false);
                //fieldManager.CreateFixedLengthFieldAsync(workspaceId, fixedLengthTextFieldRequest).ConfigureAwait(false);
            }

            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareLongTextFieldsMapping();
            SourceWorkspace.Helpers.FieldsMappingHelper.PrepareFixedLengthTextFieldsMapping();
        }
    }
}

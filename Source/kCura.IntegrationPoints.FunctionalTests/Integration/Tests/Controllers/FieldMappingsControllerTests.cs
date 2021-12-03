using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class FieldMappingsControllerTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;
        private const string FIXED_LENGTH_TEXT_NAME = "Fixed-Length Text";
        private const string LONG_TEXT_NAME = "Long Text";
        private FakeFieldsRepository _fakeFieldsRepository;

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

            _fakeFieldsRepository = Container.Resolve<IFieldsRepository>() as FakeFieldsRepository;
            
        }

        [IdentifiedTest("AE9E4DD3-6E12-4000-BDE7-6CFDAE14F1EB")]
        public async Task GetMappableFieldsFromSourceWorkspace_SampleTest()
        {
            // Arrange
            IEnumerable<RelativityObject> fixedLengthTextFields =
                CreateFieldsWithSpecialCharactersAsync(FIXED_LENGTH_TEXT_NAME);
            IEnumerable<RelativityObject> longTextFields =
                CreateFieldsWithSpecialCharactersAsync(LONG_TEXT_NAME);
            _fakeFieldsRepository.WorkspacesFields = new List<Tuple<int, IEnumerable<RelativityObject>>>
            {
                new Tuple<int, IEnumerable<RelativityObject>>(SourceWorkspace.ArtifactId, fixedLengthTextFields)
            };
            FieldMappingsController sut = PrepareSut(HttpMethod.Get, "/GetMappableFieldsFromSourceWorkspace");

            // Act
            HttpResponseMessage result = await sut.GetMappableFieldsFromDestinationWorkspace(SourceWorkspace.ArtifactId);

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
    }
}

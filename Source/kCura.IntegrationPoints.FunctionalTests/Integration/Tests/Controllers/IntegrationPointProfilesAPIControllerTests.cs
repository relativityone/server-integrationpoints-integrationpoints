using FluentAssertions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Controllers.API;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class IntegrationPointProfilesAPIControllerTests: TestsBase
    {
        private int _longTextLimit;
        private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
        private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

        [SetUp]
        public void Setup()
        {
            var instanceSettingRepository = Container.Resolve<IInstanceSettingRepository>();
            _longTextLimit = Convert.ToInt32(instanceSettingRepository.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void GetAll_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfileFirst = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            IntegrationPointProfileTest integrationPointProfileSecond = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, "/getall");

            // Act
            var response = sut.GetAll();

            // Assert
            var objectContent = response.Content as ObjectContent;
            var result = (List<IntegrationPointProfileDto>)objectContent?.Value;
            var expected = SourceWorkspace.IntegrationPointProfiles.Select(x => x.ToIntegrationPointProfileModel());

            result.ShouldAllBeEquivalentTo(expected);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");

            // Act
            var response = sut.Get(integrationPointProfile.ArtifactId);

            // Assert
            var result = FormatResponseToGetValueFromObjectContent<IntegrationPointProfileDto>(response);
            var expected = SourceWorkspace.IntegrationPointProfiles.Select(x => x.ToIntegrationPointProfileModel()).First();

            result.ShouldBeEquivalentTo(expected);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void GetValidatedProfileModel_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"GetValidatedProfileModel/{integrationPointProfileArtifactId}");

            // Act
            var response = sut.Get(integrationPointProfile.ArtifactId);

            // Assert
            var result = FormatResponseToGetValueFromObjectContent<IntegrationPointProfileDto>(response);
            var expected = SourceWorkspace.IntegrationPointProfiles.Select(x => x.ToIntegrationPointProfileModel()).First();

            result.ShouldBeEquivalentTo(expected);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void GetByType_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            int typeArtifactId = integrationPointProfile.Type;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/GetByType/{typeArtifactId}");

            // Act
            var response = sut.GetByType(integrationPointProfile.Type);

            // Assert
            var result = FormatResponseToGetValueFromObjectContent<IEnumerable<IntegrationPointProfileDto>>(response);

            AssertIntegrationPointProfilesSimpleMatches(integrationPointProfile, result.First());
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableSourceConfiguration_When_LongTextFieldLimitExceeded()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfileWithDeserializableSourceConfiguration(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");

            // Act
            var response = sut.Get(integrationPointProfile.ArtifactId);

            // Assert

            var result = FormatResponseToGetValueFromObjectContent<IntegrationPointProfileDto>(response);

            response.IsSuccessStatusCode.Should().BeTrue();
            AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(result.SourceConfiguration);
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableDestinationConfiguration_When_LongTextFieldLimitExceeded()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfileWithDeserializableDestinationConfiguration(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");

            // Act
            var response = sut.Get(integrationPointProfile.ArtifactId);

            // Assert
            var result = FormatResponseToGetValueFromObjectContent<IntegrationPointProfileDto>(response);

            response.IsSuccessStatusCode.Should().BeTrue();
            AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(result.DestinationConfiguration);
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableFieldMappings_When_LongTextFieldLimitExceeded()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfileWithDeserializableFieldMappings(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");

            // Act
            var response = sut.Get(integrationPointProfile.ArtifactId);

            // Assert
            var result = FormatResponseToGetValueFromObjectContent<IntegrationPointProfileDto>(response);

            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Save_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileDto integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointAsIntegrationPointProfileModel(destinationWorkspace);
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Post, "");
            var expectedResult = new { returnURL = "RelativityViewUrlMock" };

            // Act
            var response = sut.Save(SourceWorkspace.ArtifactId, integrationPointProfile);

            // Assert
            var objectContent = response.Content as ObjectContent;
            var result = objectContent?.Value;

            result.ShouldBeEquivalentTo(expectedResult);
            SourceWorkspace.IntegrationPointProfiles.Count.Should().Be(1);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void SaveUsingIntegrationPoint_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointDto integrationPointDto = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPointModel(destinationWorkspace);
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Post, "");
            IntegrationPointProfileFromIntegrationPointModel integrationPointProfileFromIntegrationPointModel = new IntegrationPointProfileFromIntegrationPointModel
            {
                IntegrationPointArtifactId = integrationPointDto.ArtifactId,
                ProfileName = "TestIntegrationPointProfile"
            };
            var expectedResult = new { returnURL = "RelativityViewUrlMock" };

            // Act
            var response = sut.SaveUsingIntegrationPoint(SourceWorkspace.ArtifactId, integrationPointProfileFromIntegrationPointModel);

            // Assert
            var objectContent = response.Content as ObjectContent;
            var result = objectContent?.Value;

            result.ShouldBeEquivalentTo(expectedResult);
            SourceWorkspace.IntegrationPointProfiles.Count.Should().Be(1);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        protected ClaimsPrincipal GetUserClaimsPrincipal() => new ClaimsPrincipal(new[]
            {new ClaimsIdentity(new[] {new Claim("rel_uai", User.ArtifactId.ToString())})});

        private IntegrationPointProfilesAPIController PrepareSut(HttpMethod method, string requestUri)
        {
            IntegrationPointProfilesAPIController sut = Container.Resolve<IntegrationPointProfilesAPIController>();
            sut.User = GetUserClaimsPrincipal();

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            sut.Request = request;

            return sut;
        }
        private void AssertFieldDeserializationDoesNotThrow<T>(string fieldTextValue)
        {
            Action deserializeSourceConfig = () => Serializer.Deserialize<T>(fieldTextValue);
            deserializeSourceConfig.ShouldNotThrow();
        }

        private T FormatResponseToGetValueFromObjectContent<T>(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            var result = (T)objectContent?.Value;
            return result;
        }

        private void AssertIntegrationPointProfilesSimpleMatches(IntegrationPointProfileTest initial, IntegrationPointProfileDto result)
        {
            result.ArtifactId.Should().Be(initial.ArtifactId);
            result.Name.Should().Be(initial.Name);
            result.SourceProvider.Should().Be(initial.SourceProvider);
            result.DestinationProvider.Should().Be(initial.DestinationProvider);
        }
    }
}

using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Installers;
using kCura.IntegrationPoints.Web.Installers.Context;
using kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices;
using kCura.IntegrationPoints.Web.Models;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class IntegrationPointProfilesAPIControllerTests: TestsBase
    {
        private int _longTextLimit; //value should be 100000
        private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
        private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

        [SetUp]
        public void Setup()
        {
            var instanceSettingRepository = Container.Resolve<IInstanceSettingRepository>();
            _longTextLimit = Convert.ToInt32(instanceSettingRepository.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));
        }

        protected override WindsorContainer GetContainer()
        {
            var container = base.GetContainer();
            container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<FakeRelativityUrlHelper>());
            container.Register(Component.For<IntegrationPointProfilesAPIController>().ImplementedBy<IntegrationPointProfilesAPIController>().LifestyleTransient());
            container.Register(Component.For<IInstanceSettingRepository>().ImplementedBy<FakeInstanceSettingRepository>().LifestyleTransient().IsDefault());
            return container;
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void GetAll_ShouldReturnSuccessStatusCode()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPoint(destinationWorkspace);
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, "/getall");
            var response = sut.GetAll();
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ShouldReturnSuccessStatusCode()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPoint(destinationWorkspace);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");
            var response = sut.Get(integrationPointProfile.ArtifactId);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableSourceConfiguration()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointWithDeserializableSourceConfiguration(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");
            var response = sut.Get(integrationPointProfile.ArtifactId);
            var objectContent = response.Content as ObjectContent;
            var result = (IntegrationPointProfileModel)objectContent?.Value;

            response.IsSuccessStatusCode.Should().BeTrue();
            AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(result.SourceConfiguration);
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableDestinationConfiguration()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointWithDeserializableDestinationConfiguration(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");
            var response = sut.Get(integrationPointProfile.ArtifactId);
            var objectContent = response.Content as ObjectContent;
            var result = (IntegrationPointProfileModel)objectContent?.Value;

            response.IsSuccessStatusCode.Should().BeTrue();
            AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(result.Destination);
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Get_ReturnSuccessStatusCode_And_ReturnDeserializableFieldMappings()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointWithDeserializableFieldMappings(destinationWorkspace, _longTextLimit);
            int integrationPointProfileArtifactId = integrationPointProfile.ArtifactId;
            IntegrationPointProfilesAPIController sut = PrepareSut(HttpMethod.Get, $"/{integrationPointProfileArtifactId}");
            var response = sut.Get(integrationPointProfile.ArtifactId);
            var objectContent = response.Content as ObjectContent;
            var result = (IntegrationPointProfileModel)objectContent?.Value;

            response.IsSuccessStatusCode.Should().BeTrue();
            AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(result.Map);
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
    }
}

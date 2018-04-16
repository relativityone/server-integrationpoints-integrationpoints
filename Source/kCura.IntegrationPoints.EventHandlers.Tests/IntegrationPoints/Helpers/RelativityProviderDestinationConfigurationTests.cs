using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Artifact = kCura.Relativity.Client.Artifact;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
	public class RelativityProviderDestinationConfigurationTests
	{
		private RelativityProviderDestinationConfiguration _instace;
		private IEHHelper _helper;
		private IFederatedInstanceManager _federatedInstanceManager;
		private IObjectTypeRepository _rsapiRdoQuery;
		private const int _ARTIFACT_TYPE_ID = 0;
		private const int _FEDERATED_INSTANCE_ID = 3;
		private const string _ARTIFACT_TYPE_NAME = "ArtifactTypeName";
		private const string _DESTINATION_RELATIVITY_INSTANCE = "DestinationRelativityInstance";

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IEHHelper>();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			_rsapiRdoQuery = Substitute.For<IObjectTypeRepository>();

			var repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetObjectTypeRepository(Arg.Any<int>()).Returns(_rsapiRdoQuery);

			_instace = new RelativityProviderDestinationConfiguration(_helper, _federatedInstanceManager, repositoryFactory);
		}

		[TestCase("Other Instance")]
		public void ItShouldUpdateNames(string instanceName)
		{
			// arrange
			var settings = GetSettings();
			MockArtifactTypeNameQuery(_ARTIFACT_TYPE_ID);
			MockFederatedInstanceManager(_FEDERATED_INSTANCE_ID, instanceName);

			// act
			_instace.UpdateNames(settings, new EventHandler.Artifact(934580, 990562, 533988, "", false, null));

			//assert
			Assert.AreEqual("RDO", settings[_ARTIFACT_TYPE_NAME]);
			Assert.AreEqual(instanceName, settings[_DESTINATION_RELATIVITY_INSTANCE]);
		}

		private void MockArtifactTypeNameQuery(int transferredObjArtifactTypeId)
		{
			var field = new Field
			{
				Name = "Text Identifier",
				Value = transferredObjArtifactTypeId
			};
			var artifact = new Artifact { Fields = new List<Field>() { field } };
			var queryResult = new QueryResult()
			{
				Success = true
			};
			queryResult.QueryArtifacts.Add(artifact);

			var rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.APIOptions = new APIOptions();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<kCura.Relativity.Client.Query>()).Returns(queryResult);
			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(rsapiClient);
		}
	
		private void MockFederatedInstanceManager(int instanceId,string federatedInstanceName)
		{
			var federatedInstanceDto = Substitute.For<FederatedInstanceDto>();
			federatedInstanceDto.Name = federatedInstanceName;
			_federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(instanceId).Returns(federatedInstanceDto);
		}

		private IDictionary<string, object> GetSettings()
		{
			var settings = new Dictionary<string, object>
			{
				{nameof(DestinationConfiguration.ArtifactTypeId), _ARTIFACT_TYPE_ID},
				{nameof(DestinationConfiguration.FederatedInstanceArtifactId), _FEDERATED_INSTANCE_ID}
			};

			return settings;
		}
	}
}

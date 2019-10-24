using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
	[TestFixture]
	public class IntegrationPointProfilesQueryTests
	{
		private IntegrationPointProfilesQuery _sut;
		private Mock<IRelativityObjectManager> _fakeRelativityObjectManager;
		private Mock<IObjectArtifactIdsByStringFieldValueQuery> _fakeObjectArtifactIDsQuery;
		private ISerializer _serializer;
		private List<IntegrationPointProfile> _syncProfiles;
		private List<IntegrationPointProfile> _nonSyncProfiles;
		private List<IntegrationPointProfile> _allProfiles;
		private List<int> _relativitySourceProvidersList;
		private List<int> _relativityDestinationProvidersList;
		private List<int> _integrationPointTypesList;
		private const int _WORKSPACE_ID = 100111;

		private const int _RELATIVITY_DESTINATION_PROVIDER_ID = 500111;
		private const int _RELATIVITY_SOURCE_PROVIDER_ID = 500222;
		private const int _NON_RELATIVITY_DESTINATION_PROVIDER_ID = 600111;
		private const int _NON_RELATIVITY_SOURCE_PROVIDER_ID = 600222;
		private const int _INTEGRATION_POINT_EXPORT_TYPE_ID = 7000333;

		[SetUp]
		public void SetUp()
		{
			_fakeRelativityObjectManager = new Mock<IRelativityObjectManager>();
			_fakeObjectArtifactIDsQuery = new Mock<IObjectArtifactIdsByStringFieldValueQuery>();
			_serializer = new JSONSerializer();

			_syncProfiles = CreateSyncProfiles().ToList();
			_nonSyncProfiles = CreateNonSyncProfiles().ToList();
			_allProfiles = new List<IntegrationPointProfile>();
			_allProfiles.AddRange(_syncProfiles);
			_allProfiles.AddRange(_nonSyncProfiles);

			_fakeRelativityObjectManager
				.Setup(x => x.QueryAsync<IntegrationPointProfile>(
					It.IsAny<QueryRequest>(), false, It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(_allProfiles);

			_relativitySourceProvidersList = new List<int>();
			_relativityDestinationProvidersList = new List<int>();
			_integrationPointTypesList = new List<int>();

			_fakeObjectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(DestinationProvider provider) => provider.Identifier,
					Constants.IntegrationPoints.DestinationProviders.RELATIVITY))
				.ReturnsAsync(_relativityDestinationProvidersList);

			_fakeObjectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(SourceProvider provider) => provider.Identifier,
					Constants.IntegrationPoints.SourceProviders.RELATIVITY))
				.ReturnsAsync(_relativitySourceProvidersList);

			_fakeObjectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(IntegrationPointType integrationPointType) => integrationPointType.Identifier,
					kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()))
				.ReturnsAsync(_integrationPointTypesList);
			
			_sut = new IntegrationPointProfilesQuery(
				workspaceID => _fakeRelativityObjectManager.Object,
				_fakeObjectArtifactIDsQuery.Object,
				_serializer);
		}

		[Test]
		public async Task ItShouldReturnAllProfiles()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			List<IntegrationPointProfile> allProfiles = (await _sut
				.GetAllProfilesAsync(_WORKSPACE_ID)
				.ConfigureAwait(false)).ToList();

			// Assert
			CollectionAssert.AreEquivalent(_allProfiles, allProfiles);
		}

		[Test]
		public void ItShouldGetOnlySyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			IEnumerable<IntegrationPointProfile> syncProfiles = _sut
				.GetSyncProfiles(_allProfiles, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID);

			// Assert
			CollectionAssert.AreEquivalent(_syncProfiles, syncProfiles);
		}

		[Test]
		public void ItShouldGetOnlyNonSyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = _sut
				.GetNonSyncProfiles(_allProfiles, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID);

			// Assert
			CollectionAssert.AreEquivalent(_nonSyncProfiles, nonSyncProfiles);
		}

		[Test]
		public async Task ItShouldReturnSyncSourceProviderArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int sourceProviderID = await _sut.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			sourceProviderID.Should().Be(_RELATIVITY_SOURCE_PROVIDER_ID);
		}

		[Test]
		public async Task ItShouldReturnIntegrationPointExportTypeArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int sourceProviderID = await _sut.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			sourceProviderID.Should().Be(_INTEGRATION_POINT_EXPORT_TYPE_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfIntegrationPointExportTypeArtifactID([Values(0, 2)] int integrationPointTypesCount)
		{
			// Arrange
			SetUpSyncProviders(integrationPointTypesCount: integrationPointTypesCount);

			// Act
			Func<Task<int>> action = () => _sut.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(action);
		}

		[Test]
		public async Task ItShouldReturnNonSyncSourceProviderArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int destinationProviderArtifactID = await _sut.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			destinationProviderArtifactID.Should().Be(_RELATIVITY_DESTINATION_PROVIDER_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfSourceProviders([Values(0, 2)] int relativitySourceProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativitySourceProviderCount);

			// Act
			Func<Task<int>> action = () => _sut.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(action);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfDestinationProviders([Values(0, 2)] int relativityDestinationProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativityDestinationProviderCount: relativityDestinationProviderCount);

			// Act
			Func<Task<int>> action = () => _sut.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(action);
		}

		private static void AssertWrongNumberOfArtifactIDsInCollection(Func<Task<int>> sut)
		{
			sut.ShouldThrowExactly<InvalidOperationException>().Which.Message.Should()
				.BeOneOf("Sequence contains more than one element", "Sequence contains no elements");
		}

		private IEnumerable<IntegrationPointProfile> CreateSyncProfiles()
		{
			yield return new IntegrationPointProfile
			{
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = false
				})
			};
		}

		private IEnumerable<IntegrationPointProfile> CreateNonSyncProfiles()
		{
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.ProductionSet
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = true
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = false
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = false
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = false
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.ProductionSet
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = true
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = true
				})
			};
			yield return new IntegrationPointProfile()
			{
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
				SourceConfiguration = _serializer.Serialize(new SourceConfiguration()
				{
					TypeOfExport = SourceConfiguration.ExportType.ProductionSet
				}),
				DestinationConfiguration = _serializer.Serialize(new ImportSettings()
				{
					ProductionImport = false
				})
			};
		}

		private void SetUpSyncProviders(int relativitySourceProviderCount = 1, int relativityDestinationProviderCount = 1, int integrationPointTypesCount = 1)
		{
			_relativitySourceProvidersList.AddRange(Enumerable
				.Repeat(_RELATIVITY_SOURCE_PROVIDER_ID, relativitySourceProviderCount));
			_relativityDestinationProvidersList.AddRange(Enumerable
				.Repeat(_RELATIVITY_DESTINATION_PROVIDER_ID, relativityDestinationProviderCount));
			_integrationPointTypesList.AddRange(Enumerable
				.Repeat(_INTEGRATION_POINT_EXPORT_TYPE_ID, integrationPointTypesCount));
		}
	}
}

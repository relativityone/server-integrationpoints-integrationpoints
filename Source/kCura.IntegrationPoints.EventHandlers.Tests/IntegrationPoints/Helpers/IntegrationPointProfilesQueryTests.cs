using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
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
		private IntegrationPointProfilesQuery _query;
		private Mock<IRelativityObjectManager> _relativityObjectManager;
		private Mock<IObjectArtifactIdsByStringFieldValueQuery> _objectArtifactIDsQuery;
		private List<IntegrationPointProfile> _profilesList;
		private List<int> _relativitySourceProvidersList;
		private List<int> _relativityDestinationProvidersList;
		private List<int> _integrationPointTypesList;
		private const int _WORKSPACE_ID = 100111;

		private const int _RELATIVITY_DESTINATION_PROVIDER_ID = 500111;
		private const int _RELATIVITY_SOURCE_PROVIDER_ID = 500222;
		private const int _NON_RELATIVITY_DESTINATION_PROVIDER_ID = 600111;
		private const int _NON_RELATIVITY_SOURCE_PROVIDER_ID = 600222;
		private const int _INTEGRATION_POINT_EXPORT_TYPE_ID = 7000333;

		private const int _SYNC_PROFILE_ID = 900111;
		private const int _NON_SYNC_PROFILE_ID = 900222;

		[SetUp]
		public void SetUp()
		{
			_relativityObjectManager = new Mock<IRelativityObjectManager>();
			_objectArtifactIDsQuery = new Mock<IObjectArtifactIdsByStringFieldValueQuery>();

			_query = new IntegrationPointProfilesQuery(
				workspaceID => _relativityObjectManager.Object,
				_objectArtifactIDsQuery.Object);

			_profilesList = new List<IntegrationPointProfile>();
			_relativitySourceProvidersList = new List<int>();
			_relativityDestinationProvidersList = new List<int>();
			_integrationPointTypesList = new List<int>();

			_relativityObjectManager
				.Setup(x => x.QueryAsync<IntegrationPointProfile>(
					It.IsAny<QueryRequest>(), false, It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(_profilesList);

			_objectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(DestinationProvider provider) => provider.Identifier,
					Constants.IntegrationPoints.DestinationProviders.RELATIVITY))
				.ReturnsAsync(_relativityDestinationProvidersList);

			_objectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(SourceProvider provider) => provider.Identifier,
					Constants.IntegrationPoints.SourceProviders.RELATIVITY))
				.ReturnsAsync(_relativitySourceProvidersList);

			_objectArtifactIDsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(IntegrationPointType integrationPointType) => integrationPointType.Identifier,
					kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()))
				.ReturnsAsync(_integrationPointTypesList);
		}

		[Test]
		public async Task ItShouldReturnAllProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(syncProfilesCount: 1, nonSyncProfilesCount: 1);

			// Act
			List<IntegrationPointProfile> allProfiles = (await _query
				.GetAllProfilesAsync(_WORKSPACE_ID)
				.ConfigureAwait(false)).ToList();

			// Assert
			IntegrationPointProfile syncProfile = allProfiles.FirstOrDefault(x => x.ArtifactId == _SYNC_PROFILE_ID);
			syncProfile.Should().NotBeNull();
			IntegrationPointProfile nonSyncProfile = allProfiles.FirstOrDefault(x => x.ArtifactId == _NON_SYNC_PROFILE_ID);
			nonSyncProfile.Should().NotBeNull();
		}

		[Test]
		public async Task ItShouldReturnSyncSourceProviderArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int sourceProviderID = await _query.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			sourceProviderID.Should().Be(_RELATIVITY_SOURCE_PROVIDER_ID);
		}

		[Test]
		public async Task ItShouldReturnIntegrationPointExportTypeArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int sourceProviderID = await _query.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			sourceProviderID.Should().Be(_INTEGRATION_POINT_EXPORT_TYPE_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfIntegrationPointExportTypeArtifactID([Values(0, 2)] int integrationPointTypesCount)
		{
			// Arrange
			SetUpSyncProviders(integrationPointTypesCount: integrationPointTypesCount);

			// Act
			Func<Task<int>> sut = () => _query.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(sut);
		}

		[Test]
		public async Task ItShouldReturnNonSyncSourceProviderArtifactID()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int destinationProviderArtifactID = await _query.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			destinationProviderArtifactID.Should().Be(_RELATIVITY_DESTINATION_PROVIDER_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfSourceProviders([Values(0, 2)] int relativitySourceProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativitySourceProviderCount);

			// Act
			Func<Task<int>> sut = () => _query.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(sut);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfDestinationProviders([Values(0, 2)] int relativityDestinationProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativityDestinationProviderCount: relativityDestinationProviderCount);

			// Act
			Func<Task<int>> sut = () => _query.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID);

			// Assert
			AssertWrongNumberOfArtifactIDsInCollection(sut);
		}

		[Test]
		public async Task ItShouldGetOnlySyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(syncProfilesCount: 1, nonSyncProfilesCount: 1);

			// Act
			IEnumerable<IntegrationPointProfile> syncProfiles = await _query
				.GetSyncProfilesAsync(_profilesList, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID).ConfigureAwait(false);

			// Assert
			syncProfiles.Should().ContainSingle(x => x.ArtifactId == _SYNC_PROFILE_ID);
		}

		[Test]
		public async Task ItShouldGetOnlyNonSyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(syncProfilesCount: 1, nonSyncProfilesCount: 1);

			// Act
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = await _query
				.GetNonSyncProfilesAsync(_profilesList, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID).ConfigureAwait(false);

			// Assert
			nonSyncProfiles.Should().ContainSingle(x => x.ArtifactId == _NON_SYNC_PROFILE_ID);
		}

		private static void AssertWrongNumberOfArtifactIDsInCollection(Func<Task<int>> sut)
		{
			sut.ShouldThrowExactly<InvalidOperationException>().Which.Message.Should()
				.BeOneOf("Sequence contains more than one element", "Sequence contains no elements");
		}

		private void SetUpProfiles(int syncProfilesCount, int nonSyncProfilesCount)
		{
			IntegrationPointProfile CreateSyncProfile() => new IntegrationPointProfile
			{
				ArtifactId = _SYNC_PROFILE_ID,
				SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
				DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID
			};

			IntegrationPointProfile CreateNonSyncProfile(bool isImportProvider)
			{
				var profile = new IntegrationPointProfile
				{
					ArtifactId = _NON_SYNC_PROFILE_ID,
					SourceProvider = isImportProvider
						? _NON_RELATIVITY_SOURCE_PROVIDER_ID
						: _RELATIVITY_SOURCE_PROVIDER_ID,
					DestinationProvider = isImportProvider
						? _RELATIVITY_DESTINATION_PROVIDER_ID
						: _NON_RELATIVITY_DESTINATION_PROVIDER_ID
				};
				return profile;
			}

			// create non sync profiles
			// half of them should be import, half export
			int importProvidersCount = nonSyncProfilesCount / 2;
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = Enumerable
				.Range(0, nonSyncProfilesCount)
				.Select(i => CreateNonSyncProfile(i < importProvidersCount));
			var queue = new Queue<IntegrationPointProfile>(nonSyncProfiles);

			// randomly distribute profiles
			IEnumerable<IntegrationPointProfile> profiles =
				GenerateShuffledIntSequence(syncProfilesCount + nonSyncProfilesCount)
					.Select(x => (x < syncProfilesCount) ? CreateSyncProfile() : queue.Dequeue());

			_profilesList.AddRange(profiles);
		}

		private static IEnumerable<int> GenerateShuffledIntSequence(int count)
		{
			int[] deck = Enumerable
				.Range(0, count)
				.ToArray();
			var r = new Random();

			// Fisher-Yates algorithm
			for (int n = deck.Length - 1; n > 0; --n)
			{
				int k = r.Next(n + 1);
				int temp = deck[n];
				deck[n] = deck[k];
				deck[k] = temp;
			}
			return deck;
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

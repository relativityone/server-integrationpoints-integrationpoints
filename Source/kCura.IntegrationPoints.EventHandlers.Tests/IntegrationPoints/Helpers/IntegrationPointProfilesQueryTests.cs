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
		private Mock<IObjectArtifactIdsByStringFieldValueQuery> _objectArtifactIdsQuery;
		private List<IntegrationPointProfile> _profilesList;
		private List<int> _relativitySourceProvidersList;
		private List<int> _relativityDestinationProvidersList;
		private const int _WORKSPACE_ID = 100111;

		private const int _RELATIVITY_DESTINATION_PROVIDER_ID = 500111;
		private const int _RELATIVITY_SOURCE_PROVIDER_ID = 500222;
		private const int _NON_RELATIVITY_DESTINATION_PROVIDER_ID = 600111;
		private const int _NON_RELATIVITY_SOURCE_PROVIDER_ID = 600222;

		private const int _SYNC_PROFILE_ID = 900111;
		private const int _NON_SYNC_PROFILE_ID = 900222;

		[SetUp]
		public void SetUp()
		{
			_relativityObjectManager = new Mock<IRelativityObjectManager>();
			_objectArtifactIdsQuery = new Mock<IObjectArtifactIdsByStringFieldValueQuery>();

			_query = new IntegrationPointProfilesQuery(
				workspaceId => _relativityObjectManager.Object,
				_objectArtifactIdsQuery.Object);

			_profilesList = new List<IntegrationPointProfile>();
			_relativitySourceProvidersList = new List<int>();
			_relativityDestinationProvidersList = new List<int>();

			_relativityObjectManager
				.Setup(x => x.QueryAsync<IntegrationPointProfile>(
					It.IsAny<QueryRequest>(), false, It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(_profilesList);

			_objectArtifactIdsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(DestinationProvider p) => p.Identifier,
					Constants.IntegrationPoints.DestinationProviders.RELATIVITY))
				.ReturnsAsync(_relativityDestinationProvidersList);

			_objectArtifactIdsQuery
				.Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
					(SourceProvider p) => p.Identifier,
					Constants.IntegrationPoints.SourceProviders.RELATIVITY))
				.ReturnsAsync(_relativitySourceProvidersList);
		}

		[Test]
		public async Task ItShouldReturnAllProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(1, 1);

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
		public async Task ItShouldReturnSyncSourceProviderArtifactId()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int sourceProviderId = await _query.GetSyncSourceProviderArtifactIdAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			sourceProviderId.Should().Be(_RELATIVITY_SOURCE_PROVIDER_ID);
		}

		[Test]
		public async Task ItShouldReturnNonSyncSourceProviderArtifactId()
		{
			// Arrange
			SetUpSyncProviders();

			// Act
			int destinationProviderArtifactId = await _query.GetSyncDestinationProviderArtifactIdAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			destinationProviderArtifactId.Should().Be(_RELATIVITY_DESTINATION_PROVIDER_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfSourceProviders([Values(0, 2)] int relativitySourceProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativitySourceProviderCount);

			// Act
			Func<Task<int>> run = () => _query.GetSyncSourceProviderArtifactIdAsync(_WORKSPACE_ID);

			// Assert
			run.ShouldThrowExactly<InvalidOperationException>();
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfDestinationProviders([Values(0, 2)] int relativityDestinationProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativityDestinationProviderCount: relativityDestinationProviderCount);

			// Act
			Func<Task<int>> run = () => _query.GetSyncDestinationProviderArtifactIdAsync(_WORKSPACE_ID);

			// Assert
			run.ShouldThrowExactly<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldGetOnlySyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(1, 1);

			// Act
			List<int> syncProfiles = (await _query.GetSyncProfilesAsync(_profilesList, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID).ConfigureAwait(false)).ToList();

			// Assert
			syncProfiles.Should().ContainSingle(x => x == _SYNC_PROFILE_ID);
		}

		[Test]
		public async Task ItShouldGetOnlyNonSyncProfiles()
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(1, 1);

			// Act
			List<int> nonSyncProfiles = (await _query.GetNonSyncProfilesAsync(_profilesList, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID).ConfigureAwait(false)).ToList();

			// Assert
			nonSyncProfiles.Should().ContainSingle(x => x == _NON_SYNC_PROFILE_ID);
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

		private void SetUpSyncProviders(int relativitySourceProviderCount = 1, int relativityDestinationProviderCount = 1)
		{
			_relativitySourceProvidersList.AddRange(Enumerable
				.Repeat(_RELATIVITY_SOURCE_PROVIDER_ID, relativitySourceProviderCount));
			_relativityDestinationProvidersList.AddRange(Enumerable
				.Repeat(_RELATIVITY_DESTINATION_PROVIDER_ID, relativityDestinationProviderCount));
		}
	}
}

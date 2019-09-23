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
		public async Task ItShouldQueryAndDivideProfilesCorrectly(
			[Range(0, 2)] int syncProfilesCount, [Range(0, 4)] int nonSyncProfilesCount)
		{
			// Arrange
			SetUpSyncProviders();
			SetUpProfiles(syncProfilesCount, nonSyncProfilesCount);

			// Act
			var (nonSyncProfilesArtifactIds, syncProfilesArtifactIds) = await _query
				.GetSyncAndNonSyncProfilesArtifactIdsAsync(_WORKSPACE_ID)
				.ConfigureAwait(false);

			// Assert
			nonSyncProfilesArtifactIds.ShouldAllBeEquivalentTo(_NON_SYNC_PROFILE_ID);
			syncProfilesArtifactIds.ShouldAllBeEquivalentTo(_SYNC_PROFILE_ID);
		}

		[Test]
		public void ItShouldFailOnWrongNumberOfProviders(
			[Values(0, 2)] int relativitySourceProviderCount, [Values(0, 2)] int relativityDestinationProviderCount)
		{
			// Arrange
			SetUpSyncProviders(relativitySourceProviderCount, relativityDestinationProviderCount);

			// Act
			Func<Task<(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds)>> run =
				() => _query.GetSyncAndNonSyncProfilesArtifactIdsAsync(_WORKSPACE_ID);

			// Assert
			run.ShouldThrowExactly<InvalidOperationException>();
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.DataSourceSnapshotExecutors
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	public sealed class DocumentRetryDataSourceSnapshotExecutorTests : SystemTest
	{
		private WorkspaceRef _workspace;

		private int _savedSearchArtifactId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			const int documentArtifactTypeId = (int) ArtifactType.Document;
			using (IKeywordSearchManager keywordSearchManager = ServiceFactory.CreateProxy<IKeywordSearchManager>())
			{
				KeywordSearch search = new KeywordSearch
				{
					ArtifactTypeID = documentArtifactTypeId,
					Name = "saved search",
					Fields = new List<FieldRef>
					{
						new FieldRef
						{
							Name = "Control Number"
						}
					}
				};
				_savedSearchArtifactId = await keywordSearchManager.CreateSingleAsync(_workspace.ArtifactID, search).ConfigureAwait(false);
			}
		}

		[IdentifiedTest("e1a6bd86-8e32-4900-a810-b0566416e3e5")]
		public async Task ItShouldCreateSnapshot()
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspace.ArtifactID).ConfigureAwait(false);
			int jobHistoryToRetryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspace.ArtifactID).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DataSourceArtifactId = _savedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				SourceWorkspaceArtifactId = _workspace.ArtifactID,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryToRetryId = jobHistoryToRetryArtifactId
			};
			
		
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDocumentRetryDataSourceSnapshotConfiguration>(configuration);
			
			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			configuration.ExportRunId.Should().NotBeEmpty();
			configuration.ExportRunId.Should().NotBe(Guid.Empty);
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
	public sealed class ImageDataSourceSnapshotTests : SystemTest
	{
		private WorkspaceRef _workspace;

		private int _savedSearchArtifactId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			const int documentArtifactTypeId = (int)ArtifactType.Document;
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

		[IdentifiedTest("82d8e1e4-a8b8-43b2-b131-064de29c2117")]
		public async Task ItShouldCreateSnapshot()
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspace.ArtifactID).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DataSourceArtifactId = _savedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				SourceWorkspaceArtifactId = _workspace.ArtifactID,
				JobHistoryArtifactId = jobHistoryArtifactId
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IImageDataSourceSnapshotConfiguration>(configuration);

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			configuration.ExportRunId.Should().NotBeEmpty();
			configuration.ExportRunId.Should().NotBe(Guid.Empty);
		}
	}
}
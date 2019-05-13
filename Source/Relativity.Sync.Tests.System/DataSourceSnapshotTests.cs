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
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class DataSourceSnapshotTests : SystemTest
	{
		private WorkspaceRef _workspace;

		private int _savedSearchArtifactId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

			const int documentArtifactTypeId = 10;
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

		[Test]
		public async Task ItShouldCreateSnapshot()
		{
			ConfigurationStub configuration = new ConfigurationStub
			{
				FieldMappings = new List<FieldMap>(),
				DataSourceArtifactId = _savedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				SourceWorkspaceArtifactId = _workspace.ArtifactID
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDataSourceSnapshotConfiguration>(configuration);

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			configuration.ExportRunId.Should().NotBeEmpty();
			configuration.ExportRunId.Should().NotBe(Guid.Empty);
		}
	}
}
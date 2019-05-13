using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ProgressRdoTests : SystemTest
	{
		private ProgressRepository _repository;

		private int _syncConfigurationArtifactId;

		private int _workspaceId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_repository = new ProgressRepository(new SourceServiceFactoryStub(ServiceFactory));

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _workspaceId).ConfigureAwait(false);
			_syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, _workspaceId, jobHistoryArtifactId).ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldCreateAndReadProgress()
		{
			const string name = "name 1";
			const int order = 5;
			const SyncJobStatus status = SyncJobStatus.New;

			// ACT
			IProgress createdProgress = await _repository.CreateAsync(_workspaceId, _syncConfigurationArtifactId, name, order, status).ConfigureAwait(false);
			IProgress readProgress = await _repository.GetAsync(_workspaceId, createdProgress.ArtifactId).ConfigureAwait(false);

			// ASSERT
			readProgress.Name.Should().Be(name);
			readProgress.Order.Should().Be(order);
			readProgress.Status.Should().Be(status);
		}

		[Test]
		public async Task ItShouldUpdateValues()
		{
			const string name = "name 2";
			const int order = 6;
			const SyncJobStatus status = SyncJobStatus.InProgress;

			const string message = "message";
			Exception exception = new InvalidOperationException();
			const SyncJobStatus newStatus = SyncJobStatus.Cancelled;

			IProgress createdProgress = await _repository.CreateAsync(_workspaceId, _syncConfigurationArtifactId, name, order, status).ConfigureAwait(false);

			// ACT
			await createdProgress.SetMessageAsync(message).ConfigureAwait(false);
			await createdProgress.SetExceptionAsync(exception).ConfigureAwait(false);
			await createdProgress.SetStatusAsync(newStatus).ConfigureAwait(false);

			// ASSERT
			IProgress readProgress = await _repository.GetAsync(_workspaceId, createdProgress.ArtifactId).ConfigureAwait(false);
			readProgress.Message.Should().Be(message);
			readProgress.Exception.Should().Be(exception.ToString());
			readProgress.Status.Should().Be(newStatus);
		}
	}
}
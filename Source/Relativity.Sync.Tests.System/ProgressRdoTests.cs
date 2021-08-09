using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class ProgressRdoTests : SystemTest
	{
		private ProgressRepository _repository;

		private int _syncConfigurationArtifactId;

		private int _workspaceId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_repository = new ProgressRepository(new ServiceFactoryStub(ServiceFactory), new EmptyLogger());

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspaceId).ConfigureAwait(false);
			_syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationRdoAsync(_workspaceId, new ConfigurationStub{JobHistoryArtifactId = jobHistoryArtifactId}).ConfigureAwait(false);
		}

		[IdentifiedTest("48bc9b70-5341-4605-83e9-c96ac301457d")]
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

		[IdentifiedTest("0e58e92f-fd40-45ef-9b4c-fd45f27e7453")]
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
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal class PreValidationExecutorTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;

		private const int _NOT_EXISTING_WORKSPACE = 1;

		[SetUp]
		public async Task SetUp()
		{
			_sourceWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			_destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
		}

		[IdentifiedTest("BAA6FBA3-B03A-4627-9217-873D00ACDA59")]
		public async Task ValdiateAsync_ShouldSuccessfullyValidateJob()
		{
			// Arrange
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPreValidationConfiguration>(configuration);
			
			// Act & Assert
			await syncJob.ExecuteAsync(CancellationToken.None);
		}

		[IdentifiedTest("56039045-E4C8-4C04-8A05-C0E911CCC107")]
		public void ValidateAsync_ShouldMarkJobAsInvalid_WhenDestinationWorkspaceDoesNotExist()
		{
			// Arrange
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				DestinationWorkspaceArtifactId = _NOT_EXISTING_WORKSPACE,
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPreValidationConfiguration>(configuration);

			// Act
			Func<Task> executeJobFunc = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			executeJobFunc.Should().Throw<ValidationException>();
		}
	}
}

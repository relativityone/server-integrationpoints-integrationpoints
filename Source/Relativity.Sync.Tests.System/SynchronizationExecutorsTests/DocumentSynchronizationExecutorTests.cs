﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Tests.System.SynchronizationExecutorsTests;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SynchronizationExecutorsTests
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class DocumentSynchronizationExecutorTests : SystemTest
	{
		private const int _CONTROL_NUMBER_FIELD_ID = 1003667;
		private const string _CONTROL_NUMBER_FIELD_DISPLAY_NAME = "Control Number";

		private string SourceWorkspaceName => $"Source.{Guid.NewGuid()}";
		private string DestinationWorkspaceName => $"Destination.{Guid.NewGuid()}";

		[IdentifiedTestCase("edd705b0-5d9b-42df-a0a0-a801ba0a1b0d", 1000,1)]
		[IdentifiedTestCase("3ad4d2b1-0edb-43d9-9ce2-78ab4e942c4a", 1000,2000)]
		[IdentifiedTestCase("e1fa19e6-4a27-4ba2-bb5c-c924874ccb09", 1000,3500)]
		public async Task ItShouldPassGoldFlow(int batchSize, int totalRecordsCount)
		{
			// Arrange		
			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			Dataset dataSet = Dataset.NativesAndExtractedText;

			SynchronizationExecutorSetup setup = new SynchronizationExecutorSetup(Environment, ServiceFactory)
				.ForWorkspaces(SourceWorkspaceName, DestinationWorkspaceName)
				.ImportData(dataSet, true, true)
				.SetupDocumentConfiguration(fieldMappings, batchSize: batchSize, totalRecordsCount: totalRecordsCount)
				.SetupContainer()
				.ExecuteDocumentPreSynchronizationExecutors();

			// Act
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(setup.Container, setup.Configuration).ConfigureAwait(false);

			// Assert
			var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

			synchronizationValidator.AssertResult(syncResult, ExecutionStatus.Completed);

			synchronizationValidator.AssertTotalTransferredItems(dataSet);
		}

		[IdentifiedTest("0967e7fa-2607-48ba-bfc2-5ab6b786db86")]
		public async Task ItShouldSyncUserField()
		{
			// Arrange
			const int _USER_FIELD_ID = 1039900;
			const string _USER_FIELD_DISPLAY_NAME = "Relativity Sync Test User";

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();
			fieldMappings.Add(new FieldMap
			{
				SourceField = new FieldEntry { DisplayName = _USER_FIELD_DISPLAY_NAME, FieldIdentifier = _USER_FIELD_ID, IsIdentifier = false },
				DestinationField = new FieldEntry { DisplayName = _USER_FIELD_DISPLAY_NAME, FieldIdentifier = _USER_FIELD_ID, IsIdentifier = false }
			});

			SynchronizationExecutorSetup setup = new SynchronizationExecutorSetup(Environment, ServiceFactory)
				.ForWorkspaces(SourceWorkspaceName, DestinationWorkspaceName)
				.ImportData(DataTableFactory.GenerateDocumentWithUserField())
				.SetupDocumentConfiguration(fieldMappings)
				.SetupContainer()
				.ExecuteDocumentPreSynchronizationExecutors();

			// Act
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(setup.Container, setup.Configuration).ConfigureAwait(false);

			// Assert
			var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

			synchronizationValidator.AssertResult(syncResult, ExecutionStatus.Completed);
		}

		[IdentifiedTest("4150991e-5679-4e6f-afdd-a25c1ed4b9af")]
		public async Task ItShouldTagInBatches()
		{
			// Arrange
			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			Dataset dataSet = Dataset.NativesAndExtractedText;

			const int _BATCH_SIZE = 2;

			SynchronizationExecutorSetup setup = new SynchronizationExecutorSetup(Environment, ServiceFactory)
				.ForWorkspaces(SourceWorkspaceName, DestinationWorkspaceName)
				.ImportData(dataSet, true, true)
				.SetupDocumentConfiguration(fieldMappings, batchSize: _BATCH_SIZE)
				.SetupContainer()
				.SetDocumentTracking()
				.ExecuteDocumentPreSynchronizationExecutors();

			// Act
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(setup.Container, setup.Configuration).ConfigureAwait(false);

			// Assert
			var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

			synchronizationValidator.AssertResult(syncResult, ExecutionStatus.Completed);

			synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts);
			synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts);
		}

		[IdentifiedTest("06cacb9a-4d8c-4cd2-8de6-2aa249925eb7")]
		[Ignore("This scenario will be valid when REL-395735 will be resolved.")]
		public async Task ItShouldCompleteWithErrors_WhenSupportedByViewerIsNull()
		{
			// Arrange
			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			Dataset dataSet = Dataset.NativesAndExtractedText;

			SynchronizationExecutorSetup setup = new SynchronizationExecutorSetup(Environment, ServiceFactory)
				.ForWorkspaces(SourceWorkspaceName, DestinationWorkspaceName)
				.ImportData(dataSet, true, true)
				.SetupDocumentConfiguration(fieldMappings)
				.SetupContainer()
				.SetSupportedByViewer()
				.ExecuteDocumentPreSynchronizationExecutors();

			// Act
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(setup.Container, setup.Configuration).ConfigureAwait(false);

			// Assert
			var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

			synchronizationValidator.AssertResult(syncResult, ExecutionStatus.CompletedWithErrors);
		}

		private static List<FieldMap> CreateControlNumberFieldMapping()
		{
			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = _CONTROL_NUMBER_FIELD_DISPLAY_NAME,
						FieldIdentifier = _CONTROL_NUMBER_FIELD_ID,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry
					{
						DisplayName = _CONTROL_NUMBER_FIELD_DISPLAY_NAME,
						FieldIdentifier = _CONTROL_NUMBER_FIELD_ID,
						IsIdentifier = true
					}
				}
			};
		}
		
		private static Task<ExecutionResult> ExecuteSynchronizationExecutorAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<IDocumentSynchronizationConfiguration> syncExecutor = container.Resolve<IExecutor<IDocumentSynchronizationConfiguration>>();
			return syncExecutor.ExecuteAsync(configuration, CancellationToken.None);
		}
	}
}
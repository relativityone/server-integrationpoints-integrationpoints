using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SourceWorkspaceTagsCreationStepTests : FailingStepsBase<ISourceWorkspaceTagsCreationConfiguration>
	{
		private IExecutor<ISourceWorkspaceTagsCreationConfiguration> _executor;
		private Mock<IObjectManager> _destinationObjectManagerMock;
		private Mock<IObjectManager> _sourceObjectManagerMock;
		private Mock<IWorkspaceManager> _workspaceManagerMock;

		private static readonly Guid _DESTINATION_INSTANCE_ARTIFACT_ID_FIELD_GUID = Guid.Parse("323458DB-8A06-464B-9402-AF2516CF47E0");
		private static readonly Guid _DESTINATION_INSTANCE_NAME_FIELD_GUID = Guid.Parse("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
		private static readonly Guid _DESTINATION_WORKSPACE_ARTIFACT_ID_FIELD_GUID = Guid.Parse("207E6836-2961-466B-A0D2-29974A4FAD36");
		private static readonly Guid _DESTINATION_WORKSPACE_NAME_FIELD_GUID = Guid.Parse("348D7394-2658-4DA4-87D0-8183824ADF98");

		private static readonly Guid _NAME_FIELD_GUID = Guid.Parse("155649c0-db15-4ee7-b449-bfdf2a54b7b5");

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(IDestinationWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IPreValidationConfiguration),
			typeof(IValidationConfiguration),
			typeof(IPermissionsCheckConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IDataSourceSnapshotConfiguration),
			typeof(IDestinationWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(IDocumentJobStartMetricsConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(IJobCleanupConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};

		[SetUp]
		public void MySetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<ISourceWorkspaceTagsCreationConfiguration>(containerBuilder);
			
			_workspaceManagerMock = new Mock<IWorkspaceManager>();

			_sourceObjectManagerMock = new Mock<IObjectManager>();
			var sourceServiceFactoryMock = new Mock<ISourceServiceFactoryForUser>();
			sourceServiceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_sourceObjectManagerMock.Object));
			sourceServiceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).Returns(Task.FromResult(_workspaceManagerMock.Object));
			
			_destinationObjectManagerMock = new Mock<IObjectManager>();
			var destinationServiceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			destinationServiceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_destinationObjectManagerMock.Object));
			destinationServiceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).Returns(Task.FromResult(_workspaceManagerMock.Object));


			containerBuilder.RegisterInstance(sourceServiceFactoryMock.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(destinationServiceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();

			containerBuilder.RegisterInstance(new EmptyLogger()).As<IAPILog>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
		}

#pragma warning disable S1135 // Track uses of "TODO" tags
		// TODO REL-304544: Write integration tests to ensure we are querying for/setting NULL for DestinationInstanceArtifactID when it's -1
#pragma warning restore S1135 // Track uses of "TODO" tags

		[Test]
		public void ItCreatesTagIfItDoesNotExist()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014854;
			const int jobArtifactId = 101000;
			const int newDestinationWorkspaceTagArtifactId = 1031000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
				).Returns(Task.FromResult(new QueryResult()));

			_sourceObjectManagerMock.Setup(x => x.CreateAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<CreateRequest>())
				).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newDestinationWorkspaceTagArtifactId } })
				).Verifiable();

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_sourceObjectManagerMock.Verify();
			_destinationObjectManagerMock.Verify();
			Assert.AreEqual(newDestinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItUpdatesIncorrectDestinationWorkspaceNameOnExistingTag()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const int instanceArtifactId = 1013283;
			const string destinationWorkspaceName = "Cool Workspace";
			const string expectedDestinationWorkspaceName = "Foo Bar Baz";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = expectedDestinationWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject()
					{
						ArtifactID = destinationWorkspaceTagArtifactId,
						FieldValues = BuildFieldValuePairs(new Dictionary<Guid, object>
						{
							{ _NAME_FIELD_GUID, "kjdsfhkjsdhfjksdn" },
							{ _DESTINATION_WORKSPACE_NAME_FIELD_GUID, destinationWorkspaceName },
							{ _DESTINATION_INSTANCE_NAME_FIELD_GUID, "This Instance" },
							{ _DESTINATION_INSTANCE_ARTIFACT_ID_FIELD_GUID, instanceArtifactId },
							{ _DESTINATION_WORKSPACE_ARTIFACT_ID_FIELD_GUID, destinationWorkspaceArtifactID }
						})
					}
				}
			}));

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y =>
					y.FieldValues.Any(fv => fv.Field.Guid == _DESTINATION_INSTANCE_NAME_FIELD_GUID) &&
					y.FieldValues.First(fv => fv.Field.Guid == _DESTINATION_WORKSPACE_NAME_FIELD_GUID).Value.Equals(expectedDestinationWorkspaceName)))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_sourceObjectManagerMock.Verify();
			_destinationObjectManagerMock.Verify();
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItUpdatesIncorrectDestinationInstanceNameOnExistingTag()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const int instanceArtifactId = 1013283;
			const string destinationWorkspaceName = "Cool Workspace";
			const string expectedDestinationInstanceName = "This Instance";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
				).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject()
					{
						ArtifactID = destinationWorkspaceTagArtifactId,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { _DESTINATION_WORKSPACE_NAME_FIELD_GUID } },
								Value = destinationWorkspaceName
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { _DESTINATION_INSTANCE_NAME_FIELD_GUID } },
								Value = "Some Other Weird Instance"
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { _DESTINATION_INSTANCE_ARTIFACT_ID_FIELD_GUID } },
								Value = instanceArtifactId
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { _DESTINATION_WORKSPACE_ARTIFACT_ID_FIELD_GUID } },
								Value = destinationWorkspaceArtifactID
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { _NAME_FIELD_GUID } },
								Value = "kjdsfhkjsdhfjksdn"
							}
						}
					}
				}
			}
					));

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y =>
					y.FieldValues.Any(fv => fv.Field.Guid == _DESTINATION_INSTANCE_NAME_FIELD_GUID) &&
					y.FieldValues.First(fv => fv.Field.Guid == _DESTINATION_INSTANCE_NAME_FIELD_GUID).Value.Equals(expectedDestinationInstanceName)))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_sourceObjectManagerMock.Verify();
			_destinationObjectManagerMock.Verify();
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItDoesNotUpdateCorrectExistingTag()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const int instanceArtifactId = 1013283;
			const string destinationWorkspaceName = "Cool Workspace";
			const string destinationInstanceName = "This Instance";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});


			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject()
					{
						ArtifactID = destinationWorkspaceTagArtifactId,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("348d7394-2658-4da4-87d0-8183824adf98") } },
								Value = destinationWorkspaceName
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554") } },
								Value = destinationInstanceName
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("323458db-8a06-464b-9402-af2516cf47e0") } },
								Value = instanceArtifactId
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("207e6836-2961-466b-a0d2-29974a4fad36") } },
								Value = destinationWorkspaceArtifactID
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5") } },
								Value = "kjdsfhkjsdhfjksdn"
							}
						}
					}
				}
			}
					));

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_destinationObjectManagerMock.Verify();
			_sourceObjectManagerMock.Verify();
			_sourceObjectManagerMock.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactID, It.Is<UpdateRequest>(y => y.Object.ArtifactID == destinationWorkspaceTagArtifactId)), Times.Never);
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public async Task ItReturnsFailedResultIfDestinationWorkspaceDoesNotExist()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync((WorkspaceResponse)null);


			// Act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncException>(result.Exception);
			Assert.IsTrue(result.Exception.Message.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture)));
		}

		[Test]
		public async Task ItReturnsFailedResultIfTagCreationThrows()
		{
			// Arrange
			const int srcWorkspaceArtifactId = 1014853;
			const int destWorkspaceArtifactId = 1015853;
			const int jobArtifactId = 101000;
			const string destWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = srcWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destWorkspaceArtifactId,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destWorkspaceArtifactId))
				.ReturnsAsync(new WorkspaceResponse {Name = destWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				srcWorkspaceArtifactId,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
				).Returns(Task.FromResult(new QueryResult()));

			_sourceObjectManagerMock.Setup(x => x.CreateAsync(
				srcWorkspaceArtifactId,
				It.IsAny<CreateRequest>())
				).Throws<Exception>();

			// Act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
			Assert.AreEqual(
				$"Failed to create {nameof(DestinationWorkspaceTag)} in workspace {srcWorkspaceArtifactId}",
				result.Exception.Message);
		}

		[Test]
		public async Task ItReturnsFailedResultIfTagUpdateThrows()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const int instanceArtifactId = 1013283;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject()
					{
						ArtifactID = destinationWorkspaceTagArtifactId,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("348d7394-2658-4da4-87d0-8183824adf98") } },
								Value = "Some Other Workspace"
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554") } },
								Value = "This Instance"
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("323458db-8a06-464b-9402-af2516cf47e0") } },
								Value = instanceArtifactId
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("207e6836-2961-466b-a0d2-29974a4fad36") } },
								Value = destinationWorkspaceArtifactID
							},
							new FieldValuePair
							{
								Field = new Field { Guids = new List<Guid> { new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5") } },
								Value = "kjdsfhkjsdhfjksdn"
							}
						}
					}
				}
			}));

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y => y.Object.ArtifactID == destinationWorkspaceTagArtifactId))
			).Throws<Exception>();

			// Act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
			Assert.AreEqual(
				$"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTagArtifactId} in workspace {sourceWorkspaceArtifactID}",
				result.Exception.Message);
		}

		[Test]
		public async Task ItReturnsFailedResultIfTagLinkingThrows()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});

			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
				).Returns(Task.FromResult(new QueryResult()));

			_sourceObjectManagerMock.Setup(x => x.CreateAsync(
					It.IsAny<int>(),
					It.IsAny<CreateRequest>()
				)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = destinationWorkspaceTagArtifactId } }));

			_sourceObjectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y => y.Object.ArtifactID == jobArtifactId))
			).Throws<Exception>();

			// Act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<DestinationWorkspaceTagsLinkerException>(result.Exception);
			Assert.AreEqual(
				$"Failed to link {nameof(DestinationWorkspaceTag)} to Job History",
				result.Exception.Message);
		}

		[Test]
		public async Task ItReturnsFailedResultIfTagQueryThrows()
		{
			// Arrange
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobHistoryArtifactId = jobArtifactId
			};

			_workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactID))
				.ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});


			_sourceObjectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Throws<Exception>();

			// Act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
			Assert.AreEqual(
				$"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactID}.",
				result.Exception.Message);
		}

		private List<FieldValuePair> BuildFieldValuePairs(Dictionary<Guid, object> fieldValues)
		{
			return fieldValues
				.Select(kv => new FieldValuePair { Field = new Field { Guids = new List<Guid> { kv.Key } }, Value = kv.Value })
				.ToList();
		}
	}
}

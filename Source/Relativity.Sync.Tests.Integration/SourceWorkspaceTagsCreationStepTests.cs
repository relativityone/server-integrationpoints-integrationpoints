using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.Sync.Tests.Integration.Stubs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Moq;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SourceWorkspaceTagsCreationStepTests : FailingStepsBase<ISourceWorkspaceTagsCreationConfiguration>
	{
		private IExecutor<ISourceWorkspaceTagsCreationConfiguration> _executor;
		private Mock<IObjectManager> _objectManagerMock;
		private Mock<ISourceServiceFactoryForUser> _serviceFactoryMock;

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(IDestinationWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, destination workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 7;
			return expectedNumberOfExecutedSteps;
		}

		[SetUp]
		public void MySetUp()
		{
			// The setup will be changed once the new Containerbuilder changes are merged in.
			ContainerBuilder containerBuilder = IntegrationTestsContainerBuilder.CreateContainerBuilder(new List<Type>());
			List<IInstaller> installers = Assembly.GetAssembly(typeof(IInstaller))
				.GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IInstaller>())
				.Select(t => (IInstaller)Activator.CreateInstance(t))
				.ToList();
			installers.Add(new OutsideDependenciesStubInstaller());
			foreach (IInstaller installer in installers)
			{
				installer.Install(containerBuilder);
			}

			_objectManagerMock = new Mock<IObjectManager>();
			_serviceFactoryMock = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

			containerBuilder.RegisterInstance(_serviceFactoryMock.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			Mock<ISyncLog> loggerMock = new Mock<ISyncLog>();
			containerBuilder.RegisterInstance(loggerMock.Object).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
		}

		[Test]
		public void ItCreatesTagIfItDoesntExist()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int newDestinationWorkspaceTagArtifactId = 1031000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
				).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<CreateRequest>())
				).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newDestinationWorkspaceTagArtifactId } })
				).Verifiable();

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(newDestinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItUpdatesIncorrectDestinationWorkspaceNameOnExistingTag()
		{
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
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = expectedDestinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
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
			}
					));

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y =>
					y.FieldValues.First(fv => fv.Field.Guid == new Guid("348d7394-2658-4da4-87d0-8183824adf98")).Value.Equals(expectedDestinationWorkspaceName)))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItUpdatesIncorrectDestinationInstanceNameOnExistingTag()
		{
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
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = expectedDestinationInstanceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
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
								Value = "Some Other Weird Instance"
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

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y =>
					y.FieldValues.First(fv => fv.Field.Guid == new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554")).Value.Equals(expectedDestinationInstanceName)))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItDoesNotUpdateCorrectExistingTag()
		{
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
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
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

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == jobArtifactId))
				).Returns(Task.FromResult(new UpdateResult())
				).Verifiable();

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			_objectManagerMock.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactID, It.Is<UpdateRequest>(y => y.Object.ArtifactID == destinationWorkspaceTagArtifactId)), Times.Never);
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItThrowsProperExceptionIfDestinationWorkspaceDoesNotExist()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult()));

			SyncException thrownException = Assert.Throws<SyncException>(() => _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult());
			Assert.IsTrue(thrownException.Message.Contains(destinationWorkspaceArtifactID.ToString(CultureInfo.InvariantCulture)));
		}

		[Test]
		public void ItThrowsProperExceptionIfTagCreationThrows()
		{
			const int srcWorkspaceArtifactId = 1014853;
			const int destWorkspaceArtifactId = 1015853;
			const int jobArtifactId = 101000;
			const string destWorkspaceName = "Cool Workspace";
			const string destInstanceName = "This Instance";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = srcWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destWorkspaceArtifactId,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				srcWorkspaceArtifactId,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
				).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				srcWorkspaceArtifactId,
				It.IsAny<CreateRequest>())
				).Throws(new Exception("Blech blooch blar"));

			DestinationWorkspaceTagRepositoryException thrownException = Assert.Throws<DestinationWorkspaceTagRepositoryException>(() =>
				_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult());
			Assert.AreEqual(
				$"Failed to create {nameof(DestinationWorkspaceTag)} '{destInstanceName} - {destWorkspaceName} - {destWorkspaceArtifactId}' in workspace {srcWorkspaceArtifactId}",
				thrownException.Message);
		}

		[Test]
		public void ItThrowsProperExceptionIfTagUpdateThrows()
		{
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
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
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
			}
					));

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y => y.Object.ArtifactID == destinationWorkspaceTagArtifactId))
				).Throws(new Exception("blah blah blah"));

			DestinationWorkspaceTagRepositoryException thrownException = Assert.Throws<DestinationWorkspaceTagRepositoryException>(() =>
				_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult());
			Assert.AreEqual(
				$"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTagArtifactId} in workspace {sourceWorkspaceArtifactID}",
				thrownException.Message);
		}

		[Test]
		public void ItThrowsProperExceptionIfTagLinkingThrows()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const int destinationWorkspaceTagArtifactId = 1031000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>())
				).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
					It.IsAny<int>(),
					It.IsAny<CreateRequest>()
				)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = destinationWorkspaceTagArtifactId } }));

			_objectManagerMock.Setup(x => x.UpdateAsync(
				sourceWorkspaceArtifactID,
				It.Is<UpdateRequest>(y => y.Object.ArtifactID == jobArtifactId))
				).Throws(new Exception("blah blah blah"));

			DestinationWorkspaceTagsLinkerException thrownException = Assert.Throws<DestinationWorkspaceTagsLinkerException>(() =>
				_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult());
			Assert.AreEqual(
				$"Failed to link {nameof(DestinationWorkspaceTag)} to Job History",
				thrownException.Message);
		}

		[Test]
		public void ItThrowsProperExceptionIfTagQueryThrows()
		{
			const int sourceWorkspaceArtifactID = 1014853;
			const int destinationWorkspaceArtifactID = 1014853;
			const int jobArtifactId = 101000;
			const string destinationWorkspaceName = "Cool Workspace";
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactID,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactID,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>()))
				.Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactID,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>()
				)).Throws(new Exception("blah blah blah"));

			DestinationWorkspaceTagRepositoryException thrownException = Assert.Throws<DestinationWorkspaceTagRepositoryException>(() =>
				_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult());
			Assert.AreEqual(
				$"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactID}",
				thrownException.Message);
		}
	}
}
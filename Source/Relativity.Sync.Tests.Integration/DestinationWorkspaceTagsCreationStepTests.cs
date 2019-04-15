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
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationStepTests : FailingStepsBase<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private IExecutor<IDestinationWorkspaceTagsCreationConfiguration> _executor;
		private Mock<IObjectManager> _objectManagerMock;

		private static readonly Guid SourceCaseTagObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid SourceJobTagObjectType = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		private static readonly Guid CaseIdFieldGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid SourceWorkspaceNameFieldGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(ISourceWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot, source workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 7;
			return expectedNumberOfExecutedSteps;
		}

		[SetUp]
		public void MySetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceTagsCreationConfiguration>(containerBuilder);

			_objectManagerMock = new Mock<IObjectManager>();
			var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			var serviceFactoryMock2 = new Mock<ISourceServiceFactoryForUser>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
			serviceFactoryMock2.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(serviceFactoryMock2.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			containerBuilder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
		}

		[Test]
		public void ItShouldCreateSourceCaseTagIfItDoesNotExist()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int newSourceWorkspaceTagArtifactId = 103000;
			const int newSourceJobTagArtifactId = 103000;
			string sourceWorkspaceName = "Cool Workspace";
			string destinationWorkspaceName = "Even Cooler Workspace";
			string jobHistoryName = "Cool Job";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId

			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
			).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify();
			Assert.AreEqual(newSourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}

		[Test]
		public void ItShouldUpdateSourceCaseTagIfItDoesExist()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int sourceWorkspaceTagArtifactId = 103000;
			const int newSourceJobTagArtifactId = 103001;
			string oldSourceWorkspaceName = "Not Cool Workspace";
			string newSourceWorkspaceName = "Cool Workspace";
			string destinationWorkspaceName = "Even Cooler Workspace";
			string jobHistoryName = "Cool Job";
			string oldSourceWorkspaceTagName = "Super tag";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId

			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = newSourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				ObjectType = new ObjectType
				{
					Guids = new List<Guid>() { SourceCaseTagObjectTypeGuid }
				},
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = oldSourceWorkspaceTagName,
						ArtifactID = sourceWorkspaceTagArtifactId,
						FieldValues = new List<FieldValuePair>()
						{
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { CaseIdFieldGuid }},
								Value = sourceWorkspaceArtifactId
							},
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { SourceWorkspaceNameFieldGuid }},
								Value = oldSourceWorkspaceName
							},
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { InstanceNameFieldGuid }},
								Value = "This Instance"
							}
						}
					}
				}
			}));

			_objectManagerMock.Setup(x => x.UpdateAsync(
				destinationWorkspaceArtifactId,
				It.Is<UpdateRequest>(y => VerifyUpdateRequest(y, sourceWorkspaceTagArtifactId,
					pair => pair.Field.Guid.Equals(CaseIdFieldGuid) && pair.Value.Equals(sourceWorkspaceArtifactId),
					pair => pair.Field.Guid.Equals(SourceWorkspaceNameFieldGuid) && pair.Value.Equals(newSourceWorkspaceName)))
				)).Returns(Task.FromResult(new UpdateResult())).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify(
				x => x.CreateAsync(destinationWorkspaceArtifactId,
					It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid))), Times.Never);

			_objectManagerMock.Verify();
			Assert.AreEqual(sourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}

		[Test]
		public void ItShouldNotUpdateSourceCaseTagIfDoesNotNeedTo()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int sourceWorkspaceTagArtifactId = 103000;
			const int sourceJobTagArtifactId = 103001;
			string sourceWorkspaceName = "Not Cool Workspace";
			string destinationWorkspaceName = "Even Cooler Workspace";
			string jobHistoryName = "Cool Job";
			string sourceWorkspaceTagName = "Super tag";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId

			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult { Objects = new List<RelativityObject> { new RelativityObject() { Name = destinationWorkspaceName } } }));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				ObjectType = new ObjectType
				{
					Guids = new List<Guid>() { SourceCaseTagObjectTypeGuid }
				},
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = sourceWorkspaceTagName,
						ArtifactID = sourceWorkspaceTagArtifactId,
						FieldValues = new List<FieldValuePair>()
						{
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { CaseIdFieldGuid }},
								Value = sourceWorkspaceArtifactId
							},
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { SourceWorkspaceNameFieldGuid }},
								Value = sourceWorkspaceName
							},
							new FieldValuePair()
							{
								Field = new Field() {Guids = new List<Guid> { InstanceNameFieldGuid }},
								Value = "This Instance"
							}
						}
					}
				}
			}));

			_objectManagerMock.Verify(x => x.UpdateAsync(
				destinationWorkspaceArtifactId,
				It.Is<UpdateRequest>(y => VerifyUpdateRequest(y, sourceWorkspaceTagArtifactId))
				), Times.Never);

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = sourceJobTagArtifactId } }));

			// Act
			_executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			_objectManagerMock.Verify(
				x => x.CreateAsync(destinationWorkspaceArtifactId,
					It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid))), Times.Never);

			_objectManagerMock.Verify();
			Assert.AreEqual(sourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreEqual(sourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}

		private static bool VerifyUpdateRequest(UpdateRequest request, int tagArtifactId, params Predicate<FieldRefValuePair>[] predicates)
		{
			List<FieldRefValuePair> fields = request.FieldValues.ToList();

			return request.Object.ArtifactID == tagArtifactId && predicates.All(fields.Exists);
		}

		[Test]
		public async Task ItReturnsFailedResultIfSourceWorkspaceNameQueryThrows()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Throws<ServiceException>();

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<ServiceException>(result.Exception);
		}

		[Test]
		public async Task ItReturnsFailedResultIfSourceWorkspaceTagQueryThrows()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			string sourceWorkspaceName = "Cool Workspace";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Throws<ServiceException>();

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<RelativitySourceCaseTagRepositoryException>(result.Exception);
			Assert.IsNotNull(result.Exception.InnerException);
			Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
		}

		[Test]
		public async Task ItReturnsFailedResultIfSourceWorkspaceTagCreationThrows()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			string sourceWorkspaceName = "Cool Workspace";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Throws<ServiceException>();

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<RelativitySourceCaseTagRepositoryException>(result.Exception);
			Assert.IsNotNull(result.Exception.InnerException);
			Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
		}

		[Test]
		public async Task ItReturnsFailedResultIfJobHistoryQueryThrows()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int newSourceWorkspaceTagArtifactId = 103000;
			string sourceWorkspaceName = "Cool Workspace";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
			).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Throws<ServiceException>();

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<ServiceException>(result.Exception);
		}

		[Test]
		public async Task ItReturnsFailedResultIfSourceJobTagCreationThrows()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014854;
			const int jobArtifactId = 101000;
			const int sourceWorkspaceArtifactTypeId = 102001;
			const int sourceJobArtifactTypeId = 102002;
			const int newSourceWorkspaceTagArtifactId = 103000;
			string sourceWorkspaceName = "Cool Workspace";
			string jobHistoryName = "Cool Job";

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId,
				SourceWorkspaceArtifactTypeId = sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = sourceJobArtifactTypeId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = sourceWorkspaceName } }
			}));

			_objectManagerMock.Setup(x => x.QueryAsync(
				destinationWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult()));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
			).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
			).Verifiable();

			_objectManagerMock.Setup(x => x.QueryAsync(
				sourceWorkspaceArtifactId,
				It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
				0,
				1,
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>()
			)).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

			_objectManagerMock.Setup(x => x.CreateAsync(
				destinationWorkspaceArtifactId,
				It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
			)).Throws<ServiceException>();

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<RelativitySourceJobTagRepositoryException>(result.Exception);
			Assert.IsNotNull(result.Exception.InnerException);
			Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
		}

		[Test]
		public async Task ItReturnsFailedResultIfDestinationWorkspaceDoesNotExist()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014853;
			const int jobArtifactId = 101000;
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
					-1,
					It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
					It.IsAny<int>(),
					It.IsAny<int>(),
					CancellationToken.None,
					It.IsAny<IProgress<ProgressReport>>()))
					.Returns(Task.FromResult(new QueryResult()));

			// ACT
			ExecutionResult result =
				await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncException>(result.Exception);
			Assert.IsTrue(result.Exception.Message.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture)));
		}

		[Test]
		public async Task ItReturnsFailedResultIfSourceWorkspaceDoesNotExist()
		{
			const int sourceWorkspaceArtifactId = 1014853;
			const int destinationWorkspaceArtifactId = 1014853;
			const int jobArtifactId = 101000;
			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				JobArtifactId = jobArtifactId
			};

			_objectManagerMock.Setup(x => x.QueryAsync(
					-1,
					It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
					It.IsAny<int>(),
					It.IsAny<int>(),
					CancellationToken.None,
					It.IsAny<IProgress<ProgressReport>>()))
					.Returns(Task.FromResult(new QueryResult()));

			// ACT
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<SyncException>(result.Exception);
			Assert.IsTrue(result.Exception.Message.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.System
{
	public class JobHistoryErrorRepositoryTests : SystemTest
	{
		private WorkspaceRef _workspace;
		private ServicesManagerStub _servicesMgr;
		private DynamicProxyFactoryStub _dynamicProxyFactoryStub;
		private ISourceServiceFactoryForAdmin _sourceServiceFactoryForAdmin;

		private readonly Guid _jobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private readonly Guid _errorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private readonly Guid _errorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
		private readonly Guid _errorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
		private readonly Guid _sourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
		private readonly Guid _stackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");

		[SetUp]
		public async Task SetUp()
		{
			_workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_servicesMgr = new ServicesManagerStub();
			_dynamicProxyFactoryStub = new DynamicProxyFactoryStub();
			_sourceServiceFactoryForAdmin = new ServiceFactoryForAdmin(_servicesMgr, _dynamicProxyFactoryStub);
		}

		[Test]
		public async Task ItShouldCreateJobHistoryError()
		{
			// Arrange
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _workspace.ArtifactID, "Totally unique job history name").ConfigureAwait(false);
			ErrorType expectedErrorType = ErrorType.Item;
			ErrorStatus expErrorStatus = ErrorStatus.New;
			string expectedErrorMessage = "Mayday, mayday";
			string expectedSourceUniqueId = "Totally unique Id";
			string expectedStackTrace = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

			CreateJobHistoryErrorDto createDto = new CreateJobHistoryErrorDto(expectedJobHistoryArtifactId, expectedErrorType)
			{
				ErrorMessage = expectedErrorMessage,
				SourceUniqueId = expectedSourceUniqueId,
				StackTrace = expectedStackTrace,
			};
			
			JobHistoryErrorRepository instance = new JobHistoryErrorRepository(_sourceServiceFactoryForAdmin);

			// Act
			IJobHistoryError createResult = await instance.CreateAsync(_workspace.ArtifactID, createDto).ConfigureAwait(false);

			// Assert
			RelativityObject error = await QueryForCreatedJobHistoryError(createResult.ArtifactId).ConfigureAwait(false);
			error[_errorMessageField].Value.Should().Be(expectedErrorMessage);
			error[_stackTraceField].Value.Should().Be(expectedStackTrace);
			error[_sourceUniqueIdField].Value.Should().Be(expectedSourceUniqueId);
			error[_errorStatusField].Value.As<Choice>().Name.Should().Be(expErrorStatus.ToString());
			error[_errorTypeField].Value.As<Choice>().Name.Should().Be(expectedErrorType.ToString());
			error.ParentObject.ArtifactID.Should().Be(expectedJobHistoryArtifactId);
			error.Name.Should().Be(createResult.Name);
		}

		private async Task<RelativityObject> QueryForCreatedJobHistoryError(int jobHistoryErrorArtifactId)
		{
			IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>();

			QueryRequest queryRequest = new QueryRequest
			{
				Condition = $"\"ArtifactId\"=={jobHistoryErrorArtifactId}",
				ObjectType = new ObjectTypeRef {Guid = _jobHistoryErrorObject},
				IncludeNameInQueryResult = true,
				Fields = new List<FieldRef>
				{
					new FieldRef{Guid = _sourceUniqueIdField},
					new FieldRef{Guid = _errorMessageField},
					new FieldRef{Guid = _errorTypeField},
					new FieldRef{Guid = _stackTraceField},
					new FieldRef{Guid = _errorStatusField},
				}
			};

			QueryResult queryResult = await objectManager.QueryAsync(_workspace.ArtifactID, queryRequest, 1, 1).ConfigureAwait(false);
			return queryResult.Objects.First();
		}
	}
}

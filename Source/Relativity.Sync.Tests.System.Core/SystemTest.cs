using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core.Helpers;
using QueryResult = kCura.Relativity.Client.QueryResult;
using User = Relativity.Services.User.User;

namespace Relativity.Sync.Tests.System.Core
{
	public abstract class SystemTest : IDisposable
	{
#pragma warning disable CS0618 // Type or member is obsolete
		protected readonly int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;


		protected IRSAPIClient Client { get; private set; }
#pragma warning restore CS0618 // Type or member is obsolete
		protected ServiceFactory ServiceFactory { get; private set; }
		protected TestEnvironment Environment { get; private set; }

		protected User User { get; private set; }

		protected ISyncLog Logger { get; private set; }

		[OneTimeSetUp]
		public async Task SuiteSetup()
		{
			Client = new RSAPIClient(AppSettings.RsapiServicesUrl, new kCura.Relativity.Client.UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
			ServiceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
			User = await Rdos.GetUserAsync(ServiceFactory, 0).ConfigureAwait(false);
			Environment = new TestEnvironment();
			Logger = TestLogHelper.GetLogger();

			Logger.LogInformation("Invoking ChildSuiteSetup");
			await ChildSuiteSetup().ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task SuiteTeardown()
		{
			ChildSuiteTeardown();

			await Environment.DoCleanupAsync().ConfigureAwait(false);
			Client?.Dispose();
			Client = null;
		}

		protected virtual Task ChildSuiteSetup()
		{
			return Task.CompletedTask;
		}

		protected virtual void ChildSuiteTeardown()
		{
		}

		protected async Task<IEnumerable<FieldMap>> GetIdentifierMappingAsync(int sourceWorkspaceId, int targetWorkspaceId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest query = PrepareIdentifierFieldsQueryRequest();
				Services.Objects.DataContracts.QueryResult sourceQueryResult = await objectManager.QueryAsync(sourceWorkspaceId, query, 0, 1).ConfigureAwait(false);
				Services.Objects.DataContracts.QueryResult destinationQueryResult = await objectManager.QueryAsync(targetWorkspaceId, query, 0, 1).ConfigureAwait(false);

				return new FieldMap[]
				{
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = sourceQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  sourceQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						DestinationField = new FieldEntry
						{
							DisplayName = destinationQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  destinationQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						FieldMapType = FieldMapType.Identifier
					}
				};

			}
		}
		private QueryRequest PrepareIdentifierFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} and 'Is Identifier' == true",
				Fields = new[] { new FieldRef { Name = "Name" } },
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}

		protected virtual void Dispose(bool disposing)
		{
			Client?.Dispose();
			Environment?.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

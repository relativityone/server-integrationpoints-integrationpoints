using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class Progress : IProgress
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly int _syncConfigurationArtifactId;
		private readonly int _workspaceArtifactId;

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid NameGuid = new Guid("AE2FCA2B-0E5C-4F35-948F-6C1654D5CF95");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");
		private static readonly Guid ParentArtifactGuid = new Guid("E0188DD7-4B1B-454D-AFA4-3CCC7F9DC001");

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			_serviceFactory = serviceFactory;
			_logger = logger;
			_workspaceArtifactId = workspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;

			Name = name;
			Order = order;
			Status = status;
		}

		public Progress(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int artifactId)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
			_workspaceArtifactId = workspaceArtifactId;

			ArtifactId = artifactId;
		}

		public Progress(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
			_workspaceArtifactId = workspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;

			Name = name;
		}

		public int ArtifactId { get; private set; }

		public string Name { get; private set; }

		public int Order { get; private set; }

		public SyncJobStatus Status { get; private set; }

		public async Task SetStatusAsync(SyncJobStatus status)
		{
			string description = status.GetDescription();
			await UpdateFieldValue(StatusGuid, description).ConfigureAwait(false);
			Status = status;
		}

		public string Exception { get; private set; }

		public async Task SetExceptionAsync(Exception exception)
		{
			string exceptionString = exception?.ToString();
			await UpdateFieldValue(ExceptionGuid, exceptionString).ConfigureAwait(false);
			Exception = exceptionString;
		}

		public string Message { get; private set; }

		public async Task SetMessageAsync(string message)
		{
			await UpdateFieldValue(MessageGuid, message).ConfigureAwait(false);
			Message = message;
		}

		private async Task CreateAsync()
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = ProgressObjectTypeGuid
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = _syncConfigurationArtifactId
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = NameGuid
							},
							Value = Name
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = OrderGuid
							},
							Value = Order
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = StatusGuid
							},
							Value = Status.GetDescription()
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(_workspaceArtifactId, request).ConfigureAwait(false);

				ArtifactId = result.Object.ArtifactID;
			}
		}

		private async Task ReadAsync()
		{
			var request = new ReadRequest
			{
				Object = new RelativityObjectRef
				{
					ArtifactID = ArtifactId
				},
				Fields = GetFieldRefsForQuery()
			};

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				ReadResult result = await objectManager.ReadAsync(_workspaceArtifactId, request).ConfigureAwait(false);
				PopulateProgressProperties(result.Object);
			}
		}

		private async Task<bool> QueryAsync()
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = ProgressObjectTypeGuid
				},
				Condition = $"'{NameGuid}' == '{Name}' AND '{ParentArtifactGuid}' == {_syncConfigurationArtifactId}",
				Fields = GetFieldRefsForQuery()
			};

			bool objectExists = false;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, request, 1, 1).ConfigureAwait(false);
				if (result.Objects.Count > 0)
				{
					PopulateProgressProperties(result.Objects[0]);
					objectExists = true;
				}
			}
			return objectExists;
		}
		private async Task<IReadOnlyCollection<IProgress>> QueryAllAsync()
		{
			var progresses = new ConcurrentBag<IProgress>();
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = ProgressObjectTypeGuid
				},
				Condition = $"'{ParentArtifactGuid}' == OBJECT {_syncConfigurationArtifactId}",
				IncludeNameInQueryResult = true
			};
			try
			{
				using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, 1, int.MaxValue).ConfigureAwait(false);
					if (result.TotalCount > 0)
					{
						IEnumerable<int> progressIds = result.Objects.Select(x => x.ArtifactID);

						Parallel.ForEach(progressIds, progressArtifactId =>
						{
							var progress = new Progress(_serviceFactory, _logger, _workspaceArtifactId, progressArtifactId);
							progress.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
							progresses.Add(progress);
						});
					}
				}
			}
			catch (Exception progressQueryAllException)
			{
				_logger.LogError(progressQueryAllException, "Failed to retrieve all progress information for workspace {WorkspaceArtifactID} and sync configuration object {SyncConfigArtifactID}.",
					_workspaceArtifactId, _syncConfigurationArtifactId);
			}
			return progresses;
		}

		private static IEnumerable<FieldRef> GetFieldRefsForQuery()
		{
			IEnumerable<FieldRef> fields = new[]
			{
				new FieldRef
				{
					Guid = NameGuid
				},
				new FieldRef
				{
					Guid = OrderGuid
				},
				new FieldRef
				{
					Guid = StatusGuid
				},
				new FieldRef
				{
					Guid = ExceptionGuid
				},
				new FieldRef
				{
					Guid = MessageGuid
				}
			};
			return fields;
		}

		private void PopulateProgressProperties(RelativityObject relativityObject)
		{
			ArtifactId = relativityObject.ArtifactID;
			Name = (string)relativityObject[NameGuid].Value;
			Order = (int)relativityObject[OrderGuid].Value;
			Status = ((string)relativityObject[StatusGuid].Value).GetEnumFromDescription<SyncJobStatus>();
			Exception = (string)relativityObject[ExceptionGuid].Value;
			Message = (string)relativityObject[MessageGuid].Value;
		}

		private async Task UpdateFieldValue<T>(Guid fieldGuid, T value)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				UpdateRequest request = UpdateRequestExtensions.CreateForSingleField(ArtifactId, fieldGuid, value);
				await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public static async Task<IProgress> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, CreateProgressDto createProgressDto)
		{
			Progress progress = new Progress(serviceFactory, logger,
				createProgressDto.WorkspaceArtifactId, createProgressDto.SyncConfigurationArtifactId, createProgressDto.Name, createProgressDto.Order, createProgressDto.Status);
			await progress.CreateAsync().ConfigureAwait(false);
			return progress;
		}

		public static async Task<IProgress> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int artifactId)
		{
			Progress progress = new Progress(serviceFactory, logger, workspaceArtifactId, artifactId);
			await progress.ReadAsync().ConfigureAwait(false);
			return progress;
		}

		public static async Task<IReadOnlyCollection<IProgress>> QueryAllAsync(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			var progress = new Progress(serviceFactory, logger, workspaceArtifactId, syncConfigurationArtifactId, string.Empty);
			IReadOnlyCollection<IProgress> batches = await progress.QueryAllAsync().ConfigureAwait(false);
			return batches;
		}

		public static async Task<IProgress> QueryAsync(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
		{
			Progress progress = new Progress(serviceFactory, logger, workspaceArtifactId, syncConfigurationArtifactId, name);
			bool exists = await progress.QueryAsync().ConfigureAwait(false);
			return exists ? progress : null;
		}
	}
}
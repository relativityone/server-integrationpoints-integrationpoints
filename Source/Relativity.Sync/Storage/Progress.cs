﻿using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class Progress : IProgress
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly int _syncConfigurationArtifactId;
		private readonly int _workspaceArtifactId;

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid NameGuid = new Guid("AE2FCA2B-0E5C-4F35-948F-6C1654D5CF95");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");
		private static readonly Guid ParentArtifactGuid = new Guid("E0188DD7-4B1B-454D-AFA4-3CCC7F9DC001");

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;

			Name = name;
			Order = order;
			Status = status;
		}

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;

			ArtifactId = artifactId;
		}

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
		{
			_serviceFactory = serviceFactory;
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
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				ReadRequest request = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = ArtifactId
					},
					Fields = new[]
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
					}
				};

				ReadResult result = await objectManager.ReadAsync(_workspaceArtifactId, request).ConfigureAwait(false);

				Name = (string) result.Object[NameGuid].Value;
				Order = (int) result.Object[OrderGuid].Value;
				Status = ((string) result.Object[StatusGuid].Value).GetEnumFromDescription<SyncJobStatus>();
				Exception = (string) result.Object[ExceptionGuid].Value;
				Message = (string) result.Object[MessageGuid].Value;
			}
		}

		private async Task<bool> QueryAsync()
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = ProgressObjectTypeGuid
					},
					Condition = $"'{NameGuid}' == '{Name}' AND '{ParentArtifactGuid}' == {_syncConfigurationArtifactId}",
					Fields = new[]
					{
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
					}
				};

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, request, 1, 1).ConfigureAwait(false);

				if (result.Objects.Count > 0)
				{
					RelativityObject resultObject = result.Objects[0];
					ArtifactId = resultObject.ArtifactID;
					Order = (int) resultObject[OrderGuid].Value;
					Status = ((string) resultObject[StatusGuid].Value).GetEnumFromDescription<SyncJobStatus>();
					Exception = (string) resultObject[ExceptionGuid].Value;
					Message = (string) resultObject[MessageGuid].Value;

					return true;
				}

				return false;
			}
		}

		private async Task UpdateFieldValue<T>(Guid fieldGuid, T value)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				UpdateRequest request = UpdateRequestExtensions.CreateForSingleField(ArtifactId, fieldGuid, value);
				await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public static async Task<IProgress> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, CreateProgressDto createProgressDto)
		{
			Progress progress = new Progress(serviceFactory,
				createProgressDto.WorkspaceArtifactId, createProgressDto.SyncConfigurationArtifactId, createProgressDto.Name, createProgressDto.Order, createProgressDto.Status);
			await progress.CreateAsync().ConfigureAwait(false);
			return progress;
		}

		public static async Task<IProgress> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			Progress progress = new Progress(serviceFactory, workspaceArtifactId, artifactId);
			await progress.ReadAsync().ConfigureAwait(false);
			return progress;
		}

		public static async Task<IProgress> QueryAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
		{
			Progress progress = new Progress(serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, name);
			bool exists = await progress.QueryAsync().ConfigureAwait(false);
			return exists ? progress : null;
		}
	}
}
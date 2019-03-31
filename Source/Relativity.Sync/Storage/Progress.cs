using System;
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

		private static readonly Guid ProgressObjectTypeGuid = new Guid("7BA4FE64-683C-4492-BC1E-73E8E9CA8761");

		private static readonly Guid OrderGuid = new Guid("537FADE3-8C6C-412B-9A16-CBF97151AB97");
		private static readonly Guid StatusGuid = new Guid("536B1C81-59DB-41E5-8658-863AC8409E64");
		private static readonly Guid NameGuid = new Guid("319E2B89-26EC-4449-B3EC-8CEF553DB5EE");
		private static readonly Guid ExceptionGuid = new Guid("102F0B84-2122-4D2D-9B87-B877A033A6DD");
		private static readonly Guid MessageGuid = new Guid("E1CED416-38DB-49A8-90EC-BB8AD1DD1E6E");

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, string status)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;

			Name = name;
			Order = order;
			Status = status;
		}

		public Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;

			ArtifactId = artifactId;
		}

		public int ArtifactId { get; private set; }

		public string Name { get; private set; }

		public int Order { get; private set; }

		public string Status { get; private set; }

		public async Task SetStatusAsync(string status)
		{
			await UpdateFieldValue(StatusGuid, status).ConfigureAwait(false);
			Status = status;
		}

		public string Exception { get; private set; }

		public async Task SetExceptionAsync(Exception exception)
		{
			string exceptionString = exception.ToString();
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
							Value = Status
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

				Name = result.Object[NameGuid].Value.ToString();
				Order = (int) result.Object[OrderGuid].Value;
				Status = result.Object[StatusGuid].Value.ToString();
				Exception = result.Object[ExceptionGuid].Value.ToString();
				Message = result.Object[MessageGuid].Value.ToString();
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

#pragma warning disable RG2011 // Method Argument Count Analyzer
		public static async Task<IProgress> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, string status)
#pragma warning restore RG2011 // Method Argument Count Analyzer
		{
			Progress progress = new Progress(serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, name, order, status);
			await progress.CreateAsync().ConfigureAwait(false);
			return progress;
		}

		public static async Task<IProgress> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			Progress progress = new Progress(serviceFactory, workspaceArtifactId, artifactId);
			await progress.ReadAsync().ConfigureAwait(false);
			return progress;
		}
	}
}
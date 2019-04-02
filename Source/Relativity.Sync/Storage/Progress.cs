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

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid NameGuid = new Guid("AE2FCA2B-0E5C-4F35-948F-6C1654D5CF95");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");

		private Progress(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, string status)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(status))
			{
				throw new ArgumentNullException(nameof(status));
			}

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

				Name = (string) result.Object[NameGuid].Value;
				Order = (int) result.Object[OrderGuid].Value;
				Status = (string) result.Object[StatusGuid].Value;
				Exception = (string) result.Object[ExceptionGuid].Value;
				Message = (string) result.Object[MessageGuid].Value;
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
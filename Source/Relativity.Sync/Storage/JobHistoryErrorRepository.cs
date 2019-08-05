using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class JobHistoryErrorRepository : IJobHistoryErrorRepository
	{
		private readonly Guid _jobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");

		private readonly Guid _errorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private readonly Guid _errorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
		private readonly Guid _errorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
		private readonly Guid _nameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
		private readonly Guid _sourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
		private readonly Guid _stackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private readonly Guid _timestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

		private readonly Guid _errorStatusNew = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
		private readonly Guid _errorStatusExpired = new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57");
		private readonly Guid _errorStatusInProgress = new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA");
		private readonly Guid _errorStatusRetried = new Guid("7D3D393D-384F-434E-9776-F9966550D29A");

		private readonly Guid _errorTypeItem = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
		private readonly Guid _errorTypeJob = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");

		private readonly IDateTime _dateTime;
		private readonly ISourceServiceFactoryForAdmin _sourceServiceFactoryForAdmin;
		private readonly ISyncLog _logger;

		public JobHistoryErrorRepository(ISourceServiceFactoryForAdmin sourceServiceFactoryForAdmin, IDateTime dateTime, ISyncLog logger)
		{
			_sourceServiceFactoryForAdmin = sourceServiceFactoryForAdmin;
			_dateTime = dateTime;
			_logger = logger;
		}

		public async Task<IEnumerable<int>> MassCreateAsync(int workspaceArtifactId, int jobHistoryArtifactId, IList<CreateJobHistoryErrorDto> createJobHistoryErrorDtos)
		{
			_logger.LogInformation("Mass creating item level errors count: {count}", createJobHistoryErrorDtos.Count);

			IReadOnlyList<IReadOnlyList<object>> values = createJobHistoryErrorDtos.Select(x => new List<object>()
			{
				x.ErrorMessage,
				GetErrorStatusChoice(ErrorStatus.New),
				GetErrorTypeChoice(x.ErrorType),
				Guid.NewGuid().ToString(),
				x.SourceUniqueId,
				x.StackTrace,
				_dateTime.UtcNow
			}).ToList();

			using (IObjectManager objectManager = await _sourceServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new MassCreateRequest()
				{
					ObjectType = GetObjectTypeRef(),
					ParentObject = GetParentObject(jobHistoryArtifactId),
					Fields = GetFields(),
					ValueLists = values
				};
				MassCreateResult result = await objectManager.CreateAsync(workspaceArtifactId, request).ConfigureAwait(false);
				if (!result.Success)
				{
					throw new SyncException($"Mass creation of item level errors was not successfull. Message: {result.Message}");
				}

				_logger.LogInformation("Successfully mass-created item level errors: {count}", createJobHistoryErrorDtos.Count);
				return result.Objects.Select(x => x.ArtifactID);
			}
		}

		private ObjectTypeRef GetObjectTypeRef()
		{
			return new ObjectTypeRef()
			{
				Guid = _jobHistoryErrorObject
			};
		}

		private RelativityObjectRef GetParentObject(int jobHistoryArtifactId)
		{
			return new RelativityObjectRef()
			{
				ArtifactID = jobHistoryArtifactId
			};
		}

		private FieldRef[] GetFields()
		{
			return new[]
			{
				new FieldRef() {Guid = _errorMessageField},
				new FieldRef() {Guid = _errorStatusField},
				new FieldRef() {Guid = _errorTypeField},
				new FieldRef() {Guid = _nameField},
				new FieldRef() {Guid = _sourceUniqueIdField},
				new FieldRef() {Guid = _stackTraceField},
				new FieldRef() {Guid = _timestampUtcField}
			};
		}

		private ChoiceRef GetErrorStatusChoice(ErrorStatus errorStatus)
		{
			var errorStatusChoice = new ChoiceRef();

			switch (errorStatus)
			{
				case ErrorStatus.New:
					errorStatusChoice.Guid = _errorStatusNew;
					break;
				case ErrorStatus.InProgress:
					errorStatusChoice.Guid = _errorStatusInProgress;
					break;
				case ErrorStatus.Expired:
					errorStatusChoice.Guid = _errorStatusExpired;
					break;
				case ErrorStatus.Retried:
					errorStatusChoice.Guid = _errorStatusRetried;
					break;
				default:
					throw new ArgumentException($"Invalid Error Status {errorStatus}");
			}

			return errorStatusChoice;
		}

		private ChoiceRef GetErrorTypeChoice(ErrorType errorType)
		{
			var errorTypeChoice = new ChoiceRef();

			switch (errorType)
			{
				case ErrorType.Job:
					errorTypeChoice.Guid = _errorTypeJob;
					break;
				case ErrorType.Item:
					errorTypeChoice.Guid = _errorTypeItem;
					break;
				default:
					throw new ArgumentException($"Invalid Error Type {errorType}");
			}

			return errorTypeChoice;
		}

	}
}
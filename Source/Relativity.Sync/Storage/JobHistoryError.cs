using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class JobHistoryError : IJobHistoryError
	{
		private readonly IProxyFactory _proxyFactory;

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

		public int ArtifactId { get; private set; }

		public string ErrorMessage { get; private set; }

		public ErrorStatus ErrorStatus { get; private set; }

		public ErrorType ErrorType { get; private set; }

		public int JobHistoryArtifactId { get; private set; }

		public string Name { get; private set; }

		public string SourceUniqueId { get; private set; }

		public string StackTrace { get; private set; }

		public DateTime TimestampUtc { get; private set; }

		public JobHistoryError(IProxyFactory proxyFactory)
		{
			_proxyFactory = proxyFactory;
		}

		private async Task CreateAsync(int workspaceArtifactId, CreateJobHistoryErrorDto createJobHistoryErrorDto)
		{
			ErrorMessage = createJobHistoryErrorDto.ErrorMessage;
			ErrorStatus = ErrorStatus.New;
			ErrorType = createJobHistoryErrorDto.ErrorType;
			JobHistoryArtifactId = createJobHistoryErrorDto.JobHistoryArtifactId;
			Name = Guid.NewGuid().ToString();
			SourceUniqueId = createJobHistoryErrorDto.SourceUniqueId;
			StackTrace = createJobHistoryErrorDto.StackTrace;
			TimestampUtc = DateTime.UtcNow;

			using (var objectManager = await _proxyFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef { Guid = _jobHistoryErrorObject },
					ParentObject = new RelativityObjectRef { ArtifactID = JobHistoryArtifactId },
					FieldValues = GetFieldValueRefs()
				};

				CreateResult result = await objectManager.CreateAsync(workspaceArtifactId, request).ConfigureAwait(false);
				ArtifactId = result.Object.ArtifactID;
			}
		}

		private FieldRefValuePair[] GetFieldValueRefs()
		{
			FieldRefValuePair[] fieldValues =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _errorMessageField
					},
					Value = ErrorMessage
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _errorStatusField
					},
					Value = GetErrorStatusChoice()
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _errorTypeField
					},
					Value = GetErrorTypeChoice()
				},
				new FieldRefValuePair
				{
					Field =  new FieldRef
					{
						Guid = _nameField
					},
					Value = Name
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _sourceUniqueIdField
					},
					Value = SourceUniqueId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _stackTraceField
					},
					Value = StackTrace
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = _timestampUtcField
					},
					Value = TimestampUtc
				}
			};
			return fieldValues;
		}

		private ChoiceRef GetErrorStatusChoice()
		{
			var errorStatusChoice = new ChoiceRef();

			switch (ErrorStatus)
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
					throw new ArgumentException($"Invalid Error Status {ErrorStatus}");
			}

			return errorStatusChoice;
		}

		private ChoiceRef GetErrorTypeChoice()
		{
			var errorTypeChoice = new ChoiceRef();

			switch (ErrorType)
			{
				case ErrorType.Job:
					errorTypeChoice.Guid = _errorTypeJob;
					break;
				case ErrorType.Item:
					errorTypeChoice.Guid = _errorTypeItem;
					break;
				default:
					throw new ArgumentException($"Invalid Error Type {ErrorType}");
			}

			return errorTypeChoice;
		}

		public static async Task<IJobHistoryError> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, CreateJobHistoryErrorDto createJobHistoryErrorDto)
		{
			var jobHistoryError = new JobHistoryError(serviceFactory);
			await jobHistoryError.CreateAsync(workspaceArtifactId, createJobHistoryErrorDto).ConfigureAwait(false);
			return jobHistoryError;
		}
	}
}
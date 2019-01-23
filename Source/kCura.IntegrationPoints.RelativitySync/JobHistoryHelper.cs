using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class JobHistoryHelper
	{
		public async Task MarkJobAsStopped(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await UpdateJobAsync(job, JobStoppedStateRef(), manager).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsFailed(IExtendedJob job, Exception e, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await MarkJobAsFailed(job, manager).ConfigureAwait(false);
				await AddJobHistoryError(job, manager, e).ConfigureAwait(false);
			}
		}

		private static async Task MarkJobAsFailed(IExtendedJob job, IObjectManager manager)
		{
			await UpdateJobAsync(job, JobFailedStateRef(), manager).ConfigureAwait(false);
		}

		private static async Task UpdateJobAsync(IExtendedJob job, ChoiceRef status, IObjectManager manager)
		{
			UpdateRequest updateRequest = new UpdateRequest
			{
				Object = JobHistoryRef(job),
				FieldValues = new[]
				{
					new FieldRefValuePair
					{
						Field = JobStatusRef(),
						Value = status
					},
					new FieldRefValuePair
					{
						Field = JobIdRef(),
						Value = job.JobId.ToString()
					}
				}
			};
			await manager.UpdateAsync(job.WorkspaceId, updateRequest).ConfigureAwait(false);
		}

		private static FieldRef JobIdRef()
		{
			return new FieldRef
			{
				Guid = Guid.Parse(JobHistoryFieldGuids.JobID)
			};
		}

		private static RelativityObjectRef JobHistoryRef(IExtendedJob job)
		{
			return new RelativityObjectRef
			{
				ArtifactID = job.JobHistoryId
			};
		}

		private static FieldRef JobStatusRef()
		{
			return new FieldRef
			{
				Guid = Guid.Parse(JobHistoryFieldGuids.JobStatus)
			};
		}

		private static ChoiceRef JobStoppedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryStopped.Guids[0]
			};
		}

		private static ChoiceRef JobFailedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryErrorJobFailed.Guids[0]
			};
		}

		private async Task AddJobHistoryError(IExtendedJob job, IObjectManager manager, Exception e)
		{
			CreateRequest createRequest = new CreateRequest
			{
				ObjectType = JobHistoryErrorTypeRef(),
				ParentObject = JobHistoryRef(job),
				FieldValues = JobHistoryErrorFields(e)
			};
			await manager.CreateAsync(job.WorkspaceId, createRequest).ConfigureAwait(false);
		}

		private static ObjectTypeRef JobHistoryErrorTypeRef()
		{
			return new ObjectTypeRef
			{
				Guid = Guid.Parse(ObjectTypeGuids.JobHistoryError)
			};
		}

		private IEnumerable<FieldRefValuePair> JobHistoryErrorFields(Exception e)
		{
			return new[]
			{
				ErrorField(e),
				ErrorStatus(),
				ErrorType(),
				StackTrace(e),
				Name()
			};
		}

		private FieldRefValuePair ErrorField(Exception e)
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = Guid.Parse(JobHistoryErrorFieldGuids.Error)
				},
				Value = e.Message
			};
		}

		private FieldRefValuePair ErrorStatus()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = Guid.Parse(JobHistoryErrorFieldGuids.ErrorStatus)
				},
				Value = new ChoiceRef
				{
					Guid = ErrorStatusChoices.JobHistoryErrorNew.Guids[0]
				}
			};
		}

		private FieldRefValuePair ErrorType()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = Guid.Parse(JobHistoryErrorFieldGuids.ErrorType)
				},
				Value = new ChoiceRef
				{
					Guid = ErrorTypeChoices.JobHistoryErrorJob.Guids[0]
				}
			};
		}

		private FieldRefValuePair StackTrace(Exception e)
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = Guid.Parse(JobHistoryErrorFieldGuids.StackTrace)
				},
				Value = e.ToString()
			};
		}

		private FieldRefValuePair Name()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = Guid.Parse(JobHistoryErrorFieldGuids.Name)
				},
				Value = Guid.NewGuid().ToString()
			};
		}
	}
}
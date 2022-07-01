using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Extensions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal class JobHistorySyncService : IJobHistorySyncService
	{
		private readonly IHelper _helper;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IAPILog _logger;

        public JobHistorySyncService(IHelper helper, IRelativityObjectManager relativityObjectManager)
		{
			_helper = helper;
			_relativityObjectManager = relativityObjectManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobHistorySyncService>();
		}

		public async Task<RelativityObject> GetLastJobHistoryWithErrorsAsync(int workspaceID,
			int integrationPointArtifactID)
		{
			string integrationPointCondition = $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactID}])";
			string notRunningCondition = $"('{JobHistoryFields.EndTimeUTC}' ISSET)";
			string jobStatusCondition = $"('{JobHistoryFields.JobStatus}' IN CHOICE [{JobStatusChoices.JobHistoryCompletedWithErrorsGuid}, {JobStatusChoices.JobHistoryErrorJobFailedGuid}])";
			string condition = $"{integrationPointCondition} AND {notRunningCondition} AND {jobStatusCondition}";

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef()
				{
					Guid = ObjectTypeGuids.JobHistoryGuid
				},
				Condition = condition,
				Fields = new[]
				{
					new FieldRef
					{
						Guid = JobHistoryFieldGuids.IntegrationPointGuid
					}
				},
				Sorts = new List<Sort>
				{
					new Sort
					{
						Direction = SortEnum.Descending,
						FieldIdentifier = new FieldRef
						{
							Guid = JobHistoryFieldGuids.EndTimeUTCGuid
						}
					}
				}
			};

			List<RelativityObject> results = await _relativityObjectManager.QueryAsync(queryRequest, executionIdentity: ExecutionIdentity.System).ConfigureAwait(false);
			return results.FirstOrDefault();
		}

		public void UpdateJobStatus(string syncStatus, IExtendedJob job)
		{
			ChoiceRef status;

			const string validating = "validating";
			const string checkingPermissions = "checking permissions";

			if (syncStatus.Equals(validating, StringComparison.InvariantCultureIgnoreCase) || syncStatus.Equals(checkingPermissions, StringComparison.InvariantCultureIgnoreCase))
			{
				status = new ChoiceRef
				{
					Guid = JobStatusChoices.JobHistoryValidating.Guids[0]
				};
			}
			else
			{
				status = new ChoiceRef
				{
					Guid = JobStatusChoices.JobHistoryProcessing.Guids[0]
				};
			}

			IList<FieldRefValuePair> fieldRefValuePair = new[]
			{
				new FieldRefValuePair
				{
					Field = JobStatusRef(),
					Value = status
				}
			};

			_relativityObjectManager.Update(job.JobHistoryId, fieldRefValuePair, ExecutionIdentity.System);

		}

		public void MarkJobAsValidationFailedAsync(IExtendedJob job, Exception ex)
		{
			UpdateFinishedJob(job, JobValidationFailedRef(), true);
			AddJobHistoryError(job, ex);
		}

		public async Task MarkJobAsStoppedAsync(IExtendedJob job)
		{
			bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
			UpdateFinishedJob(job, JobStoppedStateRef(), hasErrors);
		}

		public async Task MarkJobAsSuspendingAsync(IExtendedJob job)
		{
			bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
			UpdateFinishedJob(job, JobSuspendingStateRef(), hasErrors);
		}

		public async Task MarkJobAsSuspendedAsync(IExtendedJob job)
		{
			bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
			UpdateFinishedJob(job, JobSuspendedStateRef(), hasErrors);
		}

		public void MarkJobAsFailed(IExtendedJob job, Exception e)
		{
			MarkJobAsFailed(job);
			AddJobHistoryError(job, e);
		}

		public void MarkJobAsStarted(IExtendedJob job)
		{
			IList<FieldRefValuePair> fieldValues = new[]
			{
				new FieldRefValuePair
				{
					Field = StartTimeRef(),
					Value = DateTime.UtcNow
				},
				new FieldRefValuePair
				{
					Field = JobIdRef(),
					Value = job.JobId.ToString(CultureInfo.InvariantCulture)
				}
			};

			_relativityObjectManager.Update(job.JobHistoryId, fieldValues, ExecutionIdentity.System);

		}

		public async Task MarkJobAsCompletedAsync(IExtendedJob job)
		{
			ChoiceRef status;
			bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
			if (hasErrors)
			{
				status = JobCompletedWithErrorsStateRef();
			}
			else
			{
				status = JobCompletedStateRef();
			}

			UpdateFinishedJob(job, status, hasErrors);
		}

		private async Task<bool> HasErrorsAsync(IExtendedJob job)
		{
			QueryRequest request = new QueryRequest
			{
				ObjectType = JobHistoryErrorTypeRef(),
				Condition =
					$"('{Data.JobHistoryErrorFields.JobHistory}' IN OBJECT [{job.JobHistoryId}]) AND ('{Data.JobHistoryErrorFields.ErrorType}' == CHOICE {ErrorTypeChoices.JobHistoryErrorItem.Guids[0]})"
			};
			Data.UtilityDTO.ResultSet<RelativityObject> itemLevelErrors = await _relativityObjectManager.QueryAsync(request, 0, 1).ConfigureAwait(false);
			_logger.LogInformation("JobHistorySyncService.HasErrors(): Found {itemLevelErrors} from JobHistoryErrorObjects", itemLevelErrors.ResultCount);

			QueryRequest requestForJobHistory = new QueryRequest()
			{
				ObjectType = JobHistoryRef(),
				Condition = $"'Artifact ID' == '{job.JobHistoryId}'"
			};

			List<JobHistory> jobHistoryFromQuery =
				await _relativityObjectManager.QueryAsync<JobHistory>(requestForJobHistory).ConfigureAwait(false);

			int? jobHistoryItemsWithErrors = jobHistoryFromQuery.Single().ItemsWithErrors;
			
			_logger.LogInformation("JobHistorySyncService.HasErrors(): Found {jobHistoryItemsWithErrors} from JobHistory.ItemsWithErrors", jobHistoryItemsWithErrors);

			return itemLevelErrors.ResultCount > 0 || jobHistoryItemsWithErrors > 0;
		}

		private void MarkJobAsFailed(IExtendedJob job)
		{
			UpdateFinishedJob(job, JobFailedStateRef(), true);
		}

		private void UpdateFinishedJob(IExtendedJob job, ChoiceRef status, bool hasErrors)
		{
            DateTime currentTimeUtc = DateTime.UtcNow;
			IList<FieldRefValuePair> fieldValues = new[]
			{
				new FieldRefValuePair
				{
					Field = JobStatusRef(),
					Value = status
				},
				new FieldRefValuePair
				{
					Field = EndTimeRef(),
					Value = currentTimeUtc
				}
			};

			_relativityObjectManager.Update(job.JobHistoryId, fieldValues, ExecutionIdentity.System);
			UpdateIntegrationPointLastRuntimeUtc(job, currentTimeUtc);
			UpdateIntegrationPointHasErrors(job, hasErrors);
		}

		private void UpdateIntegrationPointLastRuntimeUtc(IExtendedJob job, DateTime currentTimeUtc)
		{
			IList<FieldRefValuePair> fieldValues = new[]
			{
				new FieldRefValuePair
				{
					Field = LastRuntimeUtcRef(),
					Value = currentTimeUtc
				},
			};

			_relativityObjectManager.Update(job.IntegrationPointId, fieldValues, ExecutionIdentity.System);
		}

		private void UpdateIntegrationPointHasErrors(IExtendedJob job, bool hasErrors)
		{
			IList<FieldRefValuePair> fieldValues = new[]
			{
				new FieldRefValuePair
				{
					Field = HasErrorsRef(),
					Value = hasErrors
				},
			};

			_relativityObjectManager.Update(job.IntegrationPointId, fieldValues, ExecutionIdentity.System);
		}

		private static FieldRef JobIdRef()
		{
			return new FieldRef
			{
				Guid = JobHistoryFieldGuids.JobIDGuid
			};
		}

		private static FieldRef EndTimeRef()
		{
			return new FieldRef
			{
				Guid = JobHistoryFieldGuids.EndTimeUTCGuid
			};
		}

		private static RelativityObjectRef JobHistoryRef(IExtendedJob job)
		{
			return new RelativityObjectRef
			{
				ArtifactID = job.JobHistoryId
			};
		}
		
		private static ObjectTypeRef JobHistoryRef()
		{
			return new ObjectTypeRef
			{
				Guid = ObjectTypeGuids.JobHistoryGuid
			};
		}

		private static RelativityObjectRef IntegrationPointRef(IExtendedJob job)
		{
			return new RelativityObjectRef
			{
				ArtifactID = job.IntegrationPointId
			};
		}

		private static FieldRef LastRuntimeUtcRef()
		{
			return new FieldRef
			{
				Guid = IntegrationPointFieldGuids.LastRuntimeUTCGuid
			};
		}

		private static FieldRef HasErrorsRef()
		{
			return new FieldRef()
			{
				Guid = IntegrationPointFieldGuids.HasErrorsGuid
			};
		}

		private static FieldRef JobStatusRef()
		{
			return new FieldRef
			{
				Guid = JobHistoryFieldGuids.JobStatusGuid
			};
		}

		private static FieldRef StartTimeRef()
		{
			return new FieldRef
			{
				Guid = JobHistoryFieldGuids.StartTimeUTCGuid
			};
		}

		private static ChoiceRef JobValidationFailedRef()
		{
			return new ChoiceRef()
			{
				Guid = JobStatusChoices.JobHistoryValidationFailed.Guids[0]
			};
		}

		private static ChoiceRef JobStoppedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryStopped.Guids[0]
			};
		}

		private static ChoiceRef JobSuspendingStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistorySuspending.Guids[0]
			};
		}

		private static ChoiceRef JobSuspendedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistorySuspended.Guids[0]
			};
		}

		private static ChoiceRef JobCompletedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryCompleted.Guids[0]
			};
		}

		private static ChoiceRef JobCompletedWithErrorsStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]
			};
		}

		private static ChoiceRef JobFailedStateRef()
		{
			return new ChoiceRef
			{
				Guid = JobStatusChoices.JobHistoryErrorJobFailed.Guids[0]
			};
		}

		private void AddJobHistoryError(IExtendedJob job, Exception e)
		{

			ObjectTypeRef objectType = JobHistoryErrorTypeRef();
			RelativityObjectRef parentObject = JobHistoryRef(job);
			List<FieldRefValuePair> fieldValues = JobHistoryErrorFields(e);

			_relativityObjectManager.Create(objectType, parentObject, fieldValues, ExecutionIdentity.System);
		}

		private static ObjectTypeRef JobHistoryErrorTypeRef()
		{
			return new ObjectTypeRef
			{
				Guid = ObjectTypeGuids.JobHistoryErrorGuid
			};
		}

		private static List<FieldRefValuePair> JobHistoryErrorFields(Exception e)
		{
			return new List<FieldRefValuePair>
			{
				ErrorField(e),
				ErrorStatus(),
				ErrorType(),
				StackTrace(e),
				Name()
			};
		}

		private static FieldRefValuePair ErrorField(Exception ex)
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.ErrorGuid
				},
				Value = ex.FlattenErrorMessages()
			};
		}

		private static FieldRefValuePair ErrorStatus()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.ErrorStatusGuid
				},
				Value = new ChoiceRef
				{
					Guid = ErrorStatusChoices.JobHistoryErrorNew.Guids[0]
				}
			};
		}

		private static FieldRefValuePair ErrorType()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.ErrorTypeGuid
				},
				Value = new ChoiceRef
				{
					Guid = ErrorTypeChoices.JobHistoryErrorJob.Guids[0]
				}
			};
		}

		private static FieldRefValuePair StackTrace(Exception e)
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.StackTraceGuid
				},
				Value = e.ToString()
			};
		}

		private static FieldRefValuePair Name()
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.NameGuid
				},
				Value = Guid.NewGuid().ToString()
			};
		}
	}
}
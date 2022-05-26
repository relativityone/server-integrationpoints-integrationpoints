using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Extensions;
using Relativity.API;
using Relativity.Services.Objects;
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
			using (IObjectManager objectManager =
				_helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
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

				QueryResult result = await objectManager.QueryAsync(workspaceID, queryRequest, 0, 1).ConfigureAwait(false);
				return result.Objects.FirstOrDefault();
			}
		}

		public async Task UpdateJobStatusAsync(string syncStatus, IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
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

				UpdateRequest updateRequest = new UpdateRequest
				{
					Object = JobHistoryRef(job),
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = JobStatusRef(),
							Value = status
						}
					}
				};
				await manager.UpdateAsync(job.WorkspaceId, updateRequest).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsValidationFailedAsync(IExtendedJob job, Exception ex)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await UpdateFinishedJobAsync(job, JobValidationFailedRef(), manager, true).ConfigureAwait(false);
				await AddJobHistoryErrorAsync(job, manager, ex).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsStoppedAsync(IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
				await UpdateFinishedJobAsync(job, JobStoppedStateRef(), manager, hasErrors).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsSuspendingAsync(IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
				await UpdateFinishedJobAsync(job, JobSuspendingStateRef(), manager, hasErrors).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsSuspendedAsync(IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
				await UpdateFinishedJobAsync(job, JobSuspendedStateRef(), manager, hasErrors).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsFailedAsync(IExtendedJob job, Exception e)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await MarkJobAsFailedAsync(job, manager).ConfigureAwait(false);
				await AddJobHistoryErrorAsync(job, manager, e).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsStartedAsync(IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				UpdateRequest updateRequest = new UpdateRequest
				{
					Object = JobHistoryRef(job),
					FieldValues = new[]
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
					}
				};
				await manager.UpdateAsync(job.WorkspaceId, updateRequest).ConfigureAwait(false);
			}
		}

		public async Task MarkJobAsCompletedAsync(IExtendedJob job)
		{
			using (IObjectManager manager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
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

				await UpdateFinishedJobAsync(job, status, manager, hasErrors).ConfigureAwait(false);
			}
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

		private static Task MarkJobAsFailedAsync(IExtendedJob job, IObjectManager manager)
		{
			return UpdateFinishedJobAsync(job, JobFailedStateRef(), manager, true);
		}

		private static async Task UpdateFinishedJobAsync(IExtendedJob job, ChoiceRef status, IObjectManager manager, bool hasErrors)
		{
			var currentTimeUtc = DateTime.UtcNow;
			UpdateRequest jobHistoryUpdateRequest = new UpdateRequest
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
						Field = EndTimeRef(),
						Value = currentTimeUtc
					}
				}
			};
			await manager.UpdateAsync(job.WorkspaceId, jobHistoryUpdateRequest).ConfigureAwait(false);
			await UpdateIntegrationPointLastRuntimeUtcAsync(job, manager, currentTimeUtc).ConfigureAwait(false);
			await UpdateIntegrationPointHasErrorsAsync(job, manager, hasErrors).ConfigureAwait(false);
		}

		private static Task UpdateIntegrationPointLastRuntimeUtcAsync(IExtendedJob job, IObjectManager manager, DateTime currentTimeUtc)
		{
			UpdateRequest integrationPointUpdateRequest = new UpdateRequest
			{
				Object = IntegrationPointRef(job),
				FieldValues = new[]
				{
					new FieldRefValuePair
					{
						Field = LastRuntimeUtcRef(),
						Value = currentTimeUtc
					},
				}
			};
			return manager.UpdateAsync(job.WorkspaceId, integrationPointUpdateRequest);
		}

		private static Task UpdateIntegrationPointHasErrorsAsync(IExtendedJob job, IObjectManager manager, bool hasErrors)
		{
			UpdateRequest integrationPointUpdateRequest = new UpdateRequest
			{
				Object = IntegrationPointRef(job),
				FieldValues = new[]
				{
					new FieldRefValuePair
					{
						Field = HasErrorsRef(),
						Value = hasErrors
					},
				}
			};
			return manager.UpdateAsync(job.WorkspaceId, integrationPointUpdateRequest);
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

		private static Task AddJobHistoryErrorAsync(IExtendedJob job, IObjectManager manager, Exception e)
		{
			CreateRequest createRequest = new CreateRequest
			{
				ObjectType = JobHistoryErrorTypeRef(),
				ParentObject = JobHistoryRef(job),
				FieldValues = JobHistoryErrorFields(e)
			};
			return manager.CreateAsync(job.WorkspaceId, createRequest);
		}

		private static ObjectTypeRef JobHistoryErrorTypeRef()
		{
			return new ObjectTypeRef
			{
				Guid = ObjectTypeGuids.JobHistoryErrorGuid
			};
		}

		private static IEnumerable<FieldRefValuePair> JobHistoryErrorFields(Exception e)
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
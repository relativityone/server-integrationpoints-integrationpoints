using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;

namespace kCura.IntegrationPoints.RelativitySync
{
	public static class JobHistoryHelper
	{
		public static async Task UpdateJobStatusAsync(string syncStatus, IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
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

		public static async Task MarkJobAsValidationFailedAsync(ValidationException ex, IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await UpdateFinishedJobAsync(job, JobValidationFailedRef(), manager, true).ConfigureAwait(false);
				await AddJobHistoryErrorAsync(job, manager, ex).ConfigureAwait(false);
			}
		}

		public static async Task MarkJobAsStoppedAsync(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				bool hasErrors = await HasErrorsAsync(job, manager).ConfigureAwait(false);
				await UpdateFinishedJobAsync(job, JobStoppedStateRef(), manager, hasErrors).ConfigureAwait(false);
			}
		}

		public static async Task MarkJobAsFailedAsync(IExtendedJob job, Exception e, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				await MarkJobAsFailedAsync(job, manager).ConfigureAwait(false);
				await AddJobHistoryErrorAsync(job, manager, e).ConfigureAwait(false);
			}
		}

		public static async Task MarkJobAsStartedAsync(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
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

		public static async Task MarkJobAsCompletedAsync(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager manager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				ChoiceRef status;
				bool hasErrors = await HasErrorsAsync(job, manager).ConfigureAwait(false);
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

		private static async Task<bool> HasErrorsAsync(IExtendedJob job, IObjectManager manager)
		{
			QueryRequest request = new QueryRequest
			{
				ObjectType = JobHistoryErrorTypeRef(),
				Condition =
					$"('{Data.JobHistoryErrorFields.JobHistory}' IN OBJECT [{job.JobHistoryId}]) AND ('{Data.JobHistoryErrorFields.ErrorType}' == CHOICE {ErrorTypeChoices.JobHistoryErrorItem.Guids[0]})"
			};
			QueryResult queryResult = await manager.QueryAsync(job.WorkspaceId, request, 0, 1).ConfigureAwait(false);
			return queryResult.ResultCount > 0;
		}

		private static async Task MarkJobAsFailedAsync(IExtendedJob job, IObjectManager manager)
		{
			await UpdateFinishedJobAsync(job, JobFailedStateRef(), manager, true).ConfigureAwait(false);
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
			await UpdateIntegrationPointLastRuntimeUtc(job, manager, currentTimeUtc).ConfigureAwait(false);
			await UpdateIntegrationPointHasErrorsAsync(job, manager, hasErrors).ConfigureAwait(false);
		}

		private static async Task UpdateIntegrationPointLastRuntimeUtc(IExtendedJob job, IObjectManager manager, DateTime currentTimeUtc)
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
			await manager.UpdateAsync(job.WorkspaceId, integrationPointUpdateRequest).ConfigureAwait(false);
		}

		private static async Task UpdateIntegrationPointHasErrorsAsync(IExtendedJob job, IObjectManager manager, bool hasErrors)
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
			await manager.UpdateAsync(job.WorkspaceId, integrationPointUpdateRequest).ConfigureAwait(false);
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

		private static async Task AddJobHistoryErrorAsync(IExtendedJob job, IObjectManager manager, Exception e)
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

		private static FieldRefValuePair ErrorField(Exception e)
		{
			return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = JobHistoryErrorFieldGuids.ErrorGuid
				},
				Value = e.Message
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
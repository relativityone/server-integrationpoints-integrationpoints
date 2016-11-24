using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.Helpers;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public abstract class IntegrationPointServiceBase<T> where T : BaseRdo, new()
	{
		protected ISerializer Serializer;
		protected ICaseServiceContext Context;
		protected IContextContainer ContextContainer;
		protected IChoiceQuery ChoiceQuery;
		protected IManagerFactory ManagerFactory;
		protected static readonly object Lock = new object();
		
		protected abstract string UnableToSaveFormat { get; }

		protected IntegrationPointServiceBase(IHelper helper, ICaseServiceContext context, IChoiceQuery choiceQuery,
			ISerializer serializer, IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory)
		{
			Serializer = serializer;
			Context = context;
			ChoiceQuery = choiceQuery;
			ManagerFactory = managerFactory;
			ContextContainer = contextContainerFactory.CreateContextContainer(helper);
		}
		public IList<T> GetAllRDOs()
		{
			var integrationPointProfileQuery = new IntegrationPointBaseQuery<T>(Context.RsapiService);
			return integrationPointProfileQuery.GetAllIntegrationPoints();
		}

		public T GetRdo(int artifactId)
		{
			try
			{
				return Context.RsapiService.GetGenericLibrary<T>().Read(artifactId);
			}
			catch (Exception ex)
			{
				throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT, ex);
			}
		}

		public IEnumerable<FieldMap> GetFieldMap(int artifactId)
		{
			IEnumerable<FieldMap> mapping = new List<FieldMap>();
			if (artifactId > 0)
			{
				string fieldmap =
						Context.RsapiService.IntegrationPointProfileLibrary.Read(artifactId, new Guid(IntegrationPointProfileFieldGuids.FieldMappings))
							.FieldMappings;

				if (!String.IsNullOrEmpty(fieldmap))
				{
					mapping = Serializer.Deserialize<IEnumerable<FieldMap>>(fieldmap);
				}
			}
			return mapping;
		}

		protected abstract IntegrationPointModelBase GetModel(int artifactId);

		public string GetSourceOptions(int artifactId)
		{
			return GetModel(artifactId).SourceConfiguration;
		}

		public FieldEntry GetIdentifierFieldEntry(int artifactId)
		{
			var model = GetModel(artifactId);
			var fields = Serializer.Deserialize<List<FieldMap>>(model.Map);
			return fields.First(x => x.FieldMapType == FieldMapTypeEnum.Identifier).SourceField;
		}

		public IEnumerable<string> GetRecipientEmails(int artifactId)
		{
			var integrationPoint = GetModel(artifactId);
			string emailRecipients = integrationPoint.NotificationEmails ?? string.Empty;
			IEnumerable<string> emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim());
			return emailRecipientList;
		}

		protected PeriodicScheduleRule ConvertModelToScheduleRule(IntegrationPointModelBase model)
		{
			var periodicScheduleRule = new PeriodicScheduleRule();
			DateTime startDate;
			if (DateTime.TryParse(model.Scheduler.StartDate, out startDate))
			{
				periodicScheduleRule.StartDate = startDate;
			}
			DateTime endDate;
			if (DateTime.TryParse(model.Scheduler.EndDate, out endDate))
			{
				periodicScheduleRule.EndDate = endDate;
			}
			periodicScheduleRule.TimeZoneOffsetInMinute = model.Scheduler.TimeZoneOffsetInMinute;

			//since we do not know what user local time is, time is passed in UTC
			TimeSpan time;
			if (TimeSpan.TryParse(model.Scheduler.ScheduledTime, out time))
			{
				periodicScheduleRule.LocalTimeOfDay =
					DateTime.UtcNow.Date.Add(new DateTime(time.Ticks, DateTimeKind.Utc).TimeOfDay).ToLocalTime().TimeOfDay;
			}
			ScheduleInterval interval;
			if (Enum.TryParse(model.Scheduler.SelectedFrequency, true, out interval))
			{
				periodicScheduleRule.Interval = interval;
			}
			periodicScheduleRule.Reoccur = Convert.ToInt32(model.Scheduler.Reoccur);
			periodicScheduleRule.DayOfMonth = null;

			switch (periodicScheduleRule.Interval)
			{
				case ScheduleInterval.Weekly:
					var sendOn = Serializer.Deserialize<Weekly>(model.Scheduler.SendOn);
					periodicScheduleRule.DaysToRun =
						DaysOfWeekConverter.FromDayOfWeek(Enumerable.Select<string, DayOfWeek>(sendOn.SelectedDays, x => (DayOfWeek) Enum.Parse(typeof(DayOfWeek), x)).ToList());
					break;
				case ScheduleInterval.Monthly:
					var monthlySendOn = Serializer.Deserialize<Monthly>(model.Scheduler.SendOn);
					if (monthlySendOn.MonthChoice == MonthlyType.Days)
					{
						periodicScheduleRule.DayOfMonth = monthlySendOn.SelectedDay;
					}
					else if (monthlySendOn.MonthChoice == MonthlyType.Month)
					{
						periodicScheduleRule.DaysToRun = monthlySendOn.SelectedDayOfTheMonth;
						periodicScheduleRule.SetLastDayOfMonth = monthlySendOn.SelectedType == OccuranceInMonth.Last;
						periodicScheduleRule.OccuranceInMonth = monthlySendOn.SelectedType;
					}
					break;
			}
			return periodicScheduleRule;
		}

		protected SourceProvider GetSourceProvider(int? sourceProviderArtifactId)
		{
			if (!sourceProviderArtifactId.HasValue)
			{
				throw new Exception(Constants.IntegrationPoints.NO_SOURCE_PROVIDER_SPECIFIED);
			}

			SourceProvider sourceProvider = null;
			try
			{
				sourceProvider = Context.RsapiService.SourceProviderLibrary.Read(sourceProviderArtifactId.Value);
			}
			catch (Exception e)
			{
				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_SOURCE_PROVIDER, e);
			}

			return sourceProvider;
		}

		protected DestinationProvider GetDestinationProvider(int? destinationProviderArtifactId)
		{
			if (!destinationProviderArtifactId.HasValue)
			{
				throw new Exception(Constants.IntegrationPoints.NO_DESTINATION_PROVIDER_SPECIFIED);
			}

			DestinationProvider destinationProvider = null;
			try
			{
				destinationProvider = Context.RsapiService.DestinationProviderLibrary.Read(destinationProviderArtifactId.Value);
			}
			catch (Exception e)
			{
				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER, e);
			}

			return destinationProvider;
		}

		protected void ValidateConfigurationWhenUpdatingObject(IntegrationPointModelBase model, IntegrationPointModelBase existingModel)
		{
			// check that only fields that are allowed to be changed are changed
			List<string> invalidProperties = new List<string>();

			if (existingModel.Name != model.Name)
			{
				invalidProperties.Add("Name");
			}
			if (existingModel.DestinationProvider != model.DestinationProvider)
			{
				invalidProperties.Add("Destination Provider");
			}
			if (existingModel.Destination != model.Destination)
			{
				dynamic existingDestination = JsonConvert.DeserializeObject(existingModel.Destination);
				dynamic newDestination = JsonConvert.DeserializeObject(model.Destination);

				if (existingDestination.artifactTypeID != newDestination.artifactTypeID)
				{
					invalidProperties.Add("Destination RDO");
				}
				if (existingDestination.CaseArtifactId != newDestination.CaseArtifactId)
				{
					invalidProperties.Add("Case");
				}
			}
			if (existingModel.SourceProvider != model.SourceProvider)
			{
				// If the source provider has been changed, the code below this exception is invalid
				invalidProperties.Add("Source Provider");
				throw new Exception(String.Format(UnableToSaveFormat, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}

			// check permission if we want to push
			// needs to be here because custom page is the only place that has user context
			SourceProvider provider;
			try
			{
				provider = Context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to save Integration Point: Unable to retrieve source provider", e);
			}

			if (provider.Identifier.Equals(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				if (existingModel != null && (existingModel.SourceConfiguration != model.SourceConfiguration))
				{
					invalidProperties.Add("Source Configuration");
				}
			}

			if (invalidProperties.Any())
			{
				throw new Exception(String.Format(UnableToSaveFormat, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}
		}

		protected void CreateRelativityError(string message, string fullText)
		{
			IErrorManager errorManager = ManagerFactory.CreateErrorManager(ContextContainer);
			var error = new ErrorDTO()
			{
				Message = message,
				FullText = fullText,
				Source = Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = Context.WorkspaceID
			};

			errorManager.Create(new[] { error });
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.Helpers;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public abstract class IntegrationPointServiceBase<T> where T : BaseRdo, new()
	{
		private readonly IIntegrationPointBaseFieldGuidsConstants _guidsConstants;
		protected IIntegrationPointSerializer Serializer;
		protected ICaseServiceContext Context;
		protected IContextContainer SourceContextContainer;

		protected IChoiceQuery ChoiceQuery;
		protected IManagerFactory ManagerFactory;
		protected IIntegrationPointProviderValidator IntegrationModelValidator;
		protected IIntegrationPointPermissionValidator _permissionValidator;
		protected IHelper _helper;

		protected static readonly object Lock = new object();

		protected abstract string UnableToSaveFormat { get; }

		protected IntegrationPointServiceBase(
			IHelper helper,
			ICaseServiceContext context,
			IChoiceQuery choiceQuery,
			IIntegrationPointSerializer serializer,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IIntegrationPointBaseFieldGuidsConstants guidsConstants,
			IIntegrationPointProviderValidator integrationModelValidator,
			IIntegrationPointPermissionValidator permissionValidator)
		{
			Serializer = serializer;
			Context = context;
			ChoiceQuery = choiceQuery;
			ManagerFactory = managerFactory;
			_guidsConstants = guidsConstants;
			IntegrationModelValidator = integrationModelValidator;
			_permissionValidator = permissionValidator;
			_helper = helper;
			SourceContextContainer = contextContainerFactory.CreateContextContainer(helper);
		}

		public IList<T> GetAllRDOs()
		{
			var query = new IntegrationPointBaseQuery<T>(Context.RsapiService);
			return query.GetAllIntegrationPoints();
		}

		public IList<T> GetALlRDOsWithBasicProfileColumns()
		{
			var query = new IntegrationPointBaseQuery<T>(Context.RsapiService);
			return query.GetAllIntegrationPointsProfileWithBasicColumns();
			
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
						Context.RsapiService.GetGenericLibrary<T>().Read(artifactId, new Guid(_guidsConstants.FieldMappings)).GetField<string>(new Guid(_guidsConstants.FieldMappings));

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
			return fields.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier)?.SourceField;
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
			const string dateFormat = "M/dd/yyyy";
			var periodicScheduleRule = new PeriodicScheduleRule();
			DateTime startDate;
			if (DateTime.TryParseExact(model.Scheduler.StartDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
			{
				periodicScheduleRule.StartDate = startDate;
			}
			DateTime endDate;
			if (DateTime.TryParseExact(model.Scheduler.EndDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
			{
				periodicScheduleRule.EndDate = endDate;
			}
			periodicScheduleRule.TimeZoneOffsetInMinute = model.Scheduler.TimeZoneOffsetInMinute;
			periodicScheduleRule.TimeZoneId = model.Scheduler.TimeZoneId;
			
			TimeSpan localTime;
			if (TimeSpan.TryParse(model.Scheduler.ScheduledTime, out localTime))
			{
				periodicScheduleRule.LocalTimeOfDay = localTime;
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
						DaysOfWeekConverter.FromDayOfWeek(Enumerable.Select<string, DayOfWeek>(sendOn.SelectedDays, x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToList());
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

		protected IntegrationPointType GetIntegrationPointType(int? integrationPointTypeArtifactId)
		{
			if (!integrationPointTypeArtifactId.HasValue)
			{
				throw new Exception(Constants.IntegrationPoints.NO_INTEGRATION_POINT_TYPE_SPECIFIED);
			}

			IntegrationPointType integrationPointType = null;
			try
			{
				integrationPointType = Context.RsapiService.IntegrationPointTypeLibrary.Read(integrationPointTypeArtifactId.Value);
			}
			catch (Exception e)
			{
				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT_TYPE, e);
			}

			return integrationPointType;
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
			}
			if (existingModel.SourceProvider != model.SourceProvider)
			{
				// If the source provider has been changed, the code below this exception is invalid
				invalidProperties.Add("Source Provider");
				throw new Exception(String.Format(UnableToSaveFormat, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}

			// check permission if we want to push
			// needs to be here because custom page is the only place that has user context
			try
			{
				Context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to save Integration Point: Unable to retrieve source provider", e);
			}

			if (invalidProperties.Any())
			{
				throw new Exception(String.Format(UnableToSaveFormat, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}
		}

		protected void CreateRelativityError(string message, string fullText)
		{
			IErrorManager errorManager = ManagerFactory.CreateErrorManager(SourceContextContainer);
			var error = new ErrorDTO()
			{
				Message = message,
				FullText = fullText,
				Source = Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = Context.WorkspaceID
			};

			errorManager.Create(new[] { error });
		}

		protected void RunValidation(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid)
		{
			ValidationResult validationResult = IntegrationModelValidator.Validate(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid);

			if (!validationResult.IsValid)
			{
				throw new IntegrationPointProviderValidationException(validationResult);
			}

			var permissionCheck = _permissionValidator.ValidateSave(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid);

			if (Context.EddsUserID == 0)
			{
				permissionCheck.Add(Constants.IntegrationPoints.NO_USERID);
			}

			if (!permissionCheck.IsValid)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
					$"{Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{String.Join(Environment.NewLine, permissionCheck.Messages)}");

				throw new PermissionException(Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE);
			}
		}
	}
}
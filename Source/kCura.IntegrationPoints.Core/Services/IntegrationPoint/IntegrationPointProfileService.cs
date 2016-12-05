using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public class IntegrationPointProfileService : IntegrationPointServiceBase<IntegrationPointProfile>, IIntegrationPointProfileService
	{
		public IntegrationPointProfileService(IHelper helper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer,
			IChoiceQuery choiceQuery,
			IManagerFactory managerFactory,
			IIntegrationPointProviderValidator integrationModelValidator)
			: base(helper, context, choiceQuery, serializer, managerFactory, contextContainerFactory, new IntegrationPointProfileFieldGuidsConstants(), integrationModelValidator)
		{
		}

		protected override string UnableToSaveFormat => "Unable to save Integration Point Profile:{0} cannot be changed once the Integration Point Profile has been saved";

		public virtual IntegrationPointProfileModel ReadIntegrationPointProfile(int artifactId)
		{
			IntegrationPointProfile integrationPoint = GetRdo(artifactId);
			var integrationPointProfileModel = IntegrationPointProfileModel.FromIntegrationPointProfile(integrationPoint);
			return integrationPointProfileModel;
		}

		public IList<IntegrationPointProfileModel> ReadIntegrationPointProfiles()
		{
			IList<IntegrationPointProfile> integrationPointProfiles = GetAllRDOs();
			return integrationPointProfiles.Select(IntegrationPointProfileModel.FromIntegrationPointProfile).ToList();
		}

		public IList<IntegrationPointProfileModel> ReadIntegrationPointProfilesSimpleModel()
		{
			IList<IntegrationPointProfile> integrationPointProfiles = GetALlRDOsWithBasicProfileColumns();
			return integrationPointProfiles.Select(IntegrationPointProfileModel.FromIntegrationPointProfileSimpleModel).ToList();
		}

		public int SaveIntegration(IntegrationPointProfileModel model)
		{
			IntegrationPointProfile profile;
			PeriodicScheduleRule rule;
			try
			{
				IList<Choice> choices =
					ChoiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields));

				rule = ConvertModelToScheduleRule(model);
				profile = model.ToRdo(choices, rule);

				SourceProvider sourceProvider = GetSourceProvider(profile.SourceProvider);
				DestinationProvider destinationProvider = GetDestinationProvider(profile.DestinationProvider);

				var validationResult = IntegrationModelValidator.Validate(model, sourceProvider, destinationProvider);
				if (!validationResult.IsValid)
				{
					throw new IntegrationPointProviderValidationException(validationResult);
				}

				//TODO create CheckForProviderAdditionalPermissions for IP Profile
				//if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) &&
				//	destinationProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID))
				//{
				//	CheckForProviderAdditionalPermissions(integrationPoint, Constants.SourceProvider.Relativity, Context.EddsUserID);
				//}
				//else
				//{
				//	CheckForProviderAdditionalPermissions(integrationPoint, Constants.SourceProvider.Other, Context.EddsUserID);
				//}

				//save RDO
				if (profile.ArtifactId > 0)
				{
					Context.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Update(profile);
				}
				else
				{
					profile.ArtifactId = Context.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Create(profile);
				}
			}
			catch (PermissionException)
			{
				throw;
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Constants.IntegrationPointProfiles.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_PROFILE_ADMIN_MESSAGE,
					string.Join(Environment.NewLine, e.Message, e.StackTrace));

				throw new Exception(Constants.IntegrationPointProfiles.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_PROFILE_USER_MESSAGE);
			}
			return profile.ArtifactId;
		}

		protected override IntegrationPointModelBase GetModel(int artifactId)
		{
			return ReadIntegrationPointProfile(artifactId);
		}
		
	}
}
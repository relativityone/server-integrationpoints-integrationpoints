﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public class IntegrationPointProfileService : IntegrationPointServiceBase, IIntegrationPointProfileService
	{
		public IntegrationPointProfileService(IHelper helper,
			ICaseServiceContext context,
			IIntegrationPointSerializer serializer,
			IChoiceQuery choiceQuery,
			IManagerFactory managerFactory,
			IValidationExecutor validationExecutor,
			IRelativityObjectManager objectManager)
			: base(helper, 
				context, 
				choiceQuery, 
				serializer, 
				managerFactory, 
				validationExecutor, 
				objectManager)
		{
		}

		protected override string UnableToSaveFormat => "Unable to save Integration Point Profile:{0} cannot be changed once the Integration Point Profile has been saved";

		public IList<IntegrationPointProfile> GetAllRDOs()
		{
			IEnumerable<FieldRef> fields = BaseRdo.GetFieldMetadata(
				typeof(IntegrationPointProfile)).Values.ToList()
					.Select(field => new FieldRef { Guid = field.FieldGuid }
			);

			var query = new QueryRequest
			{
				Fields = fields
			};

			return ObjectManager.Query<IntegrationPointProfile>(query);
		}

		public IList<IntegrationPointProfile> GetAllRDOsWithAllFields()
		{
			var query = new QueryRequest();
			IList<IntegrationPointProfile> result = ObjectManager.Query<IntegrationPointProfile>(query);
			result.Select(integrationPoint => ObjectManager.Read<IntegrationPointProfile>(integrationPoint.ArtifactId))
				.ToList();
			return result;
		}

		public IntegrationPointProfile ReadIntegrationPointProfile(int artifactId)
		{
			try
			{
				return ObjectManager.Read<IntegrationPointProfile>(artifactId);
			}
			catch (Exception ex)
			{
				throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT_PROFILE, ex);
			}
		}

		public virtual IntegrationPointProfileModel ReadIntegrationPointProfileModel(int artifactId)
		{
			IntegrationPointProfile integrationPoint = ReadIntegrationPointProfile(artifactId);
			IntegrationPointProfileModel integrationPointProfileModel = IntegrationPointProfileModel.FromIntegrationPointProfile(integrationPoint);
			return integrationPointProfileModel;
		}

		public IList<IntegrationPointProfileModel> ReadIntegrationPointProfiles()
		{
			IList<IntegrationPointProfile> integrationPointProfiles = GetAllRDOs();
			return integrationPointProfiles.Select(IntegrationPointProfileModel.FromIntegrationPointProfile).ToList();
		}

		public IList<IntegrationPointProfileModel> ReadIntegrationPointProfilesSimpleModel()
		{
			IList<IntegrationPointProfile> integrationPointProfiles = GetAllRDOsWithBasicProfileColumns();
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

				var integrationProfilePointModel = IntegrationPointProfileModel.FromIntegrationPointProfile(profile);

				SourceProvider sourceProvider = GetSourceProvider(profile.SourceProvider);
				DestinationProvider destinationProvider = GetDestinationProvider(profile.DestinationProvider);
				IntegrationPointType integrationPointType = GetIntegrationPointType(profile.Type);

				RunValidation(
					integrationProfilePointModel,
					sourceProvider,
					destinationProvider,
					integrationPointType,
					ObjectTypeGuids.IntegrationPointProfileGuid);

				//save RDO
				if (profile.ArtifactId > 0)
				{
					ObjectManager.Update(profile);
				}
				else
				{
					profile.ArtifactId = ObjectManager.Create(profile);
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

		public void UpdateIntegrationPointProfile(IntegrationPointProfile profile)
		{
			ObjectManager.Update(profile);
		}

		protected IList<IntegrationPointProfile> GetAllRDOsWithBasicProfileColumns()
		{
			var fields = BaseRdo.GetFieldMetadata(typeof(IntegrationPointProfile)).Values.ToList()
				.Select(field => new FieldValue(field.FieldGuid))
				.Where(field => field.Guids.Contains(IntegrationPointProfileFieldGuids.DestinationProviderGuid) ||
				                field.Guids.Contains(IntegrationPointProfileFieldGuids.SourceProviderGuid) ||
				                field.Guids.Contains(IntegrationPointProfileFieldGuids.NameGuid) ||
				                field.Guids.Contains(IntegrationPointProfileFieldGuids.TypeGuid))
				.Select(field => new FieldRef { Guid = field.Guids.First() });

			var query = new QueryRequest()
			{
				Fields = fields
			};

			return ObjectManager.Query<IntegrationPointProfile>(query);
		}
	}
}
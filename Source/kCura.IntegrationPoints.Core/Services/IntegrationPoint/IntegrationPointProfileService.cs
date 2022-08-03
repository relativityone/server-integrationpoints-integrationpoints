using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

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
                IntegrationPointProfile integrationPointProfile = ObjectManager.Read<IntegrationPointProfile>(artifactId);

                integrationPointProfile.DestinationConfiguration = GetDestinationConfiguration(artifactId);
                integrationPointProfile.SourceConfiguration = GetSourceConfiguration(artifactId);
                integrationPointProfile.FieldMappings = GetFieldMappings(artifactId);

                return integrationPointProfile;
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT_PROFILE, ex);
            }
        }

        private string GetDestinationConfiguration(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.DestinationConfigurationGuid });
        }

        private string GetSourceConfiguration(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid });
        }

        private string GetFieldMappings(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.FieldMappingsGuid });
        }

        private string GetUnicodeLongText(int artifactId, FieldRef field)
        {
            Stream unicodeLongTextStream = ObjectManager.StreamUnicodeLongText(artifactId, field);
            using (StreamReader unicodeLongTextStreamReader = new StreamReader(unicodeLongTextStream))
            {
                return unicodeLongTextStreamReader.ReadToEnd();
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
                IList<global::Relativity.Services.Choice.ChoiceRef> choices =
                    ChoiceQuery.GetChoicesOnField(Context.WorkspaceID, Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields));

                rule = ConvertModelToScheduleRule(model);
                profile = model.ToRdo(choices, rule);

                IntegrationPointProfileModel integrationProfilePointModel = IntegrationPointProfileModel.FromIntegrationPointProfile(profile);

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
            catch (IntegrationPointValidationException validationException)
            {
                CreateRelativityError(
                    Constants.IntegrationPointProfiles.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_PROFILE_ADMIN_MESSAGE,
                    string.Join(Environment.NewLine, validationException.ValidationResult.MessageTexts));
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
            IEnumerable<FieldRef> fields = BaseRdo
                .GetFieldMetadata(typeof(IntegrationPointProfile))
                .Values
                .ToList()
                .Select(fieldGuid => fieldGuid.FieldGuid)
                .Where(fieldGuid => fieldGuid.Equals(IntegrationPointProfileFieldGuids.DestinationProviderGuid) ||
                                    fieldGuid.Equals(IntegrationPointProfileFieldGuids.SourceProviderGuid) ||
                                    fieldGuid.Equals(IntegrationPointProfileFieldGuids.NameGuid) ||
                                    fieldGuid.Equals(IntegrationPointProfileFieldGuids.TypeGuid))
                .Select(fieldGuid => new FieldRef
                {
                    Guid = fieldGuid
                });

            var query = new QueryRequest()
            {
                Fields = fields
            };

            return ObjectManager.Query<IntegrationPointProfile>(query);
        }
    }
}
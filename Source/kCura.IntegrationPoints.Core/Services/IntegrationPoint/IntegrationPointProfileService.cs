using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public class IntegrationPointProfileService : IntegrationPointServiceBase, IIntegrationPointProfileService
    {
        private readonly IAPILog _logger;
        private readonly IRetryHandler _retryHandler;

        public IntegrationPointProfileService(
            IHelper helper,
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
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointService>();
            _retryHandler = new RetryHandlerFactory(_logger).Create();
        }

        protected override string UnableToSaveFormat => "Unable to save Integration Point Profile:{0} cannot be changed once the Integration Point Profile has been saved";

        public IntegrationPointProfileDto Read(int artifactId)
        {
            try
            {
                IntegrationPointProfile profile = ObjectManager.Read<IntegrationPointProfile>(artifactId);
                IntegrationPointProfileDto profileDto = ToDto(profile);
                profileDto.DestinationConfiguration = GetDestinationConfiguration(artifactId);
                profileDto.SourceConfiguration = GetSourceConfiguration(artifactId);
                profileDto.FieldMappings = GetFieldMappings(artifactId);
                return profileDto;
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT_PROFILE, ex);
            }
        }

        public IList<IntegrationPointProfileDto> ReadAll()
        {
            IEnumerable<FieldRef> fields = BaseRdo
                .GetFieldMetadata(typeof(IntegrationPointProfile))
                .Values
                .ToList()
                .Select(field => new FieldRef { Guid = field.FieldGuid });

            var query = new QueryRequest
            {
                Fields = fields
            };

            List<IntegrationPointProfile> profiles = ObjectManager.Query<IntegrationPointProfile>(query);
            return profiles.Select(ToDto).ToList();
        }

        public int SaveProfile(IntegrationPointProfileDto dto)
        {
            IntegrationPointProfile profile;
            PeriodicScheduleRule rule;
            try
            {
                IList<ChoiceRef> choices =
                    ChoiceQuery.GetChoicesOnField(Context.WorkspaceID, Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields));

                rule = ConvertModelToScheduleRule(dto);


                SourceProvider sourceProvider = GetSourceProvider(dto.SourceProvider);
                DestinationProvider destinationProvider = GetDestinationProvider(dto.DestinationProvider);
                IntegrationPointType integrationPointType = GetIntegrationPointType(dto.Type);

                RunValidation(
                    dto,
                    sourceProvider,
                    destinationProvider,
                    integrationPointType,
                    ObjectTypeGuids.IntegrationPointProfileGuid);

                //save RDO
                profile = ToRdo(dto, choices, rule);
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

        public void UpdateConfiguration(int profileArtifactId, string sourceConfiguration, string destinationConfiguration)
        {
            IntegrationPointProfile profile = ObjectManager.Read<IntegrationPointProfile>(profileArtifactId);
            profile.SourceConfiguration = sourceConfiguration;
            profile.DestinationConfiguration = destinationConfiguration;
            ObjectManager.Update(profile);
        }

        private string GetDestinationConfiguration(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.DestinationConfigurationGuid });
        }

        private string GetSourceConfiguration(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid });
        }

        private List<FieldMap> GetFieldMappings(int artifactId)
        {
            return _retryHandler.Execute<List<FieldMap>, RipSerializationException>(
                ReadFieldMapping,
                exception =>
                {
                    _logger.LogWarning(
                        exception,
                        "Unable to deserialize field mapping for integration point profile: {integrationPointProfileId}. Mapping value: {fieldMapping}. Operation will be retried.",
                        artifactId,
                        exception.Value);
                });

            List<FieldMap> ReadFieldMapping()
            {
                string fieldMapString = GetUnicodeLongText(artifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.FieldMappingsGuid });
                List<FieldMap> fieldMap = Serializer.Deserialize<List<FieldMap>>(fieldMapString);
                SanitizeFieldsMapping(fieldMap);
                return fieldMap;
            }
        }

        private string GetUnicodeLongText(int artifactId, FieldRef field)
        {
            Stream unicodeLongTextStream = ObjectManager.StreamUnicodeLongText(artifactId, field);
            using (StreamReader unicodeLongTextStreamReader = new StreamReader(unicodeLongTextStream))
            {
                return unicodeLongTextStreamReader.ReadToEnd();
            }
        }

        private IntegrationPointProfileDto ToDto(IntegrationPointProfile profile)
        {
            var dto = new IntegrationPointProfileDto
            {
                ArtifactId = profile.ArtifactId,
                Name = profile.Name,
                SelectedOverwrite = profile.OverwriteFields == null ? string.Empty : profile.OverwriteFields.Name,
                SourceProvider = profile.SourceProvider.GetValueOrDefault(0),
                DestinationConfiguration = profile.DestinationConfiguration,
                SourceConfiguration = profile.SourceConfiguration,
                DestinationProvider = profile.DestinationProvider.GetValueOrDefault(0),
                Type = profile.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(profile.EnableScheduler.GetValueOrDefault(false), profile.ScheduleRule),
                EmailNotificationRecipients = profile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = profile.LogErrors.GetValueOrDefault(false),
                FieldMappings = Serializer.Deserialize<List<FieldMap>>(profile.FieldMappings ?? string.Empty),
            };
            SanitizeFieldsMapping(dto.FieldMappings);
            return dto;
        }

        private IntegrationPointProfile ToRdo(IntegrationPointProfileDto dto, IEnumerable<ChoiceRef> choices, PeriodicScheduleRule rule)
        {
            ChoiceRef choice = choices.FirstOrDefault(x => x.Name.Equals(dto.SelectedOverwrite));
            if (choice == null)
            {
                throw new Exception("Cannot find choice by the name " + dto.SelectedOverwrite);
            }
            var profile = new IntegrationPointProfile
            {
                ArtifactId = dto.ArtifactId,
                Name = dto.Name,
                OverwriteFields = new ChoiceRef(choice.ArtifactID) {Name = choice.Name},
                SourceConfiguration = dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                DestinationConfiguration = dto.DestinationConfiguration,
                FieldMappings = Serializer.Serialize(dto.FieldMappings ?? new List<FieldMap>()),
                EnableScheduler = dto.Scheduler.EnableScheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                EmailNotificationRecipients = string.Join("; ",
                    (dto.EmailNotificationRecipients ?? string.Empty).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList())
            };

            if (profile.EnableScheduler.GetValueOrDefault(false))
            {
                profile.ScheduleRule = rule.ToSerializedString();
                profile.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
            }
            else
            {
                profile.ScheduleRule = string.Empty;
                profile.NextScheduledRuntimeUTC = null;
            }

            return profile;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public class IntegrationPointProfileService : IntegrationPointServiceBase, IIntegrationPointProfileService
    {
        private readonly ILogger<IntegrationPointProfileService> _logger;
        private readonly IRetryHandler _retryHandler;

        public IntegrationPointProfileService(
            ICaseServiceContext context,
            ISerializer serializer,
            IChoiceQuery choiceQuery,
            IManagerFactory managerFactory,
            IValidationExecutor validationExecutor,
            IRelativityObjectManager objectManager,
            IRetryHandler retryHandler,
            ILogger<IntegrationPointProfileService> logger)
            : base(
                context,
                choiceQuery,
                serializer,
                managerFactory,
                validationExecutor,
                objectManager)
        {
            _logger = logger;
            _retryHandler = retryHandler;
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

        public IntegrationPointProfileSlimDto ReadSlim(int artifactId)
        {
            try
            {
                IntegrationPointProfile profile = ObjectManager.Read<IntegrationPointProfile>(artifactId);
                return ToSlim(profile);
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT_PROFILE, ex);
            }
        }

        public IList<IntegrationPointProfileSlimDto> ReadAllSlim()
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
            return profiles.Select(ToSlim).ToList();
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
            List<IntegrationPointProfileDto> dtoList = profiles.Select(ToDto).ToList();

            foreach (var dto in dtoList)
            {
                dto.FieldMappings = GetFieldMappings(dto.ArtifactId);
                dto.SourceConfiguration = GetSourceConfiguration(dto.ArtifactId);
                dto.DestinationConfiguration = GetDestinationConfiguration(dto.ArtifactId);
            }

            return dtoList;
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

                // save RDO
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

        private string GetSourceConfiguration(int integrationPointProfileArtifactId)
        {
            return GetUnicodeLongText(integrationPointProfileArtifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid });
        }

        private DestinationConfiguration GetDestinationConfiguration(int artifactId)
        {
            return ReadLongTextWithRetries<DestinationConfiguration>(GetDestinationConfigurationString, artifactId);
        }

        private List<FieldMap> GetFieldMappings(int artifactId)
        {
            return ReadLongTextWithRetries<List<FieldMap>>(GetFieldsMappingString, artifactId);
        }

        private T ReadLongTextWithRetries<T>(Func<int, string> longTextAccessor, int integrationPointId)
        {
            return _retryHandler.Execute<T, RipSerializationException>(
                () =>
                {
                    string longTextString = longTextAccessor(integrationPointId);
                    return Serializer.Deserialize<T>(longTextString);
                },
                exception =>
                {
                    _logger.LogWarning(
                        exception,
                        "Unable to deserialize {fieldType} for integration point profile: {integrationPointId}. LongText value: {longText}. Operation will be retried.",
                        typeof(T),
                        integrationPointId,
                        exception.Value ?? string.Empty);
                });
        }

        private string GetDestinationConfigurationString(int artifactId)
        {
            return GetUnicodeLongText(artifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.DestinationConfigurationGuid });
        }

        private string GetFieldsMappingString(int artifactId)
        {
            return GetUnicodeLongText(artifactId, new FieldRef { Guid = IntegrationPointProfileFieldGuids.FieldMappingsGuid });
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
            // May have your attention please!
            // Long Text fields are intentionally not mapped here to avoid deserialization issues of truncated jsons
            return new IntegrationPointProfileDto
            {
                ArtifactId = profile.ArtifactId,
                Name = profile.Name,
                SelectedOverwrite = profile.OverwriteFields == null ? string.Empty : profile.OverwriteFields.Name,
                SourceProvider = profile.SourceProvider.GetValueOrDefault(0),
                DestinationProvider = profile.DestinationProvider.GetValueOrDefault(0),
                Type = profile.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(profile.EnableScheduler.GetValueOrDefault(false), profile.ScheduleRule),
                EmailNotificationRecipients = profile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = profile.LogErrors.GetValueOrDefault(false),
            };
        }

        private IntegrationPointProfileSlimDto ToSlim(IntegrationPointProfile profile)
        {
            return new IntegrationPointProfileSlimDto
            {
                ArtifactId = profile.ArtifactId,
                Name = profile.Name,
                SelectedOverwrite = profile.OverwriteFields == null ? string.Empty : profile.OverwriteFields.Name,
                SourceProvider = profile.SourceProvider.GetValueOrDefault(0),
                DestinationProvider = profile.DestinationProvider.GetValueOrDefault(0),
                Type = profile.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(profile.EnableScheduler.GetValueOrDefault(false), profile.ScheduleRule),
                EmailNotificationRecipients = profile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = profile.LogErrors.GetValueOrDefault(false),
            };
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
                OverwriteFields = new ChoiceRef(choice.ArtifactID) {Name = choice.Name },
                SourceConfiguration = dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                DestinationConfiguration = Serializer.Serialize(dto.DestinationConfiguration ?? new DestinationConfiguration()),
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

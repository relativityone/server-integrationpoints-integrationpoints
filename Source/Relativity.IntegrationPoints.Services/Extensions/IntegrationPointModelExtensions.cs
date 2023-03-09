using AutoMapper;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services.Extensions
{
    public static class IntegrationPointModelExtensions
    {
        public static IntegrationPointDto ToCoreModel(this IntegrationPointModel model, string overwriteFieldsName)
        {
            var result = new IntegrationPointDto();
            result.SetProperties(model, overwriteFieldsName);
            if (model.SecuredConfiguration != null)
            {
                result.SecuredConfiguration = JsonConvert.SerializeObject(model.SecuredConfiguration);
            }

            if (model.ImportFileCopyMode != null)
            {
                SetImportFileCopyMode(result, model.ImportFileCopyMode);
            }

            return result;
        }

        public static IntegrationPointProfileDto ToCoreProfileModel(this IntegrationPointModel model, string overwriteFieldsName)
        {
            var result = new IntegrationPointProfileDto();
            result.SetProperties(model, overwriteFieldsName);
            return result;
        }

        private static void SetProperties(this IntegrationPointDtoBase dtoBase, IntegrationPointModel model, string overwriteFieldsName)
        {
            Mapper.Map(model, dtoBase);

            // FieldMappings and should be now properly mapped by AutoMapper
            dtoBase.SourceConfiguration = JsonConvert.SerializeObject(model.SourceConfiguration);
            dtoBase.DestinationConfiguration = JsonConvert.DeserializeObject<ImportSettings>(JsonConvert.SerializeObject(model.DestinationConfiguration));
            dtoBase.EmailNotificationRecipients = model.EmailNotificationRecipients;
            dtoBase.Scheduler = Mapper.Map<Scheduler>(model.ScheduleRule);
            dtoBase.SelectedOverwrite = overwriteFieldsName;
        }

        private static void SetImportFileCopyMode(IntegrationPointDto dto, ImportFileCopyModeEnum? modelImportFileCopyMode)
        {
            switch (modelImportFileCopyMode)
            {
                case ImportFileCopyModeEnum.DoNotImportNativeFiles:
                {
                    UpdateImportFileCopyModeConfiguration(dto, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, false);
                    break;
                }
                case ImportFileCopyModeEnum.SetFileLinks:
                {
                    UpdateImportFileCopyModeConfiguration(dto, ImportNativeFileCopyModeEnum.SetFileLinks, true);
                    break;
                }
                case ImportFileCopyModeEnum.CopyFiles:
                {
                    UpdateImportFileCopyModeConfiguration(dto, ImportNativeFileCopyModeEnum.CopyFiles, true);
                    break;
                }
            }
        }

        private static void UpdateImportFileCopyModeConfiguration(
            IntegrationPointDto dto,
            ImportNativeFileCopyModeEnum fileCopyMode,
            bool importFile)
        {
            dto.DestinationConfiguration.ImportNativeFileCopyMode = fileCopyMode;
            dto.DestinationConfiguration.ImportNativeFile = importFile;
        }
    }
}

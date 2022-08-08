using System.Collections.Generic;
using AutoMapper;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services.Extensions
{
    public static class IntegrationPointModelExtensions
    {
        public static kCura.IntegrationPoints.Core.Models.IntegrationPointModel ToCoreModel(this IntegrationPointModel model, string overwriteFieldsName)
        {
            var result = new kCura.IntegrationPoints.Core.Models.IntegrationPointModel();
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

        public static IntegrationPointProfileModel ToCoreProfileModel(this IntegrationPointModel model, string overwriteFieldsName)
        {
            var result = new IntegrationPointProfileModel();
            result.SetProperties(model, overwriteFieldsName);
            return result;
        }

        private static void SetProperties(this IntegrationPointModelBase modelBase, IntegrationPointModel model, string overwriteFieldsName)
        {
            Mapper.Map(model, modelBase);
            modelBase.SourceConfiguration = JsonConvert.SerializeObject(model.SourceConfiguration);
            modelBase.Destination = JsonConvert.SerializeObject(model.DestinationConfiguration);
            modelBase.Map = JsonConvert.SerializeObject(model.FieldMappings);
            modelBase.NotificationEmails = model.EmailNotificationRecipients;
            modelBase.Scheduler = Mapper.Map<Scheduler>(model.ScheduleRule);
            modelBase.SelectedOverwrite = overwriteFieldsName;
        }

        private static void SetImportFileCopyMode(kCura.IntegrationPoints.Core.Models.IntegrationPointModel model, ImportFileCopyModeEnum? modelImportFileCopyMode)
        {
            switch (modelImportFileCopyMode)
            {
                case ImportFileCopyModeEnum.DoNotImportNativeFiles:
                {
                    UpdateImportFileCopyModeConfiguration(model, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, false);
                    break;
                }
                case ImportFileCopyModeEnum.SetFileLinks:
                {
                    UpdateImportFileCopyModeConfiguration(model, ImportNativeFileCopyModeEnum.SetFileLinks, true);
                    break;
                }
                case ImportFileCopyModeEnum.CopyFiles:
                {
                    UpdateImportFileCopyModeConfiguration(model, ImportNativeFileCopyModeEnum.CopyFiles, true);
                    break;
                }
            }
        }

        private static void UpdateImportFileCopyModeConfiguration(
            kCura.IntegrationPoints.Core.Models.IntegrationPointModel model,
            ImportNativeFileCopyModeEnum fileCopyMode, bool importFile)
        {
            model.Destination = JsonUtils.AddOrUpdatePropertyValues(model.Destination,
                new Dictionary<string, object>
                {
                    { nameof(ImportSettings.ImportNativeFileCopyMode), fileCopyMode.ToString() },
                    { nameof(ImportSettings.ImportNativeFile), importFile }
                });
        }
    }
}
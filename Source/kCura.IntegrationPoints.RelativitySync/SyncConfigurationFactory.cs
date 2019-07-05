using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class SyncConfigurationFactory
	{
		public static SyncConfiguration Create(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger)
		{
			ISerializer serializer;
			try
			{
				serializer = ripContainer.Resolve<ISerializer>();
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to resolve dependencies from container.");
				throw;
			}
			
			try
			{
				SourceConfiguration sourceConfiguration = serializer.Deserialize<SourceConfiguration>(job.IntegrationPointModel.SourceConfiguration);
				ImportSettings destinationConfiguration = serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);
				List<string> emailRecipients = IntegrationPointTaskBase.GetRecipientEmails(job.IntegrationPointModel, logger);

				ImportSettingsDto importSettingsDto = new ImportSettingsDto()
				{
					RelativityWebServiceUrl = new Uri(Config.Config.Instance.WebApiPath),
					CaseArtifactId = destinationConfiguration.CaseArtifactId,
					CopyFilesToDocumentRepository = destinationConfiguration.CopyFilesToDocumentRepository,
					DestinationFolderArtifactId = destinationConfiguration.DestinationFolderArtifactId,
					DisableNativeLocationValidation = destinationConfiguration.DisableNativeLocationValidation,
					DisableNativeValidation = destinationConfiguration.DisableNativeValidation,
					ErrorFilePath = destinationConfiguration.ErrorFilePath,
					ExtractedTextFieldContainsFilePath = destinationConfiguration.ExtractedTextFieldContainsFilePath,
					ExtractedTextFileEncoding = destinationConfiguration.ExtractedTextFileEncoding,
					FileSizeMapped = destinationConfiguration.FileSizeMapped,
					FieldOverlayBehavior = _fieldOverlayBehaviors[destinationConfiguration.ImportOverlayBehavior],
					ImportNativeFileCopyMode = (ImportNativeFileCopyMode)destinationConfiguration.ImportNativeFileCopyMode,
					ImportOverwriteMode = (ImportOverwriteMode)destinationConfiguration.ImportOverwriteMode,
					LoadImportedFullTextFromServer = destinationConfiguration.LoadImportedFullTextFromServer,
					MoveExistingDocuments = destinationConfiguration.MoveExistingDocuments,
					OiFileIdMapped = destinationConfiguration.OIFileIdMapped,
					ParentObjectIdSourceFieldName = destinationConfiguration.ParentObjectIdSourceFieldName
				};

				foreach (int artifactID in destinationConfiguration.ObjectFieldIdListContainsArtifactId)
				{
					importSettingsDto.ObjectFieldIdListContainsArtifactId.Add(artifactID);
				}

				return new SyncConfiguration(job.SubmittedById, sourceConfiguration, destinationConfiguration, emailRecipients, importSettingsDto);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to deserialize integration point configuration.");
				throw;
			}
		}

		private static readonly Dictionary<ImportOverlayBehaviorEnum, FieldOverlayBehavior> _fieldOverlayBehaviors = new Dictionary<ImportOverlayBehaviorEnum, FieldOverlayBehavior>()
		{
			{ ImportOverlayBehaviorEnum.UseRelativityDefaults, FieldOverlayBehavior.UseFieldSettings },
			{ ImportOverlayBehaviorEnum.MergeAll, FieldOverlayBehavior.MergeValues },
			{ ImportOverlayBehaviorEnum.ReplaceAll, FieldOverlayBehavior.ReplaceValues }
		};

	}
}
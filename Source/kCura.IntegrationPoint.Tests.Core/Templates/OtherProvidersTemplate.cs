using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	public abstract class OtherProvidersTemplate : SourceProviderTemplate
	{
		protected SourceProvider LdapProvider;

		public OtherProvidersTemplate(string workspaceName, string workspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
			: base(workspaceName, workspaceTemplate)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
		}

		#region Helper Methods

		private LDAPSettings CreateDefaultLdapSettings()
		{
			LDAPSettings ldapSettings = new LDAPSettings()
			{
				ConnectionPath = SharedVariables.LdapConnectionPath,
				ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind
			};

			return ldapSettings;
		}

		private string CreateDefaultLdapSourceConfig()
		{

			LDAPSettings defaultLdapSettings = CreateDefaultLdapSettings();

			string serializedLdapSettings = JsonConvert.SerializeObject(defaultLdapSettings);

			return serializedLdapSettings;
		}

		private string CreateDefaultLdapDestinationConfig(int artifactTypeId = (int)ArtifactType.Document, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOnly)
		{
			ImportSettings destinationConfig = new ImportSettings
			{
				ArtifactTypeId = artifactTypeId,
				DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
				CaseArtifactId = WorkspaceArtifactId,
				ImportOverwriteMode = overwriteMode,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				CustodianManagerFieldContainsLink = true,
				FieldOverlayBehavior = "Use Field Settings"
			};
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}

		private string CreateDefaultLdapFieldMap()
		{
			FieldMap[] map = GetDefaultLdapFieldMap();
			return Container.Resolve<ISerializer>().Serialize(map);
		}

		private FieldMap[] GetDefaultLdapFieldMap()
		{
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldQueryRepository workspaceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(WorkspaceArtifactId);

			ArtifactDTO documentDto = workspaceFieldQueryRepository.RetrieveTheIdentifierField((int)Relativity.Client.ArtifactType.Document);

			FieldMap[] map =
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						FieldIdentifier = "objectguid",
						DisplayName = "objectguid",
						IsIdentifier = false
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = documentDto.ArtifactId.ToString(),
						DisplayName = documentDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true
					},
				}
			};
			return map;
		}

		protected IntegrationPointModel CreateDefaultLdapIntegrationModel(string name,
			Scheduler scheduler = null,
			ImportOverwriteModeEnum overwrite = ImportOverwriteModeEnum.AppendOnly)
		{
			IntegrationPointModel model = new IntegrationPointModel()
			{
				SourceProvider = LdapProvider.ArtifactId,
				Name = name,
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceConfiguration = CreateDefaultLdapSourceConfig(),
				Destination = CreateDefaultLdapDestinationConfig((int)ArtifactType.Document, overwrite),
				Map = CreateDefaultLdapFieldMap(),
				Scheduler = scheduler ?? new Scheduler { EnableScheduler = false },
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid).ArtifactId
			};

			switch (overwrite)
			{
				case ImportOverwriteModeEnum.AppendOnly:
					model.SelectedOverwrite = "Append Only";
					break;
				case ImportOverwriteModeEnum.AppendOverlay:
					model.SelectedOverwrite = "Append/Overlay";
					break;
				case ImportOverwriteModeEnum.OverlayOnly:
					model.SelectedOverwrite = "Overlay Only";
					break;
			}

			return model;
		}

		#endregion Helper Methods
	}
}
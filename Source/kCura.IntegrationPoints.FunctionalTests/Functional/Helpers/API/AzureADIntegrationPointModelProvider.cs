using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    public class AzureADIntegrationPointModelProvider
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly ICommonIntegrationPointDataService _commonDataSvc;

        private readonly Workspace _sourceWorkspace;

        public AzureADIntegrationPointModelProvider(IKeplerServiceFactory serviceFactory, Workspace sourceWorkspace)
        {
            _serviceFactory = serviceFactory;

            _sourceWorkspace = sourceWorkspace;

            _commonDataSvc = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
        }

        public async Task<IntegrationPointModel> ImportAzureADModel()
        {
            var entityType = await GetEntityTypeAsync().ConfigureAwait(false);

            return new IntegrationPointModel()
            {
                Name = nameof(ImportAzureADModel),
                SourceProvider = await _commonDataSvc.GetSourceProviderIdAsync(GlobalConst.AAD._AZURE_AD_PROVIDER_IDENTIFIER).ConfigureAwait(false),
                DestinationProvider = await _commonDataSvc.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                FieldMappings = GetAzureADFieldsMapping(entityType.Fields),
                Type = await _commonDataSvc.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ImportName).ConfigureAwait(false),
                OverwriteFieldsChoiceId = await _commonDataSvc.GetOverwriteFieldsChoiceIdAsync(ImportOverwriteModeEnum.AppendOnly).ConfigureAwait(false),
                SecuredConfiguration = new
                {
                    applicationID = GlobalConst.AAD._APPLICATION_ID,
                    password = GlobalConst.AAD._PASSWORD,
                    domain = GlobalConst.AAD._DOMAIN,
                    filterBy = "User",
                    filter = "",
                    msgraphVersion = ""
                },
                SourceConfiguration = new { },
                DestinationConfiguration = new
                {
                    ArtifactTypeId = entityType.ObjectType,
                    DestinationProviderType = DestinationProviders.RELATIVITY,
                    EntityManagerFieldContainsLink = true,
                    CaseArtifactId = _sourceWorkspace.ArtifactID,
                    ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay.ToString(),
                    FieldOverlayBehavior = "Use Field Settings",
                },
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
        }

        private List<FieldMap> GetAzureADFieldsMapping(List<RelativityObject> entityFields)
        {
            FieldEntry GetFieldEntry(RelativityObject field, string type, bool isIdentifier = false)
                => new FieldEntry
                    {
                        DisplayName = field.Name,
                        FieldIdentifier = field.ArtifactID.ToString(),
                        IsIdentifier = isIdentifier,
                        IsRequired = isIdentifier,
                        Type = type
                    };

            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = new FieldEntry { DisplayName = GlobalConst.AAD.Fields._ID, FieldIdentifier = GlobalConst.AAD.Fields._ID, Type = GlobalConst.AAD.Fields._TEXT_FIELD_TYPE, IsIdentifier = true, IsRequired = true },
                    DestinationField = GetFieldEntry(entityFields.Single(x => x.Name == "UniqueID"), "Fixed-Length Text", true),
                    FieldMapType = FieldMapType.Identifier
                },
                new FieldMap
                {
                    SourceField = new FieldEntry { DisplayName = GlobalConst.AAD.Fields._FIRST_NAME, FieldIdentifier = GlobalConst.AAD.Fields._FIRST_NAME_ID, Type = GlobalConst.AAD.Fields._TEXT_FIELD_TYPE },
                    DestinationField = GetFieldEntry(entityFields.Single(x => x.Name == "First Name"), "Fixed-Length Text")
                },
            };
        }

        private async Task<(List<RelativityObject> Fields, int ObjectType)> GetEntityTypeAsync()
        {
            using (IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest objectTypeRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.ObjectType },
                    Condition = $"'Name' == 'Entity'",
                    Fields = new[] { new FieldRef { Name = "Artifact Type ID" } }
                };

                RelativityObjectSlim objectType = await objectManager.QuerySingleSlimAsync(_sourceWorkspace.ArtifactID, objectTypeRequest);

                QueryRequest fieldsInTypeRequest = new QueryRequest
                {
                    Condition = $"'FieldArtifactTypeID' == {objectType.Values[0]}",
                    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Field },
                    Fields = new[] { new FieldRef { Name = "Display Name" } },
                    IncludeNameInQueryResult = true,
                };

                var queryResult = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, fieldsInTypeRequest, 0, int.MaxValue).ConfigureAwait(false);

                return (
                    Fields: queryResult.Objects,
                    ObjectType: (int)objectType.Values[0]
                );
            }
        }
    }
}

using kCura.IntegrationPoints.Data.Repositories.Implementations.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.Services.FieldManager;
using System;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class FieldRepository : IFieldRepository
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly int _workspaceArtifactId;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
        private readonly IFoundationRepositoryFactory _foundationRepositoryFactory;
        private readonly IAPILog _logger;

        public FieldRepository(
            IServicesMgr servicesMgr, 
            IHelper helper,
            IFoundationRepositoryFactory foundationRepositoryFactory,
            IExternalServiceInstrumentationProvider instrumentationProvider, 
            int workspaceArtifactId)
        {
            _instrumentationProvider = instrumentationProvider;
            _servicesMgr = servicesMgr;
            _workspaceArtifactId = workspaceArtifactId;
            _foundationRepositoryFactory = foundationRepositoryFactory;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<FieldRepository>();
        }

        public void UpdateFilterType(int artifactViewFieldId, string filterType)
        {
            using (IFieldManager fieldManagerProxy =
                _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
            {
                fieldManagerProxy.UpdateFilterTypeAsync(_workspaceArtifactId, artifactViewFieldId, filterType);
            }
        }

        public void SetOverlayBehavior(int fieldArtifactId, bool overlayBehavior)
        {
            using (IFieldManager fieldManagerProxy =
                _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
            {
                fieldManagerProxy.SetOverlayBehaviorAsync(_workspaceArtifactId, fieldArtifactId, overlayBehavior);
            }
        }

        public int CreateMultiObjectFieldOnDocument(string name, int associatedObjectTypeDescriptorId)
        {

            try
            {
                using (var fieldManager = _servicesMgr.CreateProxy<global::Relativity.Services.Interfaces.Field.IFieldManager>(ExecutionIdentity.CurrentUser))
                {
                    int artifactId = fieldManager.CreateMultipleObjectFieldAsync(_workspaceArtifactId, new MultipleObjectFieldRequest()
                    {
                        ObjectType = new ObjectTypeIdentifier()
                        {
                            ArtifactTypeID = (int) ArtifactType.Document
                        },
                        AssociativeObjectType = new ObjectTypeIdentifier()
                        {
                            ArtifactTypeID = associatedObjectTypeDescriptorId
                        },
                        Name = name,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        AvailableInFieldTree = false,
                        IsRequired = false,
                        Width = 100
                    }).GetAwaiter().GetResult();

                    return artifactId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MultiObject field on Document for object {name}.", name);
                throw;
            }
        }

        public int CreateObjectTypeField(BaseFieldRequest field)
        {
            try
            {
                using (var fieldManager = _servicesMgr.CreateProxy<global::Relativity.Services.Interfaces.Field.IFieldManager>(ExecutionIdentity.CurrentUser))
                {
                    if (field is WholeNumberFieldRequest wholeNumberFieldRequest)
                    {
                        return fieldManager.CreateWholeNumberFieldAsync(_workspaceArtifactId, wholeNumberFieldRequest).GetAwaiter().GetResult();
                    }
                    else if (field is FixedLengthFieldRequest fixedLengthFieldRequest)
                    {
                        return fieldManager.CreateFixedLengthFieldAsync(_workspaceArtifactId, fixedLengthFieldRequest).GetAwaiter().GetResult();
                    }
                    else
                    {
                        throw new IntegrationPointsException($"Cannot create field of unsupported type: {field.GetType()}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create fields: {message}.", ex.Message);
                throw new Exception($"Failed to create fields: {ex.Message}.", ex);
            }
        }

        public IField Read(int fieldArtifactId)
        {
            global::Relativity.API.Foundation.Repositories.IFieldRepository fieldRepository = CreateFieldRepository();
            return ReadFieldFromRepository(fieldArtifactId, fieldRepository);
        }

        private global::Relativity.API.Foundation.Repositories.IFieldRepository CreateFieldRepository()
        {
            try
            {
                global::Relativity.API.Foundation.Repositories.IFieldRepository fieldRepository =
                    _foundationRepositoryFactory.GetRepository<global::Relativity.API.Foundation.Repositories.IFieldRepository>(_workspaceArtifactId);
                return fieldRepository;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating field repository for workspace: {workspaceId}", _workspaceArtifactId);
                throw new IntegrationPointsException($"An error occured while creating field repository for workspace: {_workspaceArtifactId}", ex);
            }
        }

        private IField ReadFieldFromRepository(int fieldArtifactId, global::Relativity.API.Foundation.Repositories.IFieldRepository fieldRepository)
        {
            IArtifactRef artifactRef = new ArtifactRef
            {
                ArtifactID = fieldArtifactId
            };
            IExternalServiceInstrumentationStarted instrumentation = _instrumentationProvider
                .Create(ExternalServiceTypes.API_FOUNDATION, nameof(IFieldRepository), nameof(IFieldRepository.Read))
                .Started();
            try
            {
                IField result = fieldRepository.Read(artifactRef);
                instrumentation.Completed();
                return result;
            }
            catch (Exception ex)
            {
                instrumentation.Failed(ex);
                throw new IntegrationPointsException($"An error occured while reading field {fieldArtifactId} from workspace {_workspaceArtifactId}", ex);
            }
        }
    }
}

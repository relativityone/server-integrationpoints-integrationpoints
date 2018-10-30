using kCura.IntegrationPoints.Data.Repositories.Implementations.DTO;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.Services.FieldManager;
using System;
using System.Linq;
using Field = kCura.Relativity.Client.DTOs.Field;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldRepository : IFieldRepository
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;
		private readonly IRsapiClientFactory _rsapiClientFactory;
		private readonly IHelper _helper;
		public FieldRepository(IServicesMgr servicesMgr, IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_servicesMgr = servicesMgr;
			_workspaceArtifactId = workspaceArtifactId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<FieldRepository>();
			_rsapiClientFactory = new RsapiClientFactory();
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
			var documentObjectType = new ObjectType { DescriptorArtifactTypeID = 10 };
			var associatedObjectType = new ObjectType { DescriptorArtifactTypeID = associatedObjectTypeDescriptorId };

			var field = new Field
			{
				Name = name,
				FieldTypeID = FieldType.MultipleObject,
				ObjectType = documentObjectType,
				AssociativeObjectType = associatedObjectType,
				AllowGroupBy = false,
				AllowPivot = false,
				AvailableInFieldTree = false,
				IsRequired = false,
				Width = "100"
			};

			using (var rsapiClient = _rsapiClientFactory.CreateUserClient(_servicesMgr, _logger))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					return rsapiClient.Repositories.Field.CreateSingle(field);
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Failed to create MultiObject field on Document for object {name}.", name);
					throw;
				}
			}
		}

		public int CreateObjectTypeField(Field field)
		{
			using (var proxy = _rsapiClientFactory.CreateUserClient(_servicesMgr, _logger))
			{
				proxy.APIOptions.WorkspaceID = _workspaceArtifactId;

				var createResult = proxy.Repositories.Field.Create(field);

				if (!createResult.Success)
				{
					_logger.LogError("Failed to create fields: {message}.", createResult.Message);
					throw new Exception($"Failed to create fields: {createResult.Message}.");
				}

				int newFieldId = createResult.Results.First().Artifact.ArtifactID;

				var readResult = proxy.Repositories.Field.Read(newFieldId);

				if (!readResult.Success)
				{
					_logger.LogError("Failed to create fields: {message}.", createResult.Message);
					proxy.Repositories.Field.Delete(newFieldId);
					throw new Exception($"Failed to create fields: {readResult.Message}.");
				}

				return newFieldId;
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
				IFoundationRepositoryFactory frf = new FoundationRepositoryFactory(_helper);
				global::Relativity.API.Foundation.Repositories.IFieldRepository fieldRepository =
					frf.GetRepository<global::Relativity.API.Foundation.Repositories.IFieldRepository>(
						_workspaceArtifactId);
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
			try
			{
				return fieldRepository.Read(artifactRef);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while reading field {fieldArtifactId} from workspace {workspaceArtifactId}", fieldArtifactId, _workspaceArtifactId);
				throw new IntegrationPointsException($"An error occured while reading field {fieldArtifactId} from workspace {_workspaceArtifactId}", ex);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.FieldManager;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldRepository : IFieldRepository
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public FieldRepository(IServicesMgr servicesMgr, IHelper helper, int workspaceArtifactId)
		{
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

		public List<Field> CreateObjectTypeFields(List<Field> fields)
		{
			using (var proxy = _rsapiClientFactory.CreateUserClient(_servicesMgr, _logger))
			{
				proxy.APIOptions.WorkspaceID = _workspaceArtifactId;

				var createResult = proxy.Repositories.Field.Create(fields);

				if (!createResult.Success)
				{
					_logger.LogError("Failed to create fields: {message}.", createResult.Message);
					throw new Exception($"Failed to create fields: {createResult.Message}.");
				}

				List<int> newFieldsIds = createResult.Results.Select(x => x.Artifact.ArtifactID).ToList();

				var readResult = proxy.Repositories.Field.Read(newFieldsIds);

				if (!readResult.Success)
				{
					_logger.LogError("Failed to create fields: {message}.", createResult.Message);
					proxy.Repositories.Field.Delete(newFieldsIds);
					throw new Exception($"Failed to create fields: {readResult.Message}.");
				}

				var newFields = readResult.Results.Select(x => x.Artifact).ToList();
				foreach (var fieldResult in newFields)
				{
					var fieldGuid = fields.First(x => x.Name == fieldResult.Name).Guids[0];
					fieldResult.Guids = new List<Guid> { fieldGuid };
				}

				return newFields;
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
	}
}
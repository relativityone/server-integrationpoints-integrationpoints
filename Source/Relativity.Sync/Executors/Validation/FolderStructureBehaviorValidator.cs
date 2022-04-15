using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class FolderStructureBehaviorValidator : IValidator
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IAPILog _logger;

		public FolderStructureBehaviorValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IAPILog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating folder structure behavior");

			ValidationResult result = new ValidationResult();

			if (configuration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				try
				{
					result = await ValidateFolderStructureBehaviorAsync(configuration, token).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					string message = "Exception occurred when validating folder structure behavior";
					_logger.LogError(ex, message);
					throw;
				}
			}

			return result;
		}

		public bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline();

		private async Task<ValidationResult> ValidateFolderStructureBehaviorAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				const string fieldType = "Field Type";
				const string longText = "Long Text";
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Name = "Field"
					},
					Condition = $"(('FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'Name' == '{configuration.GetFolderPathSourceFieldName()}'))",
					Fields = new[]
					{
						new FieldRef() {Name = fieldType},
					}
				};
				const int start = 0;
				const int length = 1;
				QueryResult queryResult = await objectManager.QueryAsync(configuration.SourceWorkspaceArtifactId, queryRequest, start, length, token,
					new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				if (queryResult.Objects.Count > 0)
				{
					string fieldTypeName = queryResult.Objects.Single()[fieldType].Value.ToString();
					if (longText != fieldTypeName)
					{
						result.Add($"Folder Path Source Field has invalid type: '{fieldTypeName}' but expected '{longText}'");
					}
				}
				else
				{
					result.Add($"Field Name: {configuration.GetFolderPathSourceFieldName()} not found.");
				}
			}

			return result;
		}
	}
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
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
                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        Name = "Field"
                    },
                    Condition = $"(('FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'Name' == '{configuration.GetFolderPathSourceFieldName()}'))",
                    Fields = new[]
                    {
                        new FieldRef() { Name = fieldType },
                    }
                };

                QueryResult queryResult = await objectManager.QueryAsync(
                        configuration.SourceWorkspaceArtifactId, queryRequest, 0, 1)
                    .ConfigureAwait(false);
                if (queryResult.Objects.Count > 0)
                {
                    const string longText = "Long Text";
                    const string fixedLengthText = "Fixed-Length Text";
                    string fieldTypeName = queryResult.Objects.Single()[fieldType].Value.ToString();
                    if (fieldTypeName != longText && fieldTypeName != fixedLengthText)
                    {
                        result.Add(
                            $"Folder Path Source Field has invalid type: '{fieldTypeName}', " +
                            $"but expected '{longText}' or '{fixedLengthText}'");
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

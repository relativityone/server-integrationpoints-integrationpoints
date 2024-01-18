using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Transfer
{
    internal sealed class LongTextFieldSanitizer : IExportFieldSanitizer
    {
        private const int _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
        private const string _BIG_LONG_TEXT_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";

        private readonly IImportStreamBuilder _importStreamBuilder;
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IRetriableStreamBuilderFactory _streamBuilderFactory;
        private readonly IAPILog _logger;

        public RelativityDataType SupportedType => RelativityDataType.LongText;

        public LongTextFieldSanitizer(
            ISourceServiceFactoryForUser serviceFactoryForUser,
            IRetriableStreamBuilderFactory streamBuilderFactory,
            IImportStreamBuilder importStreamBuilder,
            IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _streamBuilderFactory = streamBuilderFactory;
            _importStreamBuilder = importStreamBuilder;
            _logger = logger;
        }

        public async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue == null || initialValue is string)
            {
                return await SanitizeAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, sanitizingSourceFieldName, (string)initialValue).ConfigureAwait(false);
            }

            throw new ArgumentException($"Expected initial value to be string, instead was {initialValue.GetType().FullName}", nameof(initialValue));
        }

        private async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, string initialValue)
        {
            object value = initialValue;
            if (initialValue == _BIG_LONG_TEXT_SHIBBOLETH)
            {
                try
                {
                    value = await CreateLongTextStreamAsync(
                            workspaceArtifactId,
                            itemIdentifierSourceFieldName,
                            itemIdentifier,
                            sanitizingSourceFieldName)
                        .ConfigureAwait(false);
                }
                catch (SyncItemLevelErrorException ex)
                {
                    throw new SyncItemLevelErrorException($"Reading LongText field '{sanitizingSourceFieldName}' value failed: {ex.Message}", ex);
                }
            }

            return value;
        }

        private async Task<Stream> CreateLongTextStreamAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName)
        {
            int itemArtifactId = await GetItemArtifactIdAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier).ConfigureAwait(false);
            if (await IsFieldInUnicodeAsync(workspaceArtifactId, sanitizingSourceFieldName).ConfigureAwait(false))
            {
                return StreamLongText(workspaceArtifactId, itemArtifactId, sanitizingSourceFieldName, StreamEncoding.Unicode);
            }

            return StreamLongText(workspaceArtifactId, itemArtifactId, sanitizingSourceFieldName, StreamEncoding.ASCII);
        }

        private async Task<int> GetItemArtifactIdAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier)
        {
            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID },
                    Condition = $"'{itemIdentifierSourceFieldName}' == '{itemIdentifier}'"
                };
                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceArtifactId, request, 0, 1).ConfigureAwait(false);

                if (result.Objects == null | !result.Objects.Any())
                {
                    throw new SyncItemLevelErrorException($"Objects not found for itemIdentifier = {itemIdentifier}, itemIdentifierSourceFieldName = {itemIdentifierSourceFieldName}.");
                }

                return result.Objects[0].ArtifactID;
            }
        }

        private async Task<bool> IsFieldInUnicodeAsync(int workspaceArtifactId, string sanitizingSourceFieldName)
        {
            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                string conditionString = $"'Name' == '{sanitizingSourceFieldName}' AND 'Object Type Artifact Type ID' == {_DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID}";
                var request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Name = "Field" },
                    Condition = conditionString,
                    Fields = new[] { new FieldRef { Name = "Unicode" } }
                };
                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
                return (bool)result.Objects[0].Values[0];
            }
        }

        private Stream StreamLongText(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName, StreamEncoding encoding)
        {
            try
            {
                IRetriableStreamBuilder streamBuilder = _streamBuilderFactory.Create(workspaceArtifactId, relativityObjectArtifactId, fieldName);
                return _importStreamBuilder.Create(streamBuilder, encoding, relativityObjectArtifactId);
            }
            catch (Exception ex)
            {
                string message = GetStreamLongTextErrorMessage(workspaceArtifactId, relativityObjectArtifactId, fieldName, encoding);
                _logger.LogError(ex, message);
                throw;
            }
        }

        private static string GetStreamLongTextErrorMessage(
            int workspaceArtifactID,
            int relativityObjectArtifactId,
            string fieldName,
            StreamEncoding encoding)
        {
            var msgBuilder = new StringBuilder();
            msgBuilder.Append($"Error occurred when creating stream with {encoding} encoding. ");
            msgBuilder.Append($"Workspace: ({workspaceArtifactID}) ");
            msgBuilder.Append($"ExportObject artifact id: ({relativityObjectArtifactId}) ");
            msgBuilder.Append($"Long text field name: ({fieldName})");
            return msgBuilder.ToString();
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Transfer
{
	internal sealed class LongTextFieldSanitizer : IExportFieldSanitizer, IDisposable
	{
		private IObjectManager _objectManager;
		private const int _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
		private const string _BIG_LONG_TEXT_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";

		private readonly IImportStreamBuilder _importStreamBuilder;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public async Task<IObjectManager> GetObjectManager()
		{
			return _objectManager ?? (_objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false));
		}

		public RelativityDataType SupportedType => RelativityDataType.LongText;

		public LongTextFieldSanitizer(ISourceServiceFactoryForUser serviceFactory, IImportStreamBuilder importStreamBuilder, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
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
			if (initialValue == _BIG_LONG_TEXT_SHIBBOLETH)
			{
				return await CreateBigLongTextStreamAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, sanitizingSourceFieldName).ConfigureAwait(false);
			}
			return initialValue;
		}

		private async Task<Stream> CreateBigLongTextStreamAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName)
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
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID },
					Condition = $"'{itemIdentifierSourceFieldName}' == '{itemIdentifier}'"
				};
				QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
				return result.Objects[0].ArtifactID;
			}
		}

		private async Task<bool> IsFieldInUnicodeAsync(int workspaceArtifactId, string sanitizingSourceFieldName)
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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

		public Stream StreamLongText(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName, StreamEncoding encoding)
		{
			try
			{
				return _importStreamBuilder.Create(
					() => GetLongTextStreamAsync(workspaceArtifactId, relativityObjectArtifactId, fieldName).GetAwaiter().GetResult(),
					encoding);
			}
			catch (Exception ex)
			{
				string message = GetStreamLongTextErrorMessage(workspaceArtifactId, relativityObjectArtifactId, fieldName, encoding);
				_logger.LogError(ex, message);
				throw;
			}
		}

		private async Task<Stream> GetLongTextStreamAsync(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName)
		{
			var exportObject = new RelativityObjectRef { ArtifactID = relativityObjectArtifactId };
			var fieldRef = new FieldRef { Name = fieldName };
			IObjectManager objectManager = await GetObjectManager().ConfigureAwait(false);
			IKeplerStream keplerStream = await objectManager.StreamLongTextAsync(workspaceArtifactId, exportObject, fieldRef).ConfigureAwait(false);
			return await keplerStream.GetStreamAsync().ConfigureAwait(false);
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

		public void Dispose()
		{
			_objectManager?.Dispose();
			_objectManager = null;
		}
	}
}
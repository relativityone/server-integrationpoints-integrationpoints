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
	internal sealed class LongTextFieldSanitizer : IExportFieldSanitizer
	{
		private const string _BIG_LONG_TEXT_SHIBBOLETH = "#KCURA99DF2F0FEB88420388879F1282A55760#";
		private const int _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID = 10;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public RelativityDataType SupportedType => RelativityDataType.LongText;

		public LongTextFieldSanitizer(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			return await SanitizeAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, sanitizingSourceFieldName, (string) initialValue).ConfigureAwait(false);
		}

		private async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, string initialValue)
		{
			await Task.Yield();
			if (initialValue == _BIG_LONG_TEXT_SHIBBOLETH)
			{
				return CreateBigLongTextStreamAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, sanitizingSourceFieldName);
			}
			return initialValue;
		}

		private async Task<Stream> CreateBigLongTextStreamAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName)
		{
			int itemArtifactId = await GetItemArtifactIdAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier).ConfigureAwait(false);
			if (await IsFieldInUnicodeAsync(workspaceArtifactId, sanitizingSourceFieldName).ConfigureAwait(false))
			{
				return StreamUnicodeLongText(workspaceArtifactId, itemArtifactId, sanitizingSourceFieldName);
			}

			return StreamNonUnicodeLongText(workspaceArtifactId, itemArtifactId, sanitizingSourceFieldName);
		}

		private async Task<int> GetItemArtifactIdAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier)
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef{ ArtifactTypeID = _DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID},
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
				string conditionString = $"'Name'== '{sanitizingSourceFieldName}' AND 'Object Type Artifact Type ID' == {_DOCUMENT_OBJECT_TYPE_ARTIFACT_TYPE_ID}";
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef {Name = "Field"},
					Condition = conditionString,
					Fields = new[] {new FieldRef {Name = "Unicode"}}
				};
				QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
				return (bool)result.Objects[0].Values[0];
			}
		}

		public Stream StreamUnicodeLongText(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName)
		{
			try
			{
				var selfRecreatingStream = new SelfRecreatingStream(() => GetLongTextStreamAsync(workspaceArtifactId, relativityObjectArtifactId, fieldName).GetAwaiter().GetResult(), _logger);
				var selfDisposingStream = new SelfDisposingStream(selfRecreatingStream, _logger);
				return selfDisposingStream;
			}
			catch (Exception ex)
			{
				string message = GetStreamLongTextErrorMessage(nameof(StreamUnicodeLongText), workspaceArtifactId, relativityObjectArtifactId, fieldName);
				_logger.LogError(ex, message);
				throw;
			}
		}

		public Stream StreamNonUnicodeLongText(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName)
		{
			try
			{
				var selfRecreatingStream = new SelfRecreatingStream(() => GetLongTextStreamAsync(workspaceArtifactId, relativityObjectArtifactId, fieldName).GetAwaiter().GetResult(), _logger);
				var asciiToUnicodeStream = new AsciiToUnicodeStream(selfRecreatingStream);
				var selfDisposingStream = new SelfDisposingStream(asciiToUnicodeStream, _logger);
				return selfDisposingStream;
			}
			catch (Exception ex)
			{
				string message = GetStreamLongTextErrorMessage(nameof(StreamNonUnicodeLongText), workspaceArtifactId, relativityObjectArtifactId, fieldName);
				_logger.LogError(ex, message);
				throw;
			}
		}

		private async Task<Stream> GetLongTextStreamAsync(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName)
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var exportObject = new RelativityObjectRef() { ArtifactID = relativityObjectArtifactId };
				var fieldRef = new FieldRef {Name = fieldName};
				IKeplerStream keplerStream = await objectManager.StreamLongTextAsync(workspaceArtifactId, exportObject, fieldRef).ConfigureAwait(false);
				return await keplerStream.GetStreamAsync().ConfigureAwait(false);
			}
		}

		private static string GetStreamLongTextErrorMessage(
			string methodName,
			int workspaceArtifactID,
			int relativityObjectArtifactId,
			string fieldName)
		{
			var msgBuilder = new StringBuilder();
			msgBuilder.Append($"Error occurred when calling {methodName} method. ");
			msgBuilder.Append($"Workspace: ({workspaceArtifactID}) ");
			msgBuilder.Append($"ExportObject artifact id: ({relativityObjectArtifactId}) ");
			msgBuilder.Append($"Long text field name: ({fieldName})");
			return msgBuilder.ToString();
		}
	}
}
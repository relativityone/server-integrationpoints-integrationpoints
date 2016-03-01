using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldValueLoader _fieldsLoader;

		public DocumentTransferDataReader(
			IRDORepository rdoRepository,
			IEnumerable<int> documentArtifactIds,
			IEnumerable<FieldEntry> fieldEntries,
			QueryDataItemResult[] longTextfieldEntries) :
			base(rdoRepository, CreateQuery(documentArtifactIds, fieldEntries), GenerateDataColumnsFromFieldEntries(fieldEntries))
		{

			_longTextFieldArtifactIds = new HashSet<int>(longTextfieldEntries.Select(x => x.ArtifactId));

			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
			_fieldsLoader = new FieldValueLoader(
				rdoRepository, 
				longTextfieldEntries.Select(x => x.ArtifactId).ToArray(),
				documentArtifactIds.ToArray());
		}

		private static Query CreateQuery(IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries)
		{
			return new Query()
			{
				Condition = $"'{Shared.Constants.ARTIFACT_ID_FIELD_NAME}' in [{String.Join(",", documentArtifactIds.ToList())}]",
				Fields = fieldEntries.ToList().Select(x => x.DisplayName).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				Sorts = new[] { "'Artifact ID' ASC" },
				TruncateTextFields = false
			};
		}

		protected override ObjectQueryResutSet ExecuteQueryToGetInitialResult()
		{
			return RDORepository.RetrieveAsync(ObjectQuery, String.Empty).Result;
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(IEnumerable<FieldEntry> fieldEntries)
		{
			return fieldEntries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
		}

		public override string GetDataTypeName(int i)
		{
			return GetFieldType(i).ToString();
		}

		public override Type GetFieldType(int i)
		{
			object value = CurrentItemResult.Fields[i].Value;
			return value == null ? typeof(object) : value.GetType();
		}

		public override object GetValue(int i)
		{
			Object result = null;
			int fieldArtifactId = CurrentItemResult.Fields[i].ArtifactId;
			string fieldName = CurrentItemResult.Fields[i].Name;

			if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
			}
			else
			{
				result = CurrentItemResult.Fields[i].Value;
			}
			return result;
		}

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldFromPreLoadedCache(CurrentItemResult.ArtifactId, fieldArtifactId);
		}

		private int _localCacheId;
		private List<FieldValue> _localCache;
		private string GetLongTextFieldFromPreLoadedCache(int documentArtifactId, int fieldArtifactId)
		{
			if (documentArtifactId != _localCacheId)
			{
				Task<List<FieldValue>> getLongTextFieldsTasks = _fieldsLoader.GetFieldsValue(documentArtifactId);
				try
				{
					getLongTextFieldsTasks.Wait();

					if (getLongTextFieldsTasks.IsCompleted)
					{
						if (getLongTextFieldsTasks.Exception != null)
						{
							throw getLongTextFieldsTasks.Exception;
						}

						_localCacheId = documentArtifactId;
						_localCache = getLongTextFieldsTasks.Result;
					}
				}
				catch (Exception exception)
				{
					throw exception.InnerException ?? exception;
				}
			}
			return _localCache.First(field => field.ArtifactID == fieldArtifactId).ValueAsLongText;
		}
	}
}
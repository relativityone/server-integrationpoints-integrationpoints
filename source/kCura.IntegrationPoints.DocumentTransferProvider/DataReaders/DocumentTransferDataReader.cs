using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly int[] _documentArtifactIds;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
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
//			_documentArtifactIds = documentArtifactIds.ToArray();
//			_fieldEntries = fieldEntries.ToList();
//			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
//			_longTextFieldArtifactIds = new HashSet<int>(longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)));
//			_fieldsLoader = new FieldValueLoader(rdoRepository, longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)).ToArray(),
//				_documentArtifactIds);
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
				Sorts = new[] { "ArtifactID ASC" },
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
				result = LoadLongTextFieldValueOfCurrentDocument(fieldName);
			}
			else
			{
				result = CurrentItemResult.Fields[i].Value;
			}
			return result;
		}

		private String LoadLongTextFieldValueOfCurrentDocument(string fieldName)
		{
			return GetLongTextFieldValue(CurrentItemResult.ArtifactId, fieldName);
		}

		private String GetLongTextFieldValue(int documentArtifactId, string longTextFieldName)
		{
			var longTextQuery = new Query()
			{
				Condition = $"'ArtifactID' == {documentArtifactId}",
				Fields = new[] { longTextFieldName },
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResutSet results;
			try
			{
				results = RDORepository.RetrieveAsync(longTextQuery, String.Empty).Result;
			}
			catch (Exception e)
			{
				const string exceptionMessage = "Unable to read document of artifact id {0}. This may be due to the size of the field. Please reconfigure Relativity.Services' web.config to resolve the issue.";
				throw new ProviderReadDataException(String.Format(exceptionMessage, documentArtifactId), e)
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			var document = results.Data;
			if (results.Success == false || document == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a document object with artifact Id of {0}", documentArtifactId))
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			object extractedText = document.DataResults[0].Fields[0].Value;
			if (extractedText == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a long field with artifact Id of {0}", longTextFieldName))
				{
					Identifier = documentArtifactId.ToString()
				};
			}
			return extractedText as String;
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly int[] _documentArtifactIds;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldValueLoader _fieldsLoader;
		private readonly Dictionary<int, string> _nativeFileLocation;

		public DocumentTransferDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> longTextfieldEntries) :
			base(relativityClientAdaptor, GenerateDataColumnsFromFieldEntries(fieldEntries))
		{
			_documentArtifactIds = documentArtifactIds.ToArray();
			_fieldEntries = fieldEntries.ToList();
			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
			_longTextFieldArtifactIds = new HashSet<int>(longTextfieldEntries.Select(artifact => artifact.ArtifactID));
			_fieldsLoader = new FieldValueLoader(relativityClientAdaptor, longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)).ToArray(),
				_documentArtifactIds);

			_nativeFileLocation = _nativeFileLocation ?? new Dictionary<int, string>();
		}

		/// TEMP constructure, we will need to create kelper service to get the file locations.
		public DocumentTransferDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> longTextfieldEntries,
			IDBContext dbContext) :
			this(relativityClientAdaptor, documentArtifactIds, fieldEntries, longTextfieldEntries)
		{
			var helper = new DirectSqlCallHelper(dbContext);
			_nativeFileLocation = helper.GetFileLocation(_documentArtifactIds);
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(IEnumerable<FieldEntry> fieldEntries)
		{
			// we will always import this native file location
			List<FieldEntry> fields = fieldEntries.ToList();
			fields.Add(new FieldEntry()
			{
				DisplayName = "NATIVE_FILE_LOCATION_01",
				FieldIdentifier = Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
				FieldType = FieldType.String
			});
			return fields.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
		}

		protected override QueryResultSet<Document> ExecuteQueryToGetInitialResult()
		{
			var artifactIdSetCondition = new WholeNumberCondition
			{
				Field = Shared.Constants.ARTIFACT_ID_FIELD_NAME,
				Operator = NumericConditionEnum.In,
				Value = _documentArtifactIds.ToList()
			};

			// only query for non-full text fields at this time
			List<FieldValue> requestedFields = _fieldEntries.Where(field =>
			{
				int id = -1;
				bool success = Int32.TryParse(field.FieldIdentifier, out id);
				return (success && _longTextFieldArtifactIds.Contains(id) == false);
			} )
			.Select(x => new FieldValue() { ArtifactID = Convert.ToInt32(x.FieldIdentifier) }).ToList();

			Query<Document> query = new Query<Document>
			{
				Condition = artifactIdSetCondition,
				Fields = requestedFields
			};

			return RelativityClient.ExecuteDocumentQuery(query);
		}

		public override string GetDataTypeName(int i)
		{
			return GetFieldType(i).ToString();
		}

		public override Type GetFieldType(int i)
		{
			string columnName = GetName(i);
			return CurrentDocument[columnName] == null ? typeof(object) : CurrentDocument[columnName].GetType();
		}

		public override object GetValue(int i)
		{
			Object result = null;
			string name = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(name, out fieldArtifactId);

			if (success)
			{
				if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
				{
					result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
				}
				else
				{
					result = CurrentDocument[fieldArtifactId].Value;
				}
			}
			else if (name == Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
			{
				if (_nativeFileLocation.ContainsKey(CurrentDocument.ArtifactID))
				{
					result = _nativeFileLocation[CurrentDocument.ArtifactID];
				}
			}
			return result;
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

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldFromPreLoadedCache(CurrentDocument.ArtifactID, fieldArtifactId);
		}
	}
}
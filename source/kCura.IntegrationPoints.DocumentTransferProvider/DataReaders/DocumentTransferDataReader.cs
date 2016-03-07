using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.DocumentTransferProvider.Managers;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly IDocumentManager _documentManager;
		private readonly IEnumerable<int> _documentArtifactIds;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldValueLoader _fieldsLoader;
		private bool _queryWasRun = false;
		private readonly Dictionary<int, string> _nativeFileLocation;

		public DocumentTransferDataReader(
			IDocumentManager documentManager,
			IEnumerable<int> documentArtifactIds,
			IEnumerable<FieldEntry> fieldEntries,
			IEnumerable<int> longTextFieldIdEntries) :
			base(GenerateDataColumnsFromFieldEntries(fieldEntries))
		{
			var documentIds = documentArtifactIds as int[] ?? documentArtifactIds.ToArray();
			var longTextIds = longTextFieldIdEntries as int[] ?? longTextFieldIdEntries.ToArray();

			_documentManager = documentManager;
			_documentArtifactIds = documentIds;
			_fieldEntries = fieldEntries;

			_longTextFieldArtifactIds = new HashSet<int>(longTextIds);

			// TODO: the fields loader's responsibilities should be placed into the document manager class -- biedrzycki: Mar 1st, 2016
			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
			_fieldsLoader = new FieldValueLoader(
				_documentManager, 
				longTextIds,
				documentIds.ToArray());

			_nativeFileLocation = _nativeFileLocation ?? new Dictionary<int, string>();
		}

		/// TEMP constructure, we will need to create kelper service to get the file locations.
		public DocumentTransferDataReader(
			IDocumentManager documentManager,
			IEnumerable<int> documentArtifactIds, 
			IEnumerable<FieldEntry> fieldEntries,
			IEnumerable<int> longTextFieldIdEntries,
			IDBContext dbContext) :
			this(documentManager, documentArtifactIds, fieldEntries, longTextFieldIdEntries)
		{
			var helper = new DirectSqlCallHelper(dbContext);
			_nativeFileLocation = helper.GetFileLocation(documentArtifactIds.ToArray());
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			IEnumerable<FieldEntry> filteredFields = _fieldEntries.Where(x =>
			{
				int id = -1;
				bool success = Int32.TryParse(x.FieldIdentifier, out id);
				return success && !_longTextFieldArtifactIds.Contains(id);
			});

			HashSet<int> requestedFieldIds = new HashSet<int>(filteredFields.Select(x => Convert.ToInt32(x.FieldIdentifier)));

			ArtifactDTO[] results =  _documentManager.RetrieveDocuments(
				_documentArtifactIds,
				requestedFieldIds);

			_queryWasRun = true;

			return results;
		}

		protected override bool AllArtifactsFetched()
		{
			return _queryWasRun;
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

		public override string GetDataTypeName(int i)
		{
			return GetFieldType(i).ToString();
		}

		public override Type GetFieldType(int i)
		{
			string fieldIdentifier = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);
			object value = null;
			if (success)
			{
				value = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId).Value;
			}

			return value == null ? typeof(object) : value.GetType();
		}

		public override object GetValue(int i)
		{
			Object result = null;
			string fieldIdentifier = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);

			if (success)
			{
				if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
				{
					result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
				}
				else
				{
					result = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId).Value;
				}
			}
			else if (fieldIdentifier == Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
			{
				if (_nativeFileLocation.ContainsKey(CurrentArtifact.ArtifactId))
				{
					result = _nativeFileLocation[CurrentArtifact.ArtifactId];
				}
			}

			return result;
		}

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldFromPreLoadedCache(CurrentArtifact.ArtifactId, fieldArtifactId);
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
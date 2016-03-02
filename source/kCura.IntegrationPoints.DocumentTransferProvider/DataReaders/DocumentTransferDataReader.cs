using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.DocumentTransferProvider.Managers;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly IDocumentManager _documentManager;
		private readonly IEnumerable<int> _documentArtifactIds;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldValueLoader _fieldsLoader;
		private bool _previousRequestReturnedEmpty = false;

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
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] results =  _documentManager.RetrieveDocuments(
				_documentArtifactIds,
				new HashSet<int>(_fieldEntries.Select(x => Convert.ToInt32(x.FieldIdentifier))));

			_previousRequestReturnedEmpty = results == null || !results.Any();

			return results;
		}

		protected override bool AllArtifactsFetched()
		{
			return _previousRequestReturnedEmpty;
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
			object value = CurrentArtifact.Fields[i].Value;
			return value == null ? typeof(object) : value.GetType();
		}

		public override object GetValue(int i)
		{
			Object result = null;
			int fieldArtifactId = CurrentArtifact.Fields[i].ArtifactId;

			if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
			}
			else
			{
				result = CurrentArtifact.Fields[i].Value;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	internal class FieldValueLoader
	{
		private Dictionary<int, Task<List<FieldValue>>> _cache;
		private readonly List<FieldValue> _queryFields;
		private readonly object _lock;
		private readonly IRDORepository _rdoRepository;
		private readonly int[] _documentArtifactIds;
		private int _counter;

		internal FieldValueLoader(
			IRDORepository rdoRepository,
			int[] fieldIdentifiers,
			int[] documentArtifactIds)
		{
			_counter = 0;
			_lock = new object();
			_cache = new Dictionary<int, Task<List<FieldValue>>>();
			_rdoRepository = rdoRepository;
			_documentArtifactIds = documentArtifactIds;
			_queryFields = fieldIdentifiers.Select(id => new FieldValue(id)).ToList();

			StartRequestingLongText();
		}

		private void StartRequestingLongText()
		{
			// load ahead 20 documents' long text fields
			for (int count = 0; count < 20; count++)
			{
				LoadNextLongTextFieldsValuesIntoCache();
			}
		}

		public void LoadNextLongTextFieldsValuesIntoCache()
		{
			if (_counter < _documentArtifactIds.Length)
			{
				int docId = _documentArtifactIds[_counter];
				LoadLongTextFieldsValuesIntoCache(docId);
				_counter++;
			}
		}

		private void LoadLongTextFieldsValuesIntoCache(int documentArtifactId)
		{
			_cache[documentArtifactId] = LoadLongTextFieldsValues(documentArtifactId);
		}

		public Task<List<FieldValue>> GetFieldsValue(int documentArtifactId)
		{
			LoadNextLongTextFieldsValuesIntoCache();
			if (_cache.ContainsKey(documentArtifactId))
			{
				Task<List<FieldValue>> fields = _cache[documentArtifactId];
				_cache.Remove(documentArtifactId);
				return fields;
			}
			return LoadLongTextFieldsValues(documentArtifactId);
		} 

		private Task<List<FieldValue>> LoadLongTextFieldsValues(int documentArtifactId)
		{
			return Task.Run(() =>
			{
				// TODO: validate this query
				var documentQuery = new Query()
				{
					Condition = $"'Artifact ID' == {documentArtifactId}",
					Fields = _queryFields.Select(x => x.Name).ToArray(),
					TruncateTextFields = false
				};

				ObjectQueryResutSet results = null;
				try
				{
					lock (_lock)
					{
						results = _rdoRepository.RetrieveAsync(documentQuery, String.Empty).Result;
					}
				}
				catch (Exception e)
				{
					const string exceptionMessage =
						"Unable to read document of artifact id {0}. This may be due to the size of the field. Please reconfigure Relativity.Services' web.config to resolve the issue.";
					throw new ProviderReadDataException(String.Format(exceptionMessage, documentArtifactId), e)
					{
						Identifier = documentArtifactId.ToString()
					};
				}

				QueryDataItemResult document = results.Data.DataResults.FirstOrDefault();
				if (results.Success == false || document == null)
				{
					throw new ProviderReadDataException(String.Format("Unable to find a document object with artifact Id of {0}",
						documentArtifactId))
					{
						Identifier = documentArtifactId.ToString()
					};
				}

				return document.Fields.Select(x => new FieldValue(x.ArtifactId) {Value = x.Value}).ToList();
			});
		}
	}
}
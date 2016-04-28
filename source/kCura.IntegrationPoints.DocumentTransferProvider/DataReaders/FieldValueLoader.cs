﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	internal class FieldValueLoader
	{
		private readonly Dictionary<int, Task<FieldValue>> _cache;
		private readonly IDocumentRepository _documentRepository;
		private readonly int[] _documentArtifactIds;
		private readonly int[] _fieldsToQuery;
		private int _counter;

		internal FieldValueLoader(
			IDocumentRepository documentRepository,
			int[] documentArtifactIds,
			int field)
		{
			_counter = 0;
			_cache = new Dictionary<int, Task<FieldValue>>();
			_documentRepository = documentRepository;
			_documentArtifactIds = documentArtifactIds;
			_fieldsToQuery = new[] { field };
			StartRequestingLongText();
		}

		private void StartRequestingLongText()
		{
			// load ahead 20 documents' long text fields
			int requestCount = Math.Min(20, _documentArtifactIds.Length);
			for (int count = 0; count < requestCount; count++)
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

		public Task<FieldValue> GetFieldsValue(int documentArtifactId)
		{
			LoadNextLongTextFieldsValuesIntoCache();
			if (_cache.ContainsKey(documentArtifactId))
			{
				Task<FieldValue> fields = _cache[documentArtifactId];
				_cache.Remove(documentArtifactId);
				return fields;
			}
			return LoadLongTextFieldsValues(documentArtifactId);
		}

		private Task<FieldValue> LoadLongTextFieldsValues(int documentArtifactId)
		{
			return Task.Run(async () =>
			{
				ArtifactDTO document = null;
				try
				{
					document = await _documentRepository.RetrieveDocumentAsync(documentArtifactId, _fieldsToQuery);
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

				if (document == null)
				{
					throw new ProviderReadDataException(String.Format("Unable to find a document object with artifact Id of {0}", documentArtifactId))
					{
						Identifier = documentArtifactId.ToString()
					};
				}

				ArtifactFieldDTO field = document.Fields[0];
				return new FieldValue(field.ArtifactId) { Value = field.Value };
			});
		}
	}
}
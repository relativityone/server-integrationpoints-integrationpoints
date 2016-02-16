using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentTranfserDataReader : RelativityReaderBase
	{
		private readonly IEnumerable<int> _documentArtifactIds;
		private readonly DataTable _schemaDataTable;
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly IDictionary<string, string> _fieldIdToNameDictionary;
		private readonly HashSet<int> _longTextFieldsArtifactIds;

		public DocumentTranfserDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> documentFields) :
			base(relativityClientAdaptor)
		{
			_documentArtifactIds = documentArtifactIds;
			_fieldEntries = fieldEntries.ToList();

			_schemaDataTable = new DataTable();
			_schemaDataTable.Columns.AddRange(_fieldEntries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray());
			_fieldIdToNameDictionary = _fieldEntries.ToDictionary(
				x => x.FieldIdentifier,
				y => y.IsIdentifier ? y.DisplayName.Replace(Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, String.Empty) : y.DisplayName);

			_longTextFieldsArtifactIds =  new HashSet<int>(documentFields.Select(artifact => artifact.ArtifactID));
		}

		public override DataTable GetSchemaTable()
		{
			return _schemaDataTable;
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
			List<FieldValue> requestedFields = _fieldEntries.Where(field => _longTextFieldsArtifactIds.Contains(Convert.ToInt32(field.FieldIdentifier)) == false)
															.Select(x => new FieldValue() { ArtifactID = Convert.ToInt32(x.FieldIdentifier) }).ToList();

			Query<Document> query = new Query<Document>
			{
				Condition = artifactIdSetCondition,
				Fields = requestedFields
			};
			return RelativityClient.ExecuteDocumentQuery(query);
		}

		public override int FieldCount
		{
			get { return _schemaDataTable.Columns.Count; }
		}

		public override string GetDataTypeName(int i)
		{
			return _schemaDataTable.Columns[i].DataType.Name;
		}

		public override Type GetFieldType(int i)
		{
			return _schemaDataTable.Columns[i].DataType;
		}

		public override string GetName(int i)
		{
			return _schemaDataTable.Columns[i].ColumnName;
		}

		public override int GetOrdinal(string name)
		{
			return _schemaDataTable.Columns[name].Ordinal;
		}

		public override object GetValue(int i)
		{
			string fieldIdAsString = GetName(i);
			int fieldId = Convert.ToInt32(fieldIdAsString);

			Object result = null;
			if (_longTextFieldsArtifactIds.Contains(fieldId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldId);
			}
			else
			{
				string columnName = _fieldIdToNameDictionary[fieldIdAsString].Replace(Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, String.Empty);
				result = CurrentDocument[columnName].Value;
			}
			return result;
		}

		private String LoadLongTextFieldValueOfCurrentDocument(int fieldArtifactId)
		{
			return GetLongTextFieldValue(CurrentDocument.ArtifactID, fieldArtifactId);
		}

		private String GetLongTextFieldValue(int documentArtifactId, int longTextFieldArtifactId)
		{
			Document documentQuery = new Document(documentArtifactId)
			{
				Fields = new List<FieldValue>() { new FieldValue(longTextFieldArtifactId) }
			};

			ResultSet<Document> results = null;
			try
			{
				results = RelativityClient.ReadDocument(documentQuery);
			}
			catch (Exception e)
			{
				const string exceptionMessage = "Unable to read document of artifact id {0}. This may be due to the size of the field. Please reconfigure Relativity.Services' web.config to resolve the issue.";
				throw new ProviderReadDataException(String.Format(exceptionMessage, documentArtifactId), e)
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			var document = results.Results.FirstOrDefault();
			if (results.Success == false || document == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a document object with artifact Id of {0}", documentArtifactId))
				{
					Identifier = documentArtifactId.ToString()
				};
			}

			Document documentArtifact = document.Artifact;
			var extractedText = documentArtifact.Fields.FirstOrDefault();
			if (extractedText == null)
			{
				throw new ProviderReadDataException(String.Format("Unable to find a long field with artifact Id of {0}", longTextFieldArtifactId))
				{
					Identifier = documentArtifactId.ToString()
				};
			}
			return extractedText.ValueAsLongText;
		}
	}
}
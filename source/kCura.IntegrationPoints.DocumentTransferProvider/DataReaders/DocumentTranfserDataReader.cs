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
		private readonly IEnumerable<FieldEntry> _fieldEntries;
		private readonly HashSet<int> _longTextFieldArtifactIds;

		public DocumentTranfserDataReader(IRelativityClientAdaptor relativityClientAdaptor,
			IEnumerable<int> documentArtifactIds, IEnumerable<FieldEntry> fieldEntries,
			List<Relativity.Client.Artifact> longTextfieldEntries) :
			base(relativityClientAdaptor, GenerateDataColumnsFromFieldEntries(fieldEntries))
		{
			_documentArtifactIds = documentArtifactIds;
			_fieldEntries = fieldEntries.ToList();

			// From SynchronizerObjectBuilder, the existing framework assuming that the reader from get data will use artifact Id as the name of the column.
			_longTextFieldArtifactIds = new HashSet<int>(longTextfieldEntries.Select(artifact => Convert.ToInt32(artifact.ArtifactID)));
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(IEnumerable<FieldEntry> fieldEntries)
		{
			return fieldEntries.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
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
			List<FieldValue> requestedFields = _fieldEntries.Where(field => _longTextFieldArtifactIds.Contains(Convert.ToInt32(field.FieldIdentifier)) == false)
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
			int fieldArtifactId = Convert.ToInt32(GetName(i));

			if (_longTextFieldArtifactIds.Contains(fieldArtifactId))
			{
				result = LoadLongTextFieldValueOfCurrentDocument(fieldArtifactId);
			}
			else
			{
				result = CurrentDocument[fieldArtifactId].Value;
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
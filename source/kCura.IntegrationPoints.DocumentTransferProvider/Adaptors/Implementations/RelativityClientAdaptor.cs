using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.DocumentTransferProvider.Exceptions;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class RelativityClientAdaptor : IRelativityClientAdaptor
	{
		private readonly IRSAPIClient _rsapiClient;

		public RelativityClientAdaptor(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public ResultSet<Document> ExecuteDocumentQuery(Query<Document> query)
		{
			ResultSet<Document> results = _rsapiClient.Repositories.Document.Query(query);

			return results;
		}

		public String GetLongTextFieldValue(int documentArtifactId, int longTextFieldArtifactId)
		{
			Document documentQuery = new Document(documentArtifactId)
			{
				Fields = new List<FieldValue>() {new FieldValue(longTextFieldArtifactId)}
			};

			ResultSet<Document> results = null;
			try
			{
				results = _rsapiClient.Repositories.Document.Read(documentQuery);
			}
			catch (Exception e)
			{
				const string exceptionMessage = "Unable to read document of artifact id {0}. This may be due to the size of the field. Please reconfigure Relativity.Services' web.config to resolve the issue.";
				throw new ReadDataFromRelativityException(String.Format(exceptionMessage, documentArtifactId), e);
			}

			var document = results.Results.FirstOrDefault();
			if (document == null)
			{
				throw new ReadDataFromRelativityException(String.Format("Unable to find a document object with artifact Id of {0}", documentArtifactId));
			}

			Document documentArtifact = document.Artifact;
			var extractedText = documentArtifact.Fields.FirstOrDefault();
			if (extractedText == null)
			{
				throw new ReadDataFromRelativityException(String.Format("Unable to find a long field with artifact Id of {0}", longTextFieldArtifactId));
			}
			return extractedText.ValueAsLongText;
		}
	}
}
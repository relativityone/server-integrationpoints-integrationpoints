using System;
using System.Data;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentArtifactIdDataReader : RelativityReaderBase
	{
		private readonly int _savedSearchArtifactId;

		public DocumentArtifactIdDataReader(IRelativityClientAdaptor relativityClientAdaptor, int savedSearchArtifactId) :
			base(relativityClientAdaptor, new [] { new DataColumn(Shared.Constants.ARTIFACT_ID_FIELD_NAME) })
		{
			_savedSearchArtifactId = savedSearchArtifactId;
		}

		protected override QueryResultSet<Document> ExecuteQueryToGetInitialResult()
		{
			Query<Document> query = new Query<Document>
			{
				Condition = new SavedSearchCondition(_savedSearchArtifactId),
				Fields = FieldValue.NoFields // we only want the ArtifactId
			};
			return RelativityClient.ExecuteDocumentQuery(query);
		}

		public override string GetDataTypeName(int i)
		{
			return typeof (Int32).ToString();
		}

		public override Type GetFieldType(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return typeof(Int32);
		}

		public override object GetValue(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return CurrentDocument.ArtifactID;
		}
	}
}
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
			base(relativityClientAdaptor)
		{
			_savedSearchArtifactId = savedSearchArtifactId;
		}

		public override DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
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

		public override int FieldCount
		{
			get { return 1; }
		}

		public override string GetDataTypeName(int i)
		{
			throw new System.NotImplementedException();
		}

		public override Type GetFieldType(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return typeof(Int32);
		}

		public override string GetName(int i)
		{
			if (i != 0)
			{
				throw new IndexOutOfRangeException();
			}

			return Shared.Constants.ARTIFACT_ID_FIELD_NAME;
		}

		public override int GetOrdinal(string name)
		{
			if (name != Shared.Constants.ARTIFACT_ID_FIELD_NAME)
			{
				throw new IndexOutOfRangeException();
			}
			return 0;
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
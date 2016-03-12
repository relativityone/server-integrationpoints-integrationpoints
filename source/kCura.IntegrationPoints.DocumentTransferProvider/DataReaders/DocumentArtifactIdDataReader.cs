using System;
using System.Data;
using kCura.IntegrationPoints.DocumentTransferProvider.Managers;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentArtifactIdDataReader : RelativityReaderBase
	{
		private readonly ISavedSearchManager _savedSearchManager;

		public DocumentArtifactIdDataReader(ISavedSearchManager savedSearchManager) :
			base(new [] { new DataColumn(Shared.Constants.ARTIFACT_ID_FIELD_NAME) })
		{
			_savedSearchManager = savedSearchManager;
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] results = _savedSearchManager.RetrieveNext();

			return results;
		}

		protected override bool AllArtifactsFetched()
		{
			bool allDocumentsRetrieved = _savedSearchManager.AllDocumentsRetrieved();

			return allDocumentsRetrieved;
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
			return CurrentArtifact.ArtifactId;
		}
	}
}
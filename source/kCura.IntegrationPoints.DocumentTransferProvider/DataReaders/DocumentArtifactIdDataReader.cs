using System;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentArtifactIdDataReader : RelativityReaderBase
	{
		private readonly ISavedSearchRepository _savedSearchRepository;

		public DocumentArtifactIdDataReader(ISavedSearchRepository savedSearchRepository) :
			base(new[] { new DataColumn(Constants.ARTIFACT_ID_FIELD_NAME) })
		{
			_savedSearchRepository = savedSearchRepository;
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] results = _savedSearchRepository.RetrieveNext();

			return results;
		}

		protected override bool AllArtifactsFetched()
		{
			bool allDocumentsRetrieved = _savedSearchRepository.AllDocumentsRetrieved();

			return allDocumentsRetrieved;
		}

		public override string GetDataTypeName(int i)
		{
			return typeof(Int32).ToString();
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
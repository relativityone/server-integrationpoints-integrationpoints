using System;
using System.Data;
using kCura.IntegrationPoints.Core.Services.RDO;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	public class DocumentArtifactIdDataReader : RelativityReaderBase
	{
		public DocumentArtifactIdDataReader(IRDORepository rdoRepository, int savedSearchArtifactId) :
			base(rdoRepository, CreateQuery(savedSearchArtifactId), new [] { new DataColumn(Shared.Constants.ARTIFACT_ID_FIELD_NAME) })
		{
		}

		protected override ObjectQueryResutSet ExecuteQueryToGetInitialResult()
		{
			return RDORepository.RetrieveAsync(ObjectQuery, String.Empty).Result;
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
			return CurrentItemResult.ArtifactId;
		}

		private static Query CreateQuery(int savedSearchArtifactId)
		{
			return new Query()
			{
				Condition = $"'ArtifactID' == {savedSearchArtifactId}",
				Fields = new string[] {}, // No fields are required for this query
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				Sorts = new [] {"ArtifactID ASC"},
				TruncateTextFields = true
			};
		}
	}
}
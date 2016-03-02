using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers.Implementations
{
	public class KeplerFieldManager : IFieldManager
	{
		private readonly IRDORepository _rdoRepository;
		public KeplerFieldManager(IRDORepository rdoRepository)
		{
			_rdoRepository = rdoRepository;
		}

		public ArtifactFieldDTO[] RetrieveLongTextFields(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new Query()
			{
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId} AND 'Field Type' == '{longTextFieldName}'",
				Fields = new string[0],
			};

			ObjectQueryResutSet result = _rdoRepository.RetrieveAsync(longTextFieldsQuery, String.Empty).Result;

			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages;
				throw new Exception(e);
			}

			ArtifactFieldDTO[] fieldDtos = result.Data.DataResults.Select(x => new ArtifactFieldDTO()
			{
				ArtifactId = x.ArtifactId,
				FieldType = longTextFieldName,
				Name = x.TextIdentifier,
				Value = null
			}).ToArray();

			return fieldDtos;
		}
	}
}
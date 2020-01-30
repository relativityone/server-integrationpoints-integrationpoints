using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.Relativity.ImportAPI;
using Relativity.Services.Objects.DataContracts;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class NotSupportedByIAPIFieldsClassifier : IFieldsClassifier
	{
		private const int DocumentArtifactTypeID = (int)ArtifactType.Document;

		private readonly IImportApiFactory _importApiFactory;

		public NotSupportedByIAPIFieldsClassifier(IImportApiFactory importApiFactory)
		{
			_importApiFactory = importApiFactory;
		}

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			HashSet<string> fieldsSupportedByIAPI = new HashSet<string>(GetFieldsSupportedByIAPIAsync(workspaceID).Select(x => x.Name));

			IEnumerable<FieldClassificationResult> filteredOutFields = fields
				.Where(field => !fieldsSupportedByIAPI.Contains(field.Name))
				.Select(x => new FieldClassificationResult()
				{
					Name = x.Name,
					FieldIdentifier = x.ArtifactID.ToString(),
					ClassificationLevel = ClassificationLevel.HideFromUser,
					ClassificationReason = "Field not supported by IAPI."
				});

			return Task.FromResult(filteredOutFields);
		}

		private IEnumerable<Field> GetFieldsSupportedByIAPIAsync(int workspaceId)
		{
			IImportAPI iapi = _importApiFactory.Create();
			IEnumerable<Field> workspaceFields = iapi.GetWorkspaceFields(workspaceId, DocumentArtifactTypeID);
			return workspaceFields;
		}
	}
}
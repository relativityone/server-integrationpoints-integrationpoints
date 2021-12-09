using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class NotSupportedByIAPIFieldsClassifier : IFieldsClassifier
	{
		private readonly IImportAPI _importApi;
		private const int DocumentArtifactTypeID = (int)ArtifactType.Document;


		public NotSupportedByIAPIFieldsClassifier(IImportAPI importApi)
		{
			_importApi = importApi;
		}

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<DocumentFieldInfo> fields, int workspaceID)
		{
			HashSet<string> fieldsSupportedByIAPI = new HashSet<string>(GetFieldsSupportedByIAPIAsync(workspaceID).Select(x => x.Name));

			IEnumerable<FieldClassificationResult> filteredOutFields = fields
				.Where(field => !fieldsSupportedByIAPI.Contains(field.Name))
				.Select(x => new FieldClassificationResult(x)
				{
					ClassificationLevel = ClassificationLevel.HideFromUser,
					ClassificationReason = "Field not supported by IAPI."
				});

			return Task.FromResult(filteredOutFields);
		}

		private IEnumerable<Field> GetFieldsSupportedByIAPIAsync(int workspaceId)
		{
			IEnumerable<Field> workspaceFields = _importApi.GetWorkspaceFields(workspaceId, DocumentArtifactTypeID);
			return workspaceFields;
		}
	}
}
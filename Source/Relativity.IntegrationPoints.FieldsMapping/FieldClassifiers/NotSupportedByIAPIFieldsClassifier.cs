using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class NotSupportedByIAPIFieldsClassifier : IFieldsClassifier
	{
		private readonly IFieldsMappingImportApiFacade _importApiFacade;
		private const int DocumentArtifactTypeID = (int)ArtifactType.Document;


		public NotSupportedByIAPIFieldsClassifier(IFieldsMappingImportApiFacade importApiFacade)
		{
			_importApiFacade = importApiFacade;
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
			IEnumerable<Field> workspaceFields = _importApiFacade.GetWorkspaceFields(workspaceId, DocumentArtifactTypeID);
			return workspaceFields;
		}
	}
}
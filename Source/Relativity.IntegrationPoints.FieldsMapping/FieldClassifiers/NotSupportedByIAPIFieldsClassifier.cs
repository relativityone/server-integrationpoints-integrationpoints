using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class NotSupportedByIAPIFieldsClassifier : IFieldsClassifier
	{
		private readonly IImportApiFacade _importApiFacade;
		private const int DocumentArtifactTypeID = (int)ArtifactType.Document;


		public NotSupportedByIAPIFieldsClassifier(IImportApiFacade importApiFacade)
		{
			_importApiFacade = importApiFacade;
		}

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<DocumentFieldInfo> fields, int workspaceID)
		{
			HashSet<string> fieldsSupportedByIAPI = new HashSet<string>(GetFieldsSupportedByIAPIAsync(workspaceID));

			IEnumerable<FieldClassificationResult> filteredOutFields = fields
				.Where(field => !fieldsSupportedByIAPI.Contains(field.Name))
				.Select(x => new FieldClassificationResult(x)
				{
					ClassificationLevel = ClassificationLevel.HideFromUser,
					ClassificationReason = "Field not supported by IAPI."
				});

			return Task.FromResult(filteredOutFields);
		}

		private IEnumerable<string> GetFieldsSupportedByIAPIAsync(int workspaceId)
		{
			IEnumerable<string> workspaceFields = _importApiFacade.GetWorkspaceFieldsNames(workspaceId, DocumentArtifactTypeID).Values;
			return workspaceFields;
		}
	}
}
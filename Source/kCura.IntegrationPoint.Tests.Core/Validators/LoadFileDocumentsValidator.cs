namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using System.Collections.Generic;
	using System.Linq;
	using Relativity.Client.DTOs;

	public class LoadFileDocumentsValidator : DocumentsValidator
	{
		public LoadFileDocumentsValidator(IEnumerable<Document> expectedDocuments, int destinationWorkspaceId, params IDocumentValidator[] documentValidators) :
			base(expectedDocuments.ToList, () => DocumentService.GetAllDocuments(destinationWorkspaceId), documentValidators)
		{
		}
	}
}
﻿using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public class PushOnlyWithImagesValidator : DocumentsValidator
	{
		public PushOnlyWithImagesValidator(int sourceWorkspaceId, int destinationWorkspaceId,
			params IDocumentValidator[] documentValidators)
			: base(
				() => DocumentService.GetAllDocuments(sourceWorkspaceId)
					.Where(doc => doc.GetValueOrDefault(x => x.RelativityImageCount) > 0).ToList(),
				() => DocumentService.GetAllDocuments(destinationWorkspaceId)
					.Where(doc => doc.GetValueOrDefault(x => x.RelativityImageCount) > 0).ToList(),
				documentValidators
			)
		{
		}
	}
}
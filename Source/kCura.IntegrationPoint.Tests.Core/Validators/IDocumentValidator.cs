﻿namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using Relativity.Client.DTOs;

	public interface IDocumentValidator
	{
		void ValidateDocument(Document actualDocument, Document expectedDocument);
	}
}
﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class DocumentTransferProviderTests : RelativityProviderTemplate
	{
		private readonly DocumentTransferProvider _documentTransferProvider;
		public DocumentTransferProviderTests() : base("DestinationWorkspaceRepositoryTests", null)
		{	
			_documentTransferProvider = new DocumentTransferProvider(Helper);
		}

		[Test]
		public void Get_RelativityFieldsFromSourceWorkspace_Success()
		{
			//Arrange
			string documentTransferSettings = $"{{\"SourceWorkspaceArtifactId\":{SourceWorkspaceArtifactId}}}";

			//Act
			IEnumerable<FieldEntry> documentFields = _documentTransferProvider.GetFields(documentTransferSettings);

			//Assert
		}
	}
}

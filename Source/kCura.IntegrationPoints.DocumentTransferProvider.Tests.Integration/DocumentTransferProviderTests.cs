﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Integration
{
	[TestFixture]
	//[Ignore("Tests need refactor")]
	public class DocumentTransferProviderTests : RelativityProviderTemplate
	{
		private DocumentTransferProvider _documentTransferProvider;
		private IWebApiConfig _webApiConfig;
	    private string[] _forbiddenFields;

	    public DocumentTransferProviderTests() : base("SourceWorkspace", "DestinationWorkspace")
	    {
	    }

	    public override void SuiteSetup()
	    {
	        base.SuiteSetup();
	        _forbiddenFields = new[]
	        {
	            Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
	            Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
	            DocumentFields.RelativityDestinationCase,
	            DocumentFields.JobHistory
	        };
            _webApiConfig = new WebApiConfig();
	        _documentTransferProvider = new DocumentTransferProvider(_webApiConfig, Container.Resolve<IRepositoryFactory>(), Container.Resolve<IHelper>());
        }

	    [Test]
		public void Get_RelativityFieldsFromSourceWorkspace_Success()
		{
			//Arrange
			string documentTransferSettings = $"{{\"SourceWorkspaceArtifactId\":{SourceWorkspaceArtifactId}}}";

			//Act
			IEnumerable<FieldEntry> documentFields = _documentTransferProvider.GetFields(documentTransferSettings);

			//Assert
            Assert.That(() => documentFields.Any());
		    Assert.That(() => documentFields.All(df => _forbiddenFields.All(ff => ff != df.DisplayName)));
		}
	}
}

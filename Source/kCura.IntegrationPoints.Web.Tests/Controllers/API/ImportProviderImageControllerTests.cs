﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication.CredentialProvider;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture, Category("ImportProvider")]
	public class ImportProviderImageControllerTests : TestBase
	{
		private ImportProviderImageController _controller;
		private IManagerFactory _managerFactory;
		private IFieldManager _fieldManager;
		private ICredentialProvider _credentialProvider;
		private ICaseManagerFactory _caseManagerFactory;
		private WinEDDS.Service.Export.ICaseManager _caseManager;
		private CaseInfo _caseInfo;
		private string[] _fileRepos = new string[] { "defPath", "ghiPath", "abcPath" };

		private ArtifactFieldDTO[] _fieldArray;

		[SetUp]
		public override void SetUp()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_fieldManager = Substitute.For<IFieldManager>();
			_credentialProvider = Substitute.For<ICredentialProvider>();
			_caseManagerFactory = Substitute.For<ICaseManagerFactory>();
			_caseManager = Substitute.For<WinEDDS.Service.Export.ICaseManager>();

			_managerFactory.CreateFieldManager().Returns(_fieldManager);
			_caseManagerFactory.Create(null, null).ReturnsForAnyArgs(_caseManager);

			_controller = new ImportProviderImageController(_managerFactory, _credentialProvider, _caseManagerFactory);
		}

		[Test]
		public void ItShouldReturnOverlayFields()
		{
			//Arrange
			GenerateFieldArray();
			_fieldManager.RetrieveBeginBatesFields(123456).ReturnsForAnyArgs(_fieldArray);

			//Act
			IHttpActionResult response = _controller.GetOverlayIdentifierFields(123456);
			IEnumerable<ArtifactFieldDTO> actualResult = ExtractArrayResponse(response);

			//Assert
			CollectionAssert.AreEqual(_fieldArray, actualResult);
		}

		[Test]
		public void ItShouldReturnFileRepos()
		{
			//Arrange
			SetUpCaseInfo();
			_caseManager.Read(12345).ReturnsForAnyArgs(_caseInfo);
			_caseManager.GetAllDocumentFolderPathsForCase(12345).ReturnsForAnyArgs(_fileRepos);

			//Act
			IHttpActionResult response = _controller.GetFileRepositories(12345);
			string[] actualResult = ExtractStringArrayResponse(response);

			//Assert
			//Make sure that the result are in alpha order
			CollectionAssert.AreEqual(_fileRepos.OrderBy(x=>x).ToArray(), actualResult);
		}

		[Test]
		public void ItShouldReturnDefaultFileRepo()
		{
			//Arrange
			SetUpCaseInfo();
			_caseManager.Read(12345).ReturnsForAnyArgs(_caseInfo);
			_caseManager.GetAllDocumentFolderPathsForCase(12345).ReturnsForAnyArgs(_fileRepos);

			//Act
			IHttpActionResult response = _controller.GetDefaultFileRepo(12345);
			string actualResult = ExtractStringResponse(response);

			//Assert
			CollectionAssert.AreEqual(_caseInfo.DocumentPath, actualResult);
		}

		private void GenerateFieldArray()
		{
			List<ArtifactFieldDTO> fieldList = new List<ArtifactFieldDTO>();
			for (int i = 1; i < 6; i++)
			{
				fieldList.Add(new ArtifactFieldDTO { ArtifactId = i, Name = string.Format("FieldName{0}", i.ToString()) });
			}

			_fieldArray = fieldList.ToArray();
		}

		private void SetUpCaseInfo()
		{
			_caseInfo = new CaseInfo();
			_caseInfo.ArtifactID = 12345;
			_caseInfo.DocumentPath = "defaultPath";
		}

		private ArtifactFieldDTO[] ExtractArrayResponse(IHttpActionResult response)
		{
			JsonResult<ArtifactFieldDTO[]> result = response as JsonResult<ArtifactFieldDTO[]>;
			return result.Content;
		}

		private string[] ExtractStringArrayResponse(IHttpActionResult response)
		{
			JsonResult<string[]> result = response as JsonResult<string[]>;
			return result.Content;
		}

		private string ExtractStringResponse(IHttpActionResult response)
		{
			JsonResult<string> result = response as JsonResult<string>;
			return result.Content;
		}
	}
}

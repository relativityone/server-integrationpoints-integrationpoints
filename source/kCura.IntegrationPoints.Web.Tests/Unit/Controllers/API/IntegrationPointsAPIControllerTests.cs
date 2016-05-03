using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Tests.Helpers;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers.API
{

	[TestFixture]
	public class IntegrationPointsAPIControllerTests : TestBase
	{
		private IntegrationPointsAPIController _instance;
		private IIntegrationPointService _integrationPointService;
		private ICaseServiceContext _caseServiceContext;
		private IPermissionService _permissionService;
		private IRdoSynchronizerProvider _rdoSynchronizerProvider;
		private IRelativityUrlHelper _relativityUrlHelper;

		private const int WORKSPACE_ID = 23432;

		[SetUp]
		public void TestFixtureSetUp()
		{
			_integrationPointService = this.GetMock<IIntegrationPointService>();
			_caseServiceContext = this.GetMock<ICaseServiceContext>();
			_permissionService = this.GetMock<IPermissionService>();
			_rdoSynchronizerProvider = this.GetMock<IRdoSynchronizerProvider>();
			_relativityUrlHelper = this.GetMock<IRelativityUrlHelper>();

			_instance = this.ResolveInstance<IntegrationPointsAPIController>();
			_instance.Request = new HttpRequestMessage();
			_instance.Request.SetConfiguration(new HttpConfiguration());
		}

		[Test]
		public void Update_StandardSourceProvider_NoJobsRun_GoldFlow()
		{
			// Arrange
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			var sourceProvider = new SourceProvider()
			{
				Identifier	= "ID"
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID), 
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new {returnURL = url}), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");
		}

		[Test]
		public void Update_RelativitySourceProvider_NoJobsRun_HasPermissions_GoldFlow()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			int sourceWorkspaceArtifactId = 2039;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId })
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			var sourceProvider = new SourceProvider()
			{
				Identifier = DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID,
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_permissionService.UserCanImport(Arg.Is(targetWorkspaceArtifactId))
				.Returns(true);

			_permissionService.UserCanEditDocuments(Arg.Is(sourceWorkspaceArtifactId))
				.Returns(true);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID),
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");

		}


		[Test]
		public void Update_RelativitySourceProvider_NoJobsRun_InvalidPermissions_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			var sourceProvider = new SourceProvider()
			{
				Identifier = DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID,
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_permissionService.UserCanImport(Arg.Is(targetWorkspaceArtifactId))
				.Returns(false);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID),
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Update(WORKSPACE_ID, model);
			}
			catch (Exception e)
			{
				exceptionThrown = true;
				Assert.AreEqual(Core.Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT, e.Message, "The exception message was incorrect");
			}

			// Assert
			Assert.IsTrue(exceptionThrown, "An exception should have been thrown");
		}

		[Test]
		public void Update_RelativitySourceProvider_NoJobsRun_InvalidEdit_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			int sourceWorkspaceArtifactId = 2039;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				})
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			var sourceProvider = new SourceProvider()
			{
				Identifier = DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID,
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_permissionService.UserCanImport(Arg.Is(targetWorkspaceArtifactId))
				.Returns(true);

			_permissionService.UserCanEditDocuments(Arg.Is(sourceWorkspaceArtifactId))
				.Returns(false);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID),
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Update(WORKSPACE_ID, model);
			}
			catch (Exception e)
			{
				exceptionThrown = true;
				Assert.AreEqual(ImportNowController.NO_PERMISSION_TO_EDIT_DOCUMENTS, e.Message, "The exception message was incorrect");
			}

			// Assert
			Assert.IsTrue(exceptionThrown, "An exception should have been thrown");
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void Update_RelativitySourceProvider_NewInstance_HasPermissions_GoldFlow(int artifactId)
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			int sourceWorkspaceArtifactId = 2039;
			var model = new IntegrationModel()
			{
				ArtifactID = artifactId,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				})
			};

			var sourceProvider = new SourceProvider()
			{
				Identifier = DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID,
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_permissionService.UserCanImport(Arg.Is(targetWorkspaceArtifactId))
				.Returns(true);

			_permissionService.UserCanEditDocuments(Arg.Is(sourceWorkspaceArtifactId))
				.Returns(true);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID),
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");
		}

		[Test]
		[TestCase(false, new string[] { "Name" }, true)]
		[TestCase(false, new string[] { "Destination Provider" }, true)]
		[TestCase(false, new string[] { "Destination RDO" }, true)]
		[TestCase(false, new string[] { "Case" }, true)]
		[TestCase(false, new string[] { "Source Provider" }, true)]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case" }, true)]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Provider" }, true)]
		[TestCase(false, new string[] { "Name", "Source Configuration" }, true)] // normal providers will only throw with "Name" in list
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" }, true)]
		[TestCase(true, new string[] { "Source Configuration" }, true)]
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" }, false)] // If relativity provider and no permissions, throw permissions error first
		public void Update_InvalidProperties_Excepts(bool isRelativityProvider, string[] propertyNames, bool hasPermissions)
		{
			// Arrange
			var propertyNameHashSet = new HashSet<string>(propertyNames);
			const int targetWorkspaceArtifactId = 12329;
			const int sourceWorkspaceArtifactId = 92321;
			int existingTargetWorkspaceArtifactId = propertyNameHashSet.Contains("Source Configuration")
				? 12324
				: targetWorkspaceArtifactId;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				Name = "My Name",
				DestinationProvider = 4909,
				SourceProvider = 9830,
				Destination	= JsonConvert.SerializeObject(new { artifactTypeID = 10, CaseArtifactId = 7891232}),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				})
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				LastRun = DateTime.Now,
				Name = propertyNameHashSet.Contains("Name") ? "Diff Name" : model.Name,
				DestinationProvider = propertyNameHashSet.Contains("Destination Provider") ? 12343 : model.DestinationProvider,
				SourceProvider = propertyNameHashSet.Contains("Source Provider") ? 391232 : model.SourceProvider,
				Destination = JsonConvert.SerializeObject(new
				{
					artifactTypeID = propertyNameHashSet.Contains("Destination RDO") ? 13 : 10,
					CaseArtifactId = propertyNameHashSet.Contains("Case") ? 18392 : 7891232
				}),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = existingTargetWorkspaceArtifactId
				})
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			// Source Provider is special, if this changes we except earlier
			if (!propertyNameHashSet.Contains("Source Provider"))
			{

				var sourceProvider = new SourceProvider()
				{
					Identifier = isRelativityProvider
						? DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID
						: "YODUDE"
				};
				_caseServiceContext.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Returns(sourceProvider);

				if (isRelativityProvider)
				{
					_permissionService.UserCanImport(Arg.Is(targetWorkspaceArtifactId))
						.Returns(hasPermissions);

					_permissionService.UserCanEditDocuments(Arg.Is(sourceWorkspaceArtifactId))
						.Returns(true);
				}
			}

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Update(WORKSPACE_ID, model);
			}
			catch (Exception e)
			{
				exceptionThrown = true;
				if (hasPermissions)
				{
					string filteredNames =
						String.Join(",",
							propertyNames.Where(x => isRelativityProvider || x != "Source Configuration").Select(x => $" {x}"));
					string expectedErrorString =
						$"Unable to save Integration Point:{filteredNames} cannot be changed once the Integration Point has been run";

					Assert.AreEqual(expectedErrorString, e.Message);
				}
				else
				{
					Assert.AreEqual(Core.Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT, e.Message, "The permission error should have been thrown");
				}
			}

			// Assert
			Assert.IsTrue(exceptionThrown, "An exception should have been thrown");
		}

		[Test]
		public void Update_IPReadFails_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
			};

			const string exceptionMessage = "UH OH!";
			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Throws(new Exception(exceptionMessage));

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Update(WORKSPACE_ID, model);
			}
			catch (Exception e)
			{
				exceptionThrown = true;
				Assert.AreEqual("Unable to save Integration Point: Unable to retrieve Integration Point", e.Message, "The exception message should be correct");	
				Assert.IsNotNull(e.InnerException, "The exception should have an inner exeption");
				Assert.AreEqual(exceptionMessage, e.InnerException.Message, "The innner exception message should match");
			}

			// Assert
			Assert.IsTrue(exceptionThrown, "An exception should have been thrown");
		}

		[Test]
		public void Update_SourceProviderReadFails_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			const string exceptionMessage = "UH OH!";
			_caseServiceContext.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Throws(new Exception(exceptionMessage));

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Update(WORKSPACE_ID, model);
			}
			catch (Exception e)
			{
				exceptionThrown = true;
				Assert.AreEqual("Unable to save Integration Point: Unable to retrieve source provider", e.Message, "The exception message should be correct");
				Assert.IsNotNull(e.InnerException, "The exception should have an inner exeption");
				Assert.AreEqual(exceptionMessage, e.InnerException.Message, "The innner exception message should match");
			}

			// Assert
			Assert.IsTrue(exceptionThrown, "An exception should have been thrown");
		}
	}
}
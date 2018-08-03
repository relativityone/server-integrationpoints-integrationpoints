using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Artifact = kCura.Relativity.Client.Artifact;
using Assert = NUnit.Framework.Assert;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture]
	public class RdoSynchronizerTests : TestBase
	{
		public static RdoSynchronizer ChangeWebAPIPath(RdoSynchronizer synchronizer)
		{
			var prop = synchronizer.GetType().GetProperty(kCura.IntegrationPoints.Domain.Constants.WEB_API_PATH);
			prop.SetValue(synchronizer, "Mock value");
			return synchronizer;
		}

		[SetUp]
		public override void SetUp()
		{
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(new TestHelper());
		}

		[Test]
		public void GetRightCountOfFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = Substitute.For<IRSAPIClient>();
			var helper = Substitute.For<IHelper>();
			var fieldMock = Substitute.For<RelativityFieldQuery>(client, helper);
			var rdoQuery = Substitute.For<IObjectTypeRepository>();
			var jobFactory = Substitute.For<IImportJobFactory>();
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectTypeDTO
			{
				DescriptorArtifactTypeId = 1,
				Name = "Document"
			});
			//
			var options = new ImportSettings();
			options.ArtifactTypeId = 1268820;
			fieldMock.GetFieldsForRdo(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "System Created On", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "Artifact ID", ArtifactID = 5}
			});

			//ACT
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, RdoEntitySynchronizerTests.GetMockAPI(fieldMock), jobFactory, Substitute.For<IHelper>()));
			var str = JsonConvert.SerializeObject(options);
			var numberOfFields = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(str)).Count();
			//ASSERT

			Assert.AreEqual(3, numberOfFields);
		}

		[Test]
		public void GetRightDataInFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = Substitute.For<IRSAPIClient>();
			var helper = Substitute.For<IHelper>();
			var fieldMock = Substitute.For<RelativityFieldQuery>(client, helper);
			var rdoQuery = Substitute.For<IObjectTypeRepository>();
			var jobFactory = Substitute.For<IImportJobFactory>();
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectTypeDTO
			{
				DescriptorArtifactTypeId = 1,
				Name = "Document"
			});
			var options = new ImportSettings { ArtifactTypeId = 1268820 };
			fieldMock.GetFieldsForRdo(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "System Created On", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "Artifact ID", ArtifactID = 5}
			});
					var expectedFieldEntry = new List<FieldEntry>
			{
				new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
				new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
				new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
			};

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, RdoEntitySynchronizerTests.GetMockAPI(fieldMock), jobFactory, Substitute.For<IHelper>()));

			var listOfFieldEntry = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(str)).ToList();

			//ASSERT
			Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
			for (var i = 0; i < listOfFieldEntry.Count; i++)
			{
				Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
				Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
			}
		}

		[Test]
		public void GetRightCountOfFields()
		{
			//ARRANGE
			var client = Substitute.For<IRSAPIClient>();
			var helper = Substitute.For<IHelper>();
			var fieldMock = Substitute.For<RelativityFieldQuery>(client, helper);
			var jobFactory = Substitute.For<IImportJobFactory>();
			//
			var rdoQuery = Substitute.For<IObjectTypeRepository>();
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectTypeDTO
			{
				DescriptorArtifactTypeId = 1,
				Name = "Document"
			});
			var options = new ImportSettings();
			options.ArtifactTypeId = 1268820;
			fieldMock.GetFieldsForRdo(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "Value", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "FirstName", ArtifactID = 5}
			});

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, RdoEntitySynchronizerTests.GetMockAPI(fieldMock), jobFactory, Substitute.For<IHelper>()));

			var numberOfFields = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(str)).Count();

			//ASSERT
			Assert.AreEqual(5, numberOfFields);
		}

		[Test]
		public void GetRightDataInFields()
		{
			//ARRANGEk
			var client = Substitute.For<IRSAPIClient>();
			var helper = Substitute.For<IHelper>();
			var fieldMock = Substitute.For<RelativityFieldQuery>(client, helper);
			var rdoQuery = Substitute.For<IObjectTypeRepository>();
			var jobFactory = Substitute.For<IImportJobFactory>();
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectTypeDTO
			{
				DescriptorArtifactTypeId = 1,
				Name = "Document"
			});
			var options = new ImportSettings { ArtifactTypeId = 1268820 };
			fieldMock.GetFieldsForRdo(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "Value", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "FirstName", ArtifactID = 5}
			});
			var expectedFieldEntry = new List<FieldEntry>
			{
				new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
				new FieldEntry {DisplayName = "Value", FieldIdentifier = "2"},
				new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
				new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
				new FieldEntry {DisplayName = "FirstName", FieldIdentifier = "5"}
			};

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, RdoEntitySynchronizerTests.GetMockAPI(fieldMock), jobFactory, Substitute.For<IHelper>()));

			var listOfFieldEntry = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(str)).ToList();

			//ASSERT
			Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
			for (var i = 0; i < listOfFieldEntry.Count; i++)
			{
				Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
				Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
			}
		}

		[Test]
		public void GetSyncDataImportSettings_NoNativeFileImport_CorrectResult()
		{
			//ARRANGE
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
			{
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
			};

			NativeFileImportService nativeFileImportService = new NativeFileImportService();
			string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222, ImportNativeFile = false, ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles});
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

			Assert.AreEqual(1111111, result.ArtifactTypeId);
			Assert.AreEqual(2222222, result.CaseArtifactId);
			Assert.AreEqual(4000001, result.IdentityFieldId);
			Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
			Assert.AreEqual(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, result.ImportNativeFileCopyMode);
			Assert.AreEqual(false, result.CopyFilesToDocumentRepository);
			Assert.AreEqual(result.NativeFilePathSourceFieldName, string.Empty);
			Assert.IsFalse(result.DisableNativeLocationValidation.HasValue);
			Assert.IsFalse(result.DisableNativeValidation.HasValue);
			Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
			Assert.IsNull(nativeFileImportService.SourceFieldName);
			Assert.IsFalse(nativeFileImportService.ImportNativeFiles);
		}

		[Test]
		public void GetSyncDataImportSettings_SetFileLinks_CorrectResult()
		{
			//ARRANGE
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
			{
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
			};

			NativeFileImportService nativeFileImportService = new NativeFileImportService();
		    var importSettings = new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222, ImportNativeFile = false, ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks };
		    string options = JsonConvert.SerializeObject(importSettings);
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();
			rdoSynchronizer.SourceProvider = new SourceProvider()
			{
				Config = new SourceProviderConfiguration()
				{
					AlwaysImportNativeFiles = true
				}
			};

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);
			
			Assert.AreEqual(1111111, result.ArtifactTypeId);
			Assert.AreEqual(2222222, result.CaseArtifactId);
			Assert.AreEqual(4000001, result.IdentityFieldId);
			Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
			Assert.AreEqual(ImportNativeFileCopyModeEnum.SetFileLinks, result.ImportNativeFileCopyMode);
			Assert.AreEqual(false, result.CopyFilesToDocumentRepository);
			Assert.IsFalse(result.DisableNativeLocationValidation.Value);
			Assert.IsFalse(result.DisableNativeValidation.Value);
			Assert.AreEqual("NATIVE_FILE_PATH_001", result.NativeFilePathSourceFieldName);
			Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
			Assert.AreEqual(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD, nativeFileImportService.SourceFieldName);
			Assert.IsTrue(nativeFileImportService.ImportNativeFiles);
		}

		[Test]
		public void GetSyncDataImportSettings_NativeFileImport_CorrectResult()
		{
			//ARRANGE
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
			{
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
				new FieldMap() {DestinationField = null, FieldMapType = FieldMapTypeEnum.NativeFilePath, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld4"}},
			};

			var nativeFileImportService = new NativeFileImportService();
		    var importSettings = new ImportSettings
		    {
		        ArtifactTypeId = 1111111,
		        CaseArtifactId = 2222222,
		        ImportNativeFile = true,
		        ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles
		    };
		    string options = JsonConvert.SerializeObject(importSettings);
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);
			
			//ASSERT
			Assert.AreEqual(1111111, result.ArtifactTypeId);
			Assert.AreEqual(2222222, result.CaseArtifactId);
			Assert.AreEqual(4000001, result.IdentityFieldId);
			Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
			Assert.AreEqual(ImportNativeFileCopyModeEnum.CopyFiles, result.ImportNativeFileCopyMode);
			Assert.AreEqual(true, result.CopyFilesToDocumentRepository);
			Assert.IsFalse(result.DisableNativeLocationValidation.Value);
			Assert.IsFalse(result.DisableNativeValidation.Value);
			Assert.AreEqual("NATIVE_FILE_PATH_001", result.NativeFilePathSourceFieldName);
			Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
			Assert.AreEqual("SourceFld4", nativeFileImportService.SourceFieldName);
			Assert.IsTrue(nativeFileImportService.ImportNativeFiles);
		}

		[Test]
		public void GetSyncDataImportSettings_NoFolderInformationPath_CorrectResult()
		{
			//ARRANGE
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
			{
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
			};

			NativeFileImportService nativeFileImportService = new NativeFileImportService();
			string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222 });
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

			//ASSERT
			Assert.IsNull(result.FolderPathSourceFieldName);
			Assert.AreEqual(0, result.DestinationFolderArtifactId);
		}

		[Test]
		public void GetSyncDataImportSettings_FolderInformationPath_CorrectResult()
		{
			//ARRANGE
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
			{
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
				new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
				new FieldMap() {DestinationField = null, FieldMapType = FieldMapTypeEnum.FolderPathInformation, SourceField = new FieldEntry() {DisplayName = "SourceFld4"}},
			};

			NativeFileImportService nativeFileImportService = new NativeFileImportService();
			string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222 });
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

			//ASSERT
			Assert.AreEqual(kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME, result.FolderPathSourceFieldName);
			Assert.AreEqual(0, result.DestinationFolderArtifactId);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsNone_True()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
				FieldMapType = FieldMapTypeEnum.None,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsTrue(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsParent_False()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
				FieldMapType = FieldMapTypeEnum.Parent,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsFalse(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsNativeFilePath_False()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
				FieldMapType = FieldMapTypeEnum.NativeFilePath,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsFalse(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsIdentifier_True()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
				FieldMapType = FieldMapTypeEnum.Identifier,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsTrue(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsADestination_True()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
				FieldMapType = FieldMapTypeEnum.FolderPathInformation,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsTrue(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsNoDestinationSet_False()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = null,
				FieldMapType = FieldMapTypeEnum.FolderPathInformation,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsFalse(result);
		}

		[Test]
		public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsDestinationHasNullProperties_False()
		{
			//ARRANGE
			FieldMap fieldMap = new FieldMap()
			{
				DestinationField = new FieldEntry(),
				FieldMapType = FieldMapTypeEnum.FolderPathInformation,
				SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
			};
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

			//ASSERT
			Assert.IsFalse(result);
		}

		[Test]
		public void GetEmailBodyData_HasWorkspace_CorrectlyFormatedOutput()
		{
			//ARRANGE
			int workspaceId = 1111111;
			WorkspaceRef workspaceRef = new WorkspaceRef() { Id = workspaceId, Name = "My Test workspace" };
			IEmailBodyData rdoSynchronizer = new mockSynchronizer(workspaceRef);
			var settings = new ImportSettings { CaseArtifactId = workspaceId };
			var options = JsonConvert.SerializeObject(settings);

			//ACT
			var returnedString = rdoSynchronizer.GetEmailBodyData(null, options);

			//ASSERT
			Assert.AreEqual("\r\nDestination Workspace: My Test workspace - 1111111", returnedString);
		}

		[Test]
		public void GetEmailBodyData_NoWorkspace_CorrectlyFormatedOutput()
		{
			//ARRANGE
			int workspaceId = 1111111;
			WorkspaceRef workspaceRef = null;
			IEmailBodyData rdoSynchronizer = new mockSynchronizer(workspaceRef);
			var settings = new ImportSettings { CaseArtifactId = workspaceId };
			var options = JsonConvert.SerializeObject(settings);

			//ACT
			var returnedString = rdoSynchronizer.GetEmailBodyData(null, options);

			//ASSERT
			Assert.AreEqual("", returnedString);
		}
		
		/// <summary>
		 /// Test whether options are parsed correctly when getting the mappable fields
		 /// </summary>
		[Test]
		public void GetFields_CorrectOptionsPassed()
		{
			var relativityFieldQuery = NSubstitute.Substitute.For<IRelativityFieldQuery>();
			var importApiFactory = NSubstitute.Substitute.For<IImportApiFactory>();
			var importApi = NSubstitute.Substitute.For<IExtendedImportAPI>();
			var jobFactory = Substitute.For<IImportJobFactory>();
			var helper = Substitute.For<IHelper>();
			var rdoSynchronizerPush = new RdoSynchronizer(relativityFieldQuery, importApiFactory, jobFactory, helper);

			int artifactTypeId = 123;
			int caseArtifactId = 456;

			string options = String.Format("{{Provider:'relativity', WebServiceUrl:'WebServiceUrl', ArtifactTypeId:{0}, CaseArtifactId:{1}}}", artifactTypeId, caseArtifactId);
			List<Artifact> fields = new List<Artifact>();
			IEnumerable<kCura.Relativity.ImportAPI.Data.Field> mappableFields = new List<kCura.Relativity.ImportAPI.Data.Field>();

			relativityFieldQuery.GetFieldsForRdo(Arg.Is(artifactTypeId))
				.Returns(fields);

			importApiFactory.GetImportAPI(Arg.Any<ImportSettings>())
				.Returns(importApi);

			importApi.GetWorkspaceFields(caseArtifactId, artifactTypeId).Returns(mappableFields);

			IEnumerable<FieldEntry> results = rdoSynchronizerPush.GetFields(new DataSourceProviderConfiguration(options));

			relativityFieldQuery
				.Received(1)
				.GetFieldsForRdo(Arg.Is(artifactTypeId));
			importApiFactory
				.Received(1)
				.GetImportAPI(Arg.Any<ImportSettings>());
			importApi
				.Received(1)
				.GetWorkspaceFields(caseArtifactId, artifactTypeId);
		}
	}

	public class TestRdoSynchronizer : RdoSynchronizer
	{
		public TestRdoSynchronizer()
		  : base(null, null, Substitute.For<IImportJobFactory>(), Substitute.For<IHelper>())
		{
			WebAPIPath = kCura.IntegrationPoints.Domain.Constants.WEB_API_PATH;
			DisableNativeLocationValidation = false;
			DisableNativeValidation = false;
		}

		public new ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, string options, NativeFileImportService nativeFileImportService)
		{
			return base.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);
		}

		public new bool IncludeFieldInImport(FieldMap fieldMap)
		{
			return base.IncludeFieldInImport(fieldMap);
		}
	}

	public class mockSynchronizer : RdoSynchronizer
	{
		private WorkspaceRef _workspaceRef;
		public mockSynchronizer(WorkspaceRef workspaceRef)
		  : base(null, null, Substitute.For<IImportJobFactory>(), Substitute.For<IHelper>())
		{
			WebAPIPath = "WebAPIPath";
			DisableNativeLocationValidation = false;
			DisableNativeValidation = false;
			_workspaceRef = workspaceRef;
		}

		protected override WorkspaceRef GetWorkspace(ImportSettings settings)
		{
			return _workspaceRef;
		}
	}
}
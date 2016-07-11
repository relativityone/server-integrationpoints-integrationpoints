﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Artifact = kCura.Relativity.Client.Artifact;
using Assert = NUnit.Framework.Assert;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class RdoSynchronizerTests
	{
		public static RdoSynchronizerBase ChangeWebAPIPath(RdoSynchronizerBase synchronizer)
		{
			var prop = synchronizer.GetType().GetProperty(kCura.IntegrationPoints.Domain.Constants.WEB_API_PATH);
			prop.SetValue(synchronizer, "Mock value");
			return synchronizer;
		}

		[Test]
		public void GetRightCountOfFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RSAPIRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
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
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizerPull(fieldMock, RdoCustodianSynchronizerTests.GetMockAPI(fieldMock)));
			var str = JsonConvert.SerializeObject(options);
			var numberOfFields = rdoSynchronizer.GetFields(str).Count();
			//ASSERT

			Assert.AreEqual(3, numberOfFields);
		}

		[Test]
		public void GetRightDataInFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RSAPIRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
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
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizerPull(fieldMock, RdoCustodianSynchronizerTests.GetMockAPI(fieldMock)));
			var listOfFieldEntry = rdoSynchronizer.GetFields(str).ToList();

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
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			//
			var rdoQuery = NSubstitute.Substitute.For<RSAPIRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
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
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizerPull(fieldMock, RdoCustodianSynchronizerTests.GetMockAPI(fieldMock)));
			var numberOfFields = rdoSynchronizer.GetFields(str).Count();

			//ASSERT
			Assert.AreEqual(5, numberOfFields);
		}

		[Test]
		public void GetRightDataInFields()
		{
			//ARRANGEk
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RSAPIRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
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
			var rdoSynchronizer = ChangeWebAPIPath(new RdoSynchronizerPull(fieldMock, RdoCustodianSynchronizerTests.GetMockAPI(fieldMock)));
			var listOfFieldEntry = rdoSynchronizer.GetFields(str).ToList();

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
			string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222 });
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

			Assert.AreEqual(1111111, result.ArtifactTypeId);
			Assert.AreEqual(2222222, result.CaseArtifactId);
			Assert.AreEqual(4000001, result.IdentityFieldId);
			Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
			Assert.AreEqual(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, result.ImportNativeFileCopyMode);
			Assert.IsNull(result.NativeFilePathSourceFieldName);
			Assert.IsFalse(result.DisableNativeLocationValidation.HasValue);
			Assert.IsFalse(result.DisableNativeValidation.HasValue);
			Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
			Assert.IsNull(nativeFileImportService.SourceFieldName);
			Assert.IsFalse(nativeFileImportService.ImportNativeFiles);
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

			NativeFileImportService nativeFileImportService = new NativeFileImportService();
			string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222, ImportNativeFile = true });
			TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

			//ACT
			ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

			//ASSERT
			Assert.AreEqual(1111111, result.ArtifactTypeId);
			Assert.AreEqual(2222222, result.CaseArtifactId);
			Assert.AreEqual(4000001, result.IdentityFieldId);
			Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
			Assert.AreEqual(ImportNativeFileCopyModeEnum.CopyFiles, result.ImportNativeFileCopyMode);
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
	}

	public class TestRdoSynchronizer : RdoSynchronizerPull
	{
		public TestRdoSynchronizer()
		  : base(null, null)
		{
			WebAPIPath = kCura.IntegrationPoints.Domain.Constants.WEB_API_PATH;
			DisableNativeLocationValidation = false;
			DisableNativeValidation = false;
		}

		public ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, string options, NativeFileImportService nativeFileImportService)
		{
			return base.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);
		}

		public bool IncludeFieldInImport(FieldMap fieldMap)
		{
			return base.IncludeFieldInImport(fieldMap);
		}
	}

	public class mockSynchronizer : RdoSynchronizerPush
	{
		private WorkspaceRef _workspaceRef;
		public mockSynchronizer(WorkspaceRef workspaceRef)
		  : base(null, null)
		{
			WebAPIPath = "WebAPIPath";
			DisableNativeLocationValidation = false;
			DisableNativeValidation = false;
			_workspaceRef = workspaceRef;
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			return base.GetEmailBodyData(fields, options);
		}

		protected override WorkspaceRef GetWorkspace(ImportSettings settings)
		{
			return _workspaceRef;
		}
	}
}
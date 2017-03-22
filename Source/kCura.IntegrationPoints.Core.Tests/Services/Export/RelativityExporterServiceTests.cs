using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
{
	[TestFixture]
	public class RelativityExporterServiceTests : TestBase
	{
		private const string _CONTROL_NUMBER = "Control Num";
		private const string _FILE_NAME = "FileName";
		private IExporter _exporter;
		private IHelper _helper;
		private IILongTextStreamFactory _longTextFieldFactory;
		private global::Relativity.Core.Export.InitializationResults _exportApiResult;
		private FieldMap[] _mappedFields;
		private HashSet<int> _longTextField;
		private RelativityExporterService _instance;
		private int[] _avfIds;
		private IJobStopManager _jobStopManager;
		private ArtifactDTO _goldFlowExpectedDto;
		private object[] _goldFlowRetrievableData;
		private IFolderPathReader _folderPathReader;

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
			_folderPathReader = Substitute.For<IFolderPathReader>();
			_helper = Substitute.For<IHelper>();
			_exporter = Substitute.For<IExporter>();
			_longTextFieldFactory = Substitute.For<IILongTextStreamFactory>();
			_exportApiResult = new global::Relativity.Core.Export.InitializationResults()
			{
				RunId = Guid.NewGuid(),
				ColumnNames = new[] { _CONTROL_NUMBER, _FILE_NAME }
			};

			// source identifier is the only thing that matter
			_mappedFields = new FieldMap[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = "123"
					}
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = "456"
					}
				}
			};

			_goldFlowRetrievableData = new object[] { new object[] { "REL01", "FileName", 1111 } };
			_goldFlowExpectedDto = new ArtifactDTO(1111, 10, "Document", new[]
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = 123,
					Value = "REL01",
					Name = _CONTROL_NUMBER
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = _FILE_NAME,
					Name = _FILE_NAME
				},
			});
			_longTextField = new HashSet<int>(new int[] {456});
		}

		[SetUp]
		public override void SetUp()
		{
			_avfIds = new[] { 1, 2 };
			_jobStopManager = Substitute.For<IJobStopManager>();
		}

		[Test]
		public void HasDataToRetrieve_RetunsFalseOnJobThatDoesNotHaveAnythingToRead()
		{
			// arrange
			_exportApiResult.RowCount = 0;
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, _avfIds);
			_jobStopManager.IsStopRequested().Returns(false);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}


		[Test]
		public void HasDataToRetrieve_RetunsFalseOnFinishedJob()
		{
			// arrange
			_exportApiResult.RowCount = 1;
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_exporter.RetrieveResults(_exportApiResult.RunId, _avfIds, 1).Returns(_goldFlowRetrievableData);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, _avfIds);
			_jobStopManager.IsStopRequested().Returns(false);

			// act
			ArtifactDTO[] retrievedData = _instance.RetrieveData(1);
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			ValidateArtifact(_goldFlowExpectedDto, retrievedData[0]);
			Assert.IsFalse(hasDataToRetrieve);

		}


		[Test]
		public void HasDataToRetrieve_RetunsTrueOnRunningJob_WithIntMaxData()
		{
			// arrange
			_exportApiResult.RowCount = Int32.MaxValue;
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, _avfIds);
			_jobStopManager.IsStopRequested().Returns(false);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsTrue(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_RetunsFalseOnRunningJob_WithLongMaxData()
		{
			// arrange
			_exportApiResult.RowCount = Int64.MaxValue;
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, _avfIds);
			_jobStopManager.IsStopRequested().Returns(false);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_RetunsFalseOnStoppedJob()
		{
			// arrange
			_exportApiResult.RowCount = Int64.MaxValue;
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, _avfIds);
			_jobStopManager.IsStopRequested().Returns(true);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}

		[Test]
		public void RetrieveData_GoldFlow()
		{
			// Arrange
			object[] obj = new[] {new object[] { "REL01", "FileName", 1111 }};
			int[] avfIds = new[] {1, 2};

			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns(obj);

			ArtifactDTO expecteDto = new ArtifactDTO(1111, 10, "Document", new []
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = 123,
					Value = "REL01",
					Name = _CONTROL_NUMBER
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = _FILE_NAME,
					Name = _FILE_NAME
				},
			});

			// Act
			RelativityExporterService rel = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, avfIds);
			ArtifactDTO[] data = rel.RetrieveData(1);


			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(1, data.Length);
			ArtifactDTO artifact = data[0];

			ValidateArtifact(expecteDto, artifact);
		}

		[Test]
		public void RetrieveData_UnableToGetLongTextField()
		{
			// Arrange
			object[] obj = new[] { new object[] { "REL01", "#KCURA99DF2F0FEB88420388879F1282A55760#", 1111 } };
			int[] avfIds = new[] { 1, 2 };

			_longTextFieldFactory.CreateLongTextStream(Arg.Any<int>(), Arg.Any<int>()).Throws(new Exception("exception please"));
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns(obj);


			// Act
			RelativityExporterService rel = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, avfIds);
			ArtifactDTO[] data = rel.RetrieveData(1);

			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(1, data.Length);
			ArtifactDTO result = data[0];
			Assert.AreEqual(2, result.Fields.Count);

			ArtifactFieldDTO exceptedfield = result.Fields[1];
			Assert.Throws<Exception>(() => { object x = exceptedfield.Value; });
		}

		[Test]
		public void RetrieveData_NoDataReturned()
		{
			// Arrange
			int[] avfIds = new[] { 1, 2 };

			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns((object)null);

			// Act
			RelativityExporterService rel = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, avfIds);
			ArtifactDTO[] data = rel.RetrieveData(1);

			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(0, data.Length);
		}

		[Test]
		public void RetrieveData_ShouldSetFolderPath()
		{
			// Arrange
			int[] avfIds = new[] { 1, 2 };
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns((object)null);

			// Act
			RelativityExporterService rel = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper, _folderPathReader, _mappedFields, _longTextField, avfIds);
			rel.RetrieveData(1);

			// Assert
			_folderPathReader.Received(1).SetFolderPaths(Arg.Any<List<ArtifactDTO>>());
		}

		private void ValidateArtifact(ArtifactDTO expect, ArtifactDTO actual)
		{
			Assert.AreEqual(expect.ArtifactId, actual.ArtifactId);
			Assert.AreEqual(expect.ArtifactTypeId, expect.ArtifactTypeId);
			for (int i = 0; i < expect.Fields.Count; i++)
			{
				ArtifactFieldDTO expectedField = expect.Fields[i];
				ArtifactFieldDTO actualField = actual.Fields[i];

				Assert.AreEqual(expectedField.Name, actualField.Name);
				Assert.AreEqual(expectedField.Value, actualField.Value);
				Assert.AreEqual(expectedField.ArtifactId, actualField.ArtifactId);
				Assert.AreEqual(expectedField.FieldType, actualField.FieldType);
			}
		}
	}
}
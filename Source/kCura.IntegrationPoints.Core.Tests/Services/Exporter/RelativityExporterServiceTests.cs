using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
	[TestFixture]
	public class RelativityExporterServiceTests : TestBase
	{
		private ArtifactDTO _goldFlowExpectedDto;
		private FieldMap[] _mappedFields;
		private global::Relativity.Core.Export.InitializationResults _exportApiResult;
		private HashSet<int> _longTextField;
		private Mock<IExporter> _exporter;
		private Mock<IFolderPathReader> _folderPathReader;
		private Mock<IHelper> _helper;
		private IJobStopManager _jobStopManager;
		private int[] _avfIds;
		private IQueryFieldLookupRepository _queryFieldLookupRepository;
		private IRelativityObjectManager _relativityObjectManager;
		private object[] _goldFlowRetrievableData;
		private RelativityExporterService _instance;
		private const string _CONTROL_NUMBER = "Control Num";
		private const string _FILE_NAME = "FileName";
		
		[SetUp]
		public override void SetUp()
		{
			var apiLogMock = new Mock<IAPILog>();
			_helper = new Mock<IHelper>();
			_helper
				.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<RelativityExporterService>())
				.Returns(apiLogMock.Object);
			_exporter = new Mock<IExporter>();
			_exportApiResult = new global::Relativity.Core.Export.InitializationResults()
			{
				RunId = new Guid("3A51AF56-0813-4E25-89DD-E08EC0C8526C"),
				ColumnNames = new[] { _CONTROL_NUMBER, _FILE_NAME }
			};
			_exporter
				.Setup(x => x.InitializeExport(0, null, 0))
				.Returns(_exportApiResult);

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
					Name = _CONTROL_NUMBER,
					FieldType = FieldTypeHelper.FieldType.Empty.ToString()
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = _FILE_NAME,
					Name = _FILE_NAME,
					FieldType = FieldTypeHelper.FieldType.Empty.ToString()
				},
			});
			_longTextField = new HashSet<int>(new int[] { 456 });

			_avfIds = new[] { 1, 2 };
			_jobStopManager = new Mock<IJobStopManager>().Object;
			Mock.Get(_jobStopManager)
				.Setup(x => x.IsStopRequested())
				.Returns(false);
			_folderPathReader = new Mock<IFolderPathReader>();
			_queryFieldLookupRepository = new Mock<IQueryFieldLookupRepository>().Object;
			_relativityObjectManager = new Mock<IRelativityObjectManager>().Object;
			_instance = new RelativityExporterService(
				_exporter.Object, 
				_relativityObjectManager,
				_jobStopManager, 
				_helper.Object,
				_queryFieldLookupRepository, 
				_folderPathReader.Object, 
				_mappedFields, 
				_longTextField, 
				_avfIds);
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnJobThatDoesNotHaveAnythingToRead()
		{
			// arrange
			_exportApiResult.RowCount = 0;

			Mock.Get(_queryFieldLookupRepository)
				.Setup(x => x.GetFieldByArtifactId(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Code));

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}


		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnFinishedJob()
		{
			// arrange
			_exportApiResult.RowCount = 1;
			
			_exporter
				.Setup(x => x.RetrieveResults(_exportApiResult.RunId, _avfIds, 1))
				.Returns(_goldFlowRetrievableData);

			Mock.Get(_queryFieldLookupRepository)
				.Setup(x => x.GetFieldByArtifactId(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));

			Mock.Get(_queryFieldLookupRepository)
				.Setup(x => x.GetFieldTypeByArtifactId(It.IsAny<int>()))
				.Returns(FieldTypeHelper.FieldType.Empty.ToString());

			// act
			ArtifactDTO[] retrievedData = _instance.RetrieveData(1);
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			ValidateArtifact(_goldFlowExpectedDto, retrievedData[0]);
			Assert.IsFalse(hasDataToRetrieve);

		}


		[Test]
		public void HasDataToRetrieve_ReturnsTrueOnRunningJob_WithIntMaxData()
		{
			// arrange
			_exportApiResult.RowCount = Int32.MaxValue;

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsTrue(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnRunningJob_WithLongMaxData()
		{
			// arrange
			_exportApiResult.RowCount = Int64.MaxValue;
			
			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnStoppedJob()
		{
			// arrange
			_exportApiResult.RowCount = Int64.MaxValue;
			
			Mock.Get(_jobStopManager)
				.Setup(x => x.IsStopRequested())
				.Returns(true);

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


			_exporter
				.Setup(x => x.RetrieveResults(_exportApiResult.RunId, It.IsAny<int[]>(), 1))
				.Returns(obj);

			ArtifactDTO expecteDto = new ArtifactDTO(1111, 10, "Document", new []
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = 123,
					Value = "REL01",
					Name = _CONTROL_NUMBER,
					FieldType = FieldTypeHelper.FieldType.Empty.ToString()
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = _FILE_NAME,
					Name = _FILE_NAME,
					FieldType = FieldTypeHelper.FieldType.Empty.ToString()
				},
			});

			Mock.Get(_queryFieldLookupRepository)
				.Setup(x => x.GetFieldByArtifactId(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));

			Mock.Get(_queryFieldLookupRepository)
				.Setup(x => x.GetFieldTypeByArtifactId(It.IsAny<int>()))
				.Returns(FieldTypeHelper.FieldType.Empty.ToString());

			// Act
			ArtifactDTO[] data = _instance.RetrieveData(1);


			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(1, data.Length);
			ArtifactDTO artifact = data[0];

			ValidateArtifact(expecteDto, artifact);
		}

		[Test]
		public void RetrieveData_NoDataReturned()
		{
			// Arrange
			int[] avfIds = new[] { 1, 2 };

			_exporter
				.Setup(x => x.RetrieveResults(_exportApiResult.RunId, avfIds, 1))
				.Returns<object>(null);

			// Act
			ArtifactDTO[] data = _instance.RetrieveData(1);

			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(0, data.Length);
		}

		[Test]
		public void RetrieveData_ShouldSetFolderPath()
		{
			// Arrange
			int[] avfIds = new[] { 1, 2 };

			_exporter
				.Setup(x => x.RetrieveResults(_exportApiResult.RunId, avfIds, 1))
				.Returns<object>(null);

			// Act
			_instance.RetrieveData(1);

			// Assert
			Mock.Get(_folderPathReader)
				.Verify(x => x.SetFolderPaths(It.IsAny<List<ArtifactDTO>>()), Times.Once);
		}

		private void ValidateArtifact(ArtifactDTO expect, ArtifactDTO actual)
		{
			Assert.AreEqual(expect.ArtifactId, actual.ArtifactId);
			Assert.AreEqual(expect.ArtifactTypeId, actual.ArtifactTypeId);
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
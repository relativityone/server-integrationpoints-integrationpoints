using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
	[TestFixture]
	public class RelativityExporterServiceTests : TestBase
	{
		private ArtifactDTO _goldFlowExpectedDto;
		private FieldMap[] _mappedFields;
		private global::Relativity.Core.Export.InitializationResults _exportApiResult;
		private HashSet<int> _longTextField;
		private IExporter _exporter;
		private IFolderPathReader _folderPathReader;
		private IHelper _helper;
		private IILongTextStreamFactory _longTextFieldFactory;
		private IJobStopManager _jobStopManager;
		private int[] _avfIds;
		private IQueryFieldLookupRepository _queryFieldLookupRepository;
		private object[] _goldFlowRetrievableData;
		private RelativityExporterService _instance;
		private const string _CONTROL_NUMBER = "Control Num";
		private const string _FILE_NAME = "FileName";
		
		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_exporter = Substitute.For<IExporter>();
			_longTextFieldFactory = Substitute.For<IILongTextStreamFactory>();
			_exportApiResult = new global::Relativity.Core.Export.InitializationResults()
			{
				RunId = new Guid("3A51AF56-0813-4E25-89DD-E08EC0C8526C"),
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
			_jobStopManager = Substitute.For<IJobStopManager>();
			_folderPathReader = Substitute.For<IFolderPathReader>();
			_queryFieldLookupRepository = Substitute.For<IQueryFieldLookupRepository>();
			IToggleProvider toggleProvider = Substitute.For<IToggleProvider>();
			_exporter.InitializeExport(0, null, 0).Returns(_exportApiResult);
			_instance = new RelativityExporterService(_exporter, _longTextFieldFactory, _jobStopManager, _helper,
				_queryFieldLookupRepository, _folderPathReader, toggleProvider, _mappedFields, _longTextField, _avfIds);
		}

		[Test]
		public void HasDataToRetrieve_RetunsFalseOnJobThatDoesNotHaveAnythingToRead()
		{
			// arrange
			_exportApiResult.RowCount = 0;
			
			_queryFieldLookupRepository.GetFieldByArtifactId(Arg.Any<int>()).Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Code));
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
			
			_exporter.RetrieveResults(_exportApiResult.RunId, _avfIds, 1).Returns(_goldFlowRetrievableData);
			_queryFieldLookupRepository.GetFieldByArtifactId(Arg.Any<int>()).Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));
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


			_exporter.RetrieveResults(_exportApiResult.RunId, Arg.Any<int[]>(), 1).Returns(obj);

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

			// Act
			_queryFieldLookupRepository.GetFieldByArtifactId(Arg.Any<int>()).Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));
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

			
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns((object)null);

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
			
			_exporter.RetrieveResults(_exportApiResult.RunId, avfIds, 1).Returns((object)null);

			// Act
			_instance.RetrieveData(1);

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
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class TempTableReaderTests
	{
		private const string ConstValue = "456";
		private IDocumentRepository _documentRepo;
		private IDataReader _reader;
		private IScratchTableRepository _scratchTable;
		private TempTableReader _instance;
		private int _identifierFieldId;
		private DataColumn[] _columns;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			_documentRepo = Substitute.For<IDocumentRepository>();
			_reader = Substitute.For<IDataReader>();
			_scratchTable = Substitute.For<IScratchTableRepository>();
			_identifierFieldId = 100;

			_scratchTable.GetDocumentIdsDataReaderFromTable().Returns(_reader);

			_columns = new[] {new DataColumn("Identifier"), new DataColumnWithValue("someValue", ConstValue) };

			_instance = new TempTableReader(_documentRepo, _scratchTable, _columns, _identifierFieldId);
		}

		[Test]
		public void Close_DisposeInnerReader()
		{
			// act
			_instance.Close();

			// assert
			_reader.Received().Close();
		}

		[Test]
		public void GetFieldType_ReturnString()
		{
			// act
			Type type = _instance.GetFieldType(0);
			
			// assert
			Assert.AreEqual(typeof(string), type);
		}

		[Test]
		public void GetDataTypeName_ReturnString()
		{
			// act
			string type = _instance.GetDataTypeName(0);

			// assert
			Assert.AreEqual(typeof(string).ToString(), type);
		}

		[Test]
		public void GetFieldType_IndexOutOfRange()
		{
			Assert.Throws<IndexOutOfRangeException>(() => _instance.GetFieldType(564543));
		}

		[Test]
		[Ignore("Fails on run script, Passes on VS")]
		public void FetchArtifactDTOs_ReturnArtifactDto()
		{
			// arrange
			const string id = "123";
			Task<ArtifactDTO[]> task =
				new Task<ArtifactDTO[]>(() => new ArtifactDTO[]
				{
					new ArtifactDTO(0, 0, new List<ArtifactFieldDTO>()
					{
						new ArtifactFieldDTO() { Value = id }
					})
				});
			task.Start();
			Task.WaitAll(task);

			int artifactId = 987;
			_reader.Read().Returns(true, false);
			_reader.GetInt32(0).Returns(artifactId);
			_documentRepo.RetrieveDocumentsAsync(Arg.Any<List<int>>(), Arg.Any<HashSet<int>>()).
				Returns(task);

			// act
			bool read = _instance.Read();
			string idValue = _instance.GetString(0);
			string dataColumnValue = _instance.GetString(1);

			bool read2 = _instance.Read();

			// assert
			Assert.IsTrue(read);
			Assert.AreEqual(id, idValue);
			Assert.AreEqual(ConstValue, dataColumnValue);
			Assert.IsFalse(read2);
		}
	}
}
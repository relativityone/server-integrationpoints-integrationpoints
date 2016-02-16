using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Unit
{
	[TestFixture]
	public class RelativityReaderBaseTests
	{
		private const string _TOKEN_FOR_MORE = "There are some more things to get";
		private IRelativityClientAdaptor _adaptor;

		[SetUp]
		public void Setup()
		{
			_adaptor = Substitute.For<IRelativityClientAdaptor>();
		}

		protected QueryResultSet<Document> ExecuteQueryToGetInitialResult()
		{
			return new QueryResultSet<Document>()
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(1)
					},
					new Result<Document>()
					{
						Artifact = new Document(2)
					}
				},
				Message = "lol",
				QueryToken = _TOKEN_FOR_MORE,
				TotalCount = 3
			};
		}

		[Test]
		public void ReadDocumentCounterGetUpdated()
		{
			MockRelativityReaderBase instance = new MockRelativityReaderBase(_adaptor, ExecuteQueryToGetInitialResult());
			Assert.AreEqual(0, instance.Counter);

			instance.Read();
			instance.Read();

			Assert.AreEqual(2, instance.Counter);
		}

		[Test]
		public void ReadDocumentsUntilTheEnd_FetchMoreData()
		{
			MockRelativityReaderBase instance = new MockRelativityReaderBase(_adaptor, ExecuteQueryToGetInitialResult());

			var newResult = new QueryResultSet<Document>()
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(3)
					}
				},
				Message = "lol",
				QueryToken = String.Empty,
				TotalCount = 3
			};

			_adaptor.ExecuteSubSetOfDocumentQuery(_TOKEN_FOR_MORE, 2, Shared.Constants.QUERY_BATCH_SIZE).Returns(newResult);

			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(3, instance.Counter);
			Assert.AreEqual(false, instance.IsClosed);
		}

		[Test]
		public void ReadDocumentsUntilTheEnd_NoMoreData_TheReaderClose()
		{
			MockRelativityReaderBase instance = new MockRelativityReaderBase(_adaptor, ExecuteQueryToGetInitialResult());

			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(false, instance.Read());
			Assert.AreEqual(true, instance.IsClosed);
		}

		[Test]
		public void ReadDocumentsUntilTheEnd_FetchMoreDataAndReadToEnd()
		{
			MockRelativityReaderBase instance = new MockRelativityReaderBase(_adaptor, ExecuteQueryToGetInitialResult());

			var newResult = new QueryResultSet<Document>()
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(3)
					}
				},
				Message = "lol",
				QueryToken = String.Empty,
				TotalCount = 3
			};

			_adaptor.ExecuteSubSetOfDocumentQuery(_TOKEN_FOR_MORE, 2, Shared.Constants.QUERY_BATCH_SIZE).Returns(newResult);

			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(false, instance.Read());
			Assert.AreEqual(true, instance.IsClosed);
		}

		[Test]
		public void ReadDocumentsUntilTheEnd_FetchMoreDataFail()
		{
			MockRelativityReaderBase instance = new MockRelativityReaderBase(_adaptor, ExecuteQueryToGetInitialResult());

			var newResult = new QueryResultSet<Document>()
			{
				Success = false,
				Results = null,
				Message = "lol",
				QueryToken = String.Empty,
				TotalCount = 3
			};

			_adaptor.ExecuteSubSetOfDocumentQuery(_TOKEN_FOR_MORE, 2, Shared.Constants.QUERY_BATCH_SIZE).Returns(newResult);

			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(true, instance.Read());
			Assert.AreEqual(false, instance.Read());
			Assert.AreEqual(true, instance.IsClosed);
			Assert.AreEqual(2, instance.Counter);
		}

		private class MockRelativityReaderBase : RelativityReaderBase
		{
			private readonly QueryResultSet<Document> _initialResult;

			public MockRelativityReaderBase(IRelativityClientAdaptor relativityClient, QueryResultSet<Document> initialResult)
				: base(relativityClient)
			{
				_initialResult = initialResult;
			}

			public int Counter { get { return ReadEntriesCount; } }

			public override int FieldCount { get; }

			public override string GetName(int i)
			{
				throw new NotImplementedException();
			}

			public override int GetOrdinal(string name)
			{
				throw new NotImplementedException();
			}

			public override DataTable GetSchemaTable()
			{
				throw new NotImplementedException();
			}

			public override string GetDataTypeName(int i)
			{
				throw new NotImplementedException();
			}

			public override Type GetFieldType(int i)
			{
				throw new NotImplementedException();
			}

			public override object GetValue(int i)
			{
				throw new NotImplementedException();
			}

			protected override QueryResultSet<Document> ExecuteQueryToGetInitialResult()
			{
				return _initialResult;
			}
		}
	}
}
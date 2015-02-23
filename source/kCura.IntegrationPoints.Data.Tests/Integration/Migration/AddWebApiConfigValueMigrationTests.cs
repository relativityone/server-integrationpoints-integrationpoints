using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Migration
{
	[TestFixture]
	[IntegrationTest]
	public class AddWebApiConfigValueMigrationTests
	{
		private IEddsDBContext _context;
		[SetUp]
		public void Setup()
		{
			_context = new EddsContext(new MockDBContext(string.Empty));
		}

		[Test]
		public void Execute_KeyAlreadyExists_DoesNotUpdate()
		{
			
		}

		[Test]
		public void Execute_KeyDoesNotExistButProcessingDoes_UsesProcessingKey()
		{

		}

		[Test]
		public void Execute_KeyDoesNotExistAndProcessingDoesnot_UsesDBMTKey()
		{

		}

		[TearDown]
		public void TearDown()
		{
			//restore previous value
		}

	}
}

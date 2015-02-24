using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Migration
{
	[TestFixture]
	//[IntegrationTest]
	public class AddWebApiConfigValueMigrationTests
	{
		public const string IP_SECTION = "kCura.IntegrationPoints";
		public const string IP_WEB_KEY = "webAPIPATH";

		public const string PROCESSING_SECTION = "Relativity.Core";

		private IEddsDBContext _context = new EddsContext(new MockDBContext(Config.ConnectionString));
		private string _oldWebAPIValue;
		private string _oldDBMTValue;
		private string _oldProcessAPIValue;

		private string GetConfigValue(string section, string name)
		{
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@section", section));
			sqlParams.Add(new SqlParameter("@name", name));
			return _context.ExecuteSqlStatementAsScalar<string>(
				"SELECT [Value] FROM [EDDS].[eddsdbo].[Configuration] where Section = @section AND [name] = @name", sqlParams);
		}

		public void SetConfigValue(string section, string name, string value)
		{
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@section", section));
			sqlParams.Add(new SqlParameter("@name", name));
			sqlParams.Add(new SqlParameter("@value", value));
			_context.ExecuteNonQuerySQLStatement("Update [EDDS].[eddsdbo].[Configuration] set [Value] = @value where Section = @section AND [name] = @name", sqlParams);
		}

		public void DeleteConfigValue(string section, string name)
		{
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@section", section));
			sqlParams.Add(new SqlParameter("@name", name));
			_context.ExecuteNonQuerySQLStatement("Delete From [EDDS].[eddsdbo].[Configuration] where Section = @section AND [name] = @name", sqlParams);
		}


		[TestFixtureSetUp]
		public void Setup()
		{
			_oldWebAPIValue = GetConfigValue(IP_SECTION, IP_WEB_KEY);
			_oldDBMTValue = GetConfigValue("kCura.EDDS.DBMT", "WebAPIPath");
			_oldProcessAPIValue = GetConfigValue(PROCESSING_SECTION, "ProcessingWebAPIPath");

			if (string.IsNullOrEmpty(_oldWebAPIValue))
			{
				//make sure a value is there before going on a testing spree
				var runner = new Data.Migrations.AddWebApiConfigValueMigration(_context);
				runner.Execute();
			}

		}

		[Test]
		public void Execute_KeyAlreadyExists_DoesNotUpdate()
		{
			//ARRANGE
			var current = GetConfigValue(IP_SECTION, IP_WEB_KEY);
			SetConfigValue(PROCESSING_SECTION, "ProcessingWebAPIPath", "some value");
			
			//ACT
			var runner = new Data.Migrations.AddWebApiConfigValueMigration(_context);
			runner.Execute();

			var newValue = GetConfigValue(IP_SECTION, IP_WEB_KEY);

			Assert.AreEqual(current, newValue);
		}

		[Test]
		public void Execute_KeyDoesNotExistButProcessingDoes_UsesProcessingKey()
		{
			//ARRANGE
			var processingValue = "my new processing value";
			DeleteConfigValue(IP_SECTION, IP_WEB_KEY);
			SetConfigValue(PROCESSING_SECTION, "ProcessingWebAPIPath", processingValue);

			//ACT
			var runner = new Data.Migrations.AddWebApiConfigValueMigration(_context);
			runner.Execute();
			
			//ASSERT
			var value = GetConfigValue(IP_SECTION, IP_WEB_KEY);
			Assert.AreEqual(processingValue, value);

		}

		[Test]
		public void Execute_KeyDoesNotExistAndProcessingDoesnot_UsesDBMTKey()
		{
			//ARRANGE
			var dbmtValue = "my new dbmt value";
			DeleteConfigValue("kCura.IntegrationPoints", "webAPIPATH");
			SetConfigValue("Relativity.Core", "ProcessingWebAPIPath", string.Empty);
			SetConfigValue("kCura.EDDS.DBMT", "WebAPIPath", dbmtValue);

			//ACT
			var runner = new Data.Migrations.AddWebApiConfigValueMigration(_context);
			runner.Execute();

			//ASSERT
			var value = GetConfigValue("kCura.IntegrationPoints", "webAPIPATH");
			Assert.AreEqual(dbmtValue, value);


		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			//restore previous value
			SetConfigValue(IP_SECTION, IP_WEB_KEY, _oldWebAPIValue);
			SetConfigValue("kCura.EDDS.DBMT", "WebAPIPath", _oldDBMTValue);
			SetConfigValue(PROCESSING_SECTION, "ProcessingWebAPIPath", _oldProcessAPIValue);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture, Category("Unit")]
	public class ImportServiceTests : TestBase
	{
		private IHelper _helper;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
		}

		[Test]
		public void GenerateImportFields_Pass()
		{
			//ARRANGE
			int fieldValue1 = 111;
			DateTime fieldValue2 = DateTime.Parse("11/22/2010 11:22:33");
			bool fieldValue3 = true;
			string fieldValue4 = "Abc";
			object fieldValue5 = null;
			Dictionary<string, object> sourceFields = new Dictionary<string, object>()
			{
				{"sourceField1",fieldValue1},
				{"sourceField2",fieldValue2},
				{"sourceField3",fieldValue3},
				{"sourceField4",fieldValue4},
				{"sourceField5",fieldValue5}
			};

			Dictionary<string, Field> mapping = new Dictionary<string, Field>()
			{
				{"sourceField1",GetFieldObject(1,"F1")},
				{"sourceField2",GetFieldObject(2,"F2")},
				{"sourceField3",GetFieldObject(3,"F3")},
				{"sourceField4",GetFieldObject(4,"F4")},
				{"sourceField5",GetFieldObject(5,"F5")},
				{"sourceField6",GetFieldObject(6,"F6")},
				{"sourceField7",GetFieldObject(7,"F7")},
			};

			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);

			//ACT
			Dictionary<string, object> data = importService.GenerateImportFields(sourceFields, mapping);

			//ASSERT
			Assert.AreEqual(5, data.Count);
			Assert.IsTrue(data.Keys.Contains("F1"));
			Assert.IsTrue(data.Keys.Contains("F2"));
			Assert.IsTrue(data.Keys.Contains("F3"));
			Assert.IsTrue(data.Keys.Contains("F4"));
			Assert.IsTrue(data.Keys.Contains("F5"));
			Assert.AreEqual(fieldValue1, data["F1"]);
			Assert.AreEqual(fieldValue2, data["F2"]);
			Assert.AreEqual(fieldValue3, data["F3"]);
			Assert.AreEqual(fieldValue4, data["F4"]);
			Assert.AreEqual(fieldValue5, data["F5"]);
		}

		#region ValidateAllMappedFieldsAreInWorkspace

		[Test]
		public void ValidateAllMappedFieldsAreInWorkspace_AllFieldsMatch()
		{
			//ARRANGE
			Dictionary<string, int> fieldMapping = new Dictionary<string, int>(){
				{"sourceField2",2},
				{"sourceField3",3},
				{"sourceField4",4},
				{"sourceField5",5}
			};
			Dictionary<int, Field> rdoAllFields = new Dictionary<int, Field>()			{
				{1,GetFieldObject(1,"F1")},
				{2,GetFieldObject(2,"F2")},
				{3,GetFieldObject(3,"F3")},
				{4,GetFieldObject(4,"F4")},
				{5,GetFieldObject(5,"F5")},
				{6,GetFieldObject(6,"F6")},
				{7,GetFieldObject(7,"F7")},
			};

			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);


			//ACT
			Dictionary<string, Field> fieldMap = importService.ValidateAllMappedFieldsAreInWorkspace(fieldMapping, rdoAllFields);


			//ASSERT
			Assert.AreEqual(4, fieldMap.Count);
			Assert.IsTrue(fieldMap.Keys.Contains("sourceField2"));
			Assert.IsTrue(fieldMap.Keys.Contains("sourceField3"));
			Assert.IsTrue(fieldMap.Keys.Contains("sourceField4"));
			Assert.IsTrue(fieldMap.Keys.Contains("sourceField5"));
			Assert.AreEqual(2, fieldMap["sourceField2"].ArtifactID);
			Assert.AreEqual(3, fieldMap["sourceField3"].ArtifactID);
			Assert.AreEqual(4, fieldMap["sourceField4"].ArtifactID);
			Assert.AreEqual(5, fieldMap["sourceField5"].ArtifactID);
		}

		[Test]
		public void ValidateAllMappedFieldsAreInWorkspace_NoneFieldsMatch()
		{
			//ARRANGE
			Dictionary<string, int> fieldMapping = new Dictionary<string, int>(){
				{"sourceField2",12},
				{"sourceField3",13},
				{"sourceField4",14},
				{"sourceField5",15}
			};
			Dictionary<int, Field> rdoAllFields = new Dictionary<int, Field>()			{
				{1,GetFieldObject(1,"F1")},
				{2,GetFieldObject(2,"F2")},
				{3,GetFieldObject(3,"F3")},
				{4,GetFieldObject(4,"F4")},
				{5,GetFieldObject(5,"F5")},
				{6,GetFieldObject(6,"F6")},
				{7,GetFieldObject(7,"F7")},
			};

			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);


			//ACT
			Exception ex = Assert.Throws<Exception>(() => importService.ValidateAllMappedFieldsAreInWorkspace(fieldMapping, rdoAllFields));

			Assert.That(ex.Message, Is.EqualTo("Missing mapped field IDs: 12, 13, 14, 15"));
		}

		[Test]
		public void ValidateAllMappedFieldsAreInWorkspace_OneOutOfAllFieldsMatch()
		{
			//ARRANGE
			Dictionary<string, int> fieldMapping = new Dictionary<string, int>(){
				{"sourceField2",2},
				{"sourceField3",13},
				{"sourceField4",14},
				{"sourceField5",15}
			};
			Dictionary<int, Field> rdoAllFields = new Dictionary<int, Field>()			{
				{1,GetFieldObject(1,"F1")},
				{2,GetFieldObject(2,"F2")},
				{3,GetFieldObject(3,"F3")},
				{4,GetFieldObject(4,"F4")},
				{5,GetFieldObject(5,"F5")},
				{6,GetFieldObject(6,"F6")},
				{7,GetFieldObject(7,"F7")},
			};

			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);


			//ACT
			Exception ex = Assert.Throws<Exception>(() => importService.ValidateAllMappedFieldsAreInWorkspace(fieldMapping, rdoAllFields));

			Assert.That(ex.Message, Is.EqualTo("Missing mapped field IDs: 13, 14, 15"));
		}

		#endregion

		#region GenerateImportFields

		[Test]
		public void GenerateImportFields_NativeFileImportServiceIsNull_CorrectResult()
		{
			//ARRANGE
			Dictionary<string, object> fieldMapping = new Dictionary<string, object>(){
				{"sourceField1", "ABC"},
				{"sourceField2", 123},
				{"sourceField3", DateTime.MaxValue},
				{"sourceField4", true},
				{"MyPath", "\\\\Server1\\path1\\file1"}
			};
			Dictionary<string, Field> mapping = new Dictionary<string, Field>()			{
				{"sourceField1",GetFieldObject(111,"F1")},
				{"sourceField2",GetFieldObject(222,"F2")},
				{"sourceField4",GetFieldObject(444,"F4")}
			};
			NativeFileImportService nativeFileImportService = null;

			ImportService importService = new ImportService(null, null, null, nativeFileImportService, null, null, _helper, null);


			//ACT
			Dictionary<string, object> result = importService.GenerateImportFields(fieldMapping, mapping);


			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("ABC", result["F1"]);
			Assert.AreEqual(123, result["F2"]);
			Assert.AreEqual(true, result["F4"]);
		}

		[Test]
		public void GenerateImportFields_NativeFileImportServiceIsFalse_CorrectResult()
		{
			//ARRANGE
			Dictionary<string, object> fieldMapping = new Dictionary<string, object>(){
				{"sourceField1", "ABC"},
				{"sourceField2", 123},
				{"sourceField3", DateTime.MaxValue},
				{"sourceField4", true},
				{"MyPath", "\\\\Server1\\path1\\file1"}
			};
			Dictionary<string, Field> mapping = new Dictionary<string, Field>()			{
				{"sourceField1",GetFieldObject(111,"F1")},
				{"sourceField2",GetFieldObject(222,"F2")},
				{"sourceField4",GetFieldObject(444,"F4")}
			};
			NativeFileImportService nativeFileImportService = new NativeFileImportService()
			{
				ImportNativeFiles = false
			};

			ImportService importService = new ImportService(null, null, null, nativeFileImportService, null, null, _helper, null);


			//ACT
			Dictionary<string, object> result = importService.GenerateImportFields(fieldMapping, mapping);


			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("ABC", result["F1"]);
			Assert.AreEqual(123, result["F2"]);
			Assert.AreEqual(true, result["F4"]);
		}

		[Test]
		public void GenerateImportFields_NativeFileImportServiceIsTrue_CorrectResult()
		{
			//ARRANGE
			Dictionary<string, object> fieldMapping = new Dictionary<string, object>(){
				{"sourceField1", "ABC"},
				{"sourceField2", 123},
				{"sourceField3", DateTime.MaxValue},
				{"sourceField4", true},
				{"MyPath", "\\\\Server1\\path1\\file1"}
			};
			Dictionary<string, Field> mapping = new Dictionary<string, Field>()			{
				{"sourceField1",GetFieldObject(111,"F1")},
				{"sourceField2",GetFieldObject(222,"F2")},
				{"sourceField4",GetFieldObject(444,"F4")}
			};
			NativeFileImportService nativeFileImportService = new NativeFileImportService()
			{
				ImportNativeFiles = true,
				SourceFieldName = "MyPath"
			};

			ImportService importService = new ImportService(null, null, null, nativeFileImportService, null, null, _helper, null);


			//ACT
			Dictionary<string, object> result = importService.GenerateImportFields(fieldMapping, mapping);


			Assert.AreEqual(4, result.Count);
			Assert.AreEqual("ABC", result["F1"]);
			Assert.AreEqual(123, result["F2"]);
			Assert.AreEqual(true, result["F4"]);
			Assert.AreEqual("\\\\Server1\\path1\\file1", result[nativeFileImportService.DestinationFieldName]);
		}

		[TestCase("prefix", "message")]
		[TestCase("Prefix", "Message")]
		[TestCase("    Prefix", "    Message")]
		[TestCase("", "    Message")]
		[TestCase(null, "    Message")]
		[TestCase("{0}{1}{2}", "{3}{4}{5}{6}")]
		public void PrependString_NonEmptyOrWhitespaceMessage_ReturnsConcatenatedStrings(string prefix, string message)
		{
			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);

			string result = importService.PrependString(prefix, message);

			Assert.That(string.Equals(result, $"{prefix} {message}", StringComparison.InvariantCulture));
		}

		[TestCase("")]
		[TestCase("				     ")]
		[TestCase(null)]
		public void PrependString_MessageIsNullOrWhitespace_ReturnsGenericMessageWithPrefix(string message)
		{
			string prefix = "Test Prefix";

			ImportService importService = new ImportService(null, null, null, null, null, null, _helper, null);

			string result = importService.PrependString(prefix, message);

			Assert.That(string.Equals(result, $"{prefix} [Unknown message]", StringComparison.InvariantCulture));
		}

		#endregion

		private Field GetFieldObject(int artifactID, string name, Guid? guid = null)
		{
			Field f = new Field();
			f.GetType().GetProperty("ArtifactID").SetValue(f, artifactID, null);
			f.GetType().GetProperty("Name").SetValue(f, name, null);
			if (guid.HasValue) f.GetType().GetProperty("Guid").SetValue(f, new List<Guid>() { guid.Value }, null);

			return f;
		}
	}
}
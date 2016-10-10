using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.Choice;

namespace kCura.IntegrationPoints.Data.Tests
{
	public class BaseRdoTests
	{
		private Guid guidChoice1 = Guid.Parse("E68EF4BE-EB69-4CB6-94FC-D205F2096411");
		private Guid guidChoice2 = Guid.Parse("5F8731F3-C899-4419-BA0F-7E05E4F739DF");

		[Test]
		public void ConvertValue_MultipleChoiceFieldValueNull_CorrectValue()
		{
			//ARRANGE
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, null });


			//ASSERT 
			Assert.IsNull(returnValue);
		}


		[Test]
		public void ConvertValue_MultipleChoiceFieldValueNotNull_CorrectValue()
		{
			//ARRANGE
			Choice[] choices = new Choice[]
			{
				new Choice(){ArtifactGuids = new List<Guid>(){guidChoice1}, Name = "AAA"},
				new Choice(){ArtifactGuids = new List<Guid>(){guidChoice2}, Name= "bbb" }
			};
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, choices });


			//ASSERT 
			Assert.IsTrue(returnValue is MultiChoiceFieldValueList);
			MultiChoiceFieldValueList returnedChoices = (MultiChoiceFieldValueList)returnValue;
			Assert.AreEqual(2, returnedChoices.Count);
			Assert.AreEqual(guidChoice1, returnedChoices[0].Guids.First());
			Assert.AreEqual("AAA", returnedChoices[0].Name);
			Assert.AreEqual(guidChoice2, returnedChoices[1].Guids.First());
			Assert.AreEqual("bbb", returnedChoices[1].Name);
		}

		[Test]
		public void ConvertValue_SingleChoiceFieldValueNotNull_CorrectValue()
		{
			//ARRANGE
			Choice myChoice = new Choice() { ArtifactGuids = new List<Guid>() { guidChoice1 }, Name = "AAA" };
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.SingleChoice, myChoice });


			//ASSERT 
			Assert.IsTrue(returnValue is Relativity.Client.DTOs.Choice);
			Relativity.Client.DTOs.Choice returnedChoice = (Relativity.Client.DTOs.Choice)returnValue;
			Assert.AreEqual(guidChoice1, returnedChoice.Guids.First());
			Assert.AreEqual("AAA", returnedChoice.Name);
		}

		[Test]
		public void ConvertValue_MultipleObjectFieldValueNotNull_CorrectValue()
		{
			//ARRANGE
			int[] multiObjectIDs = new int[]
			{
				111,
				222 
			};
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });


			//ASSERT 
			Assert.IsTrue(returnValue is FieldValueList<RDO>);
			FieldValueList<RDO> returnedObjects = (FieldValueList<RDO>)returnValue;
			Assert.AreEqual(2, returnedObjects.Count);
			Assert.AreEqual(111, returnedObjects[0].ArtifactID);
			Assert.AreEqual(222, returnedObjects[1].ArtifactID);
		}

		// ConvertForGet

		[Test]
		public void ConvertForGet_MultipleChoiceFieldValueNull_CorrectValue()
		{
			//ARRANGE
			TestBaseRdo baseRdo = new TestBaseRdo();
			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertForGet", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, null });
			//ASSERT 
			Assert.IsNull(returnValue);
		}

		[Test]
		public void ConvertForGet_SingleChoiceFieldValueNotNull_CorrectValue()
		{
			//ARRANGE
			Choice myChoice = new Choice() { ArtifactID = 111, Name = "AAA" };
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertForGet", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.SingleChoice, myChoice });


			//ASSERT 
			Assert.IsTrue(returnValue is kCura.Relativity.Client.Choice);
			var returnedChoice = (kCura.Relativity.Client.Choice)returnValue;
			Assert.AreEqual(111, returnedChoice.ArtifactID);
			Assert.AreEqual("AAA", returnedChoice.Name);
		}

		[Test]
		public void ConvertForGet_MultipleObjectFieldValueIsNull_CorrectValue()
		{
			//ARRANGE
			int[] multiObjectIDs = new int[]
			{
				111,
				222 
			};
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertForGet", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });


			//ASSERT 
			Assert.IsTrue(returnValue is System.Int32[]);
		}
		[Test]
		public void ConvertForGet_MultipleObjectFieldValueEmptyArray_CorrectValue()
		{
			//ARRANGE
			int[] multiObjectIDs = new int[]
			{
				111,
				222 
			};
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertForGet", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });

			//ASSERT 
			Assert.IsTrue(returnValue is System.Int32[]);
		}
		[Test]
		public void ConvertForGet_MultipleObjectFieldValueFieldValueList_CorrectValue()
		{
			//ARRANGE
			var fieldList = new FieldValueList<Artifact>()
			{
				new Artifact(123),
				new Artifact(456)
			};

			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertForGet", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, fieldList });

			//ASSERT 
			Assert.IsTrue(returnValue is System.Int32[]);
			Assert.AreEqual(123, ((int[])returnValue)[0]);
			Assert.AreEqual(456, ((int[])returnValue)[1]);
		}
	}

	internal class TestBaseRdo : BaseRdo
	{
		public override System.Collections.Generic.Dictionary<System.Guid, Attributes.DynamicFieldAttribute> FieldMetadata
		{
			get { throw new System.NotImplementedException(); }
		}

		public override Attributes.DynamicObjectAttribute ObjectMetadata
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}

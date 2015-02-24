using System.Reflection;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.Choice;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	public class BaseRDOTests
	{
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
				new Choice(){ArtifactID = 111, Name = "AAA"},
				new Choice(){ArtifactID = 222, Name= "bbb" }
			};
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, choices });


			//ASSERT 
			Assert.IsTrue(returnValue is MultiChoiceFieldValueList);
			MultiChoiceFieldValueList returnedChoices = (MultiChoiceFieldValueList)returnValue;
			Assert.AreEqual(2, returnedChoices.Count);
			Assert.AreEqual(111, returnedChoices[0].ArtifactID);
			Assert.AreEqual("AAA", returnedChoices[0].Name);
			Assert.AreEqual(222, returnedChoices[1].ArtifactID);
			Assert.AreEqual("bbb", returnedChoices[1].Name);
		}

		[Test]
		public void ConvertValue_SingleChoiceFieldValueNotNull_CorrectValue()
		{
			//ARRANGE
			Choice myChoice = new Choice() { ArtifactID = 111, Name = "AAA" };
			TestBaseRdo baseRdo = new TestBaseRdo();


			//ACT
			MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.SingleChoice, myChoice });


			//ASSERT 
			Assert.IsTrue(returnValue is Relativity.Client.DTOs.Choice);
			Relativity.Client.DTOs.Choice returnedChoice = (Relativity.Client.DTOs.Choice)returnValue;
			Assert.AreEqual(111, returnedChoice.ArtifactID);
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

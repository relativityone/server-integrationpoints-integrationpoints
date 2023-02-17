using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Attributes;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Data.Tests
{
    [TestFixture, Category("Unit")]
    public class BaseRdoTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
        }

        private Guid guidChoice1 = Guid.Parse("E68EF4BE-EB69-4CB6-94FC-D205F2096411");
        private Guid guidChoice2 = Guid.Parse("5F8731F3-C899-4419-BA0F-7E05E4F739DF");

        [Test]
        public void ConvertValue_MultipleChoiceFieldValueNull_CorrectValue()
        {
            // ARRANGE
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = baseRdo.GetType().GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, null });

            // ASSERT
            Assert.IsNull(returnValue);
        }

        [Test]
        public void ConvertValue_MultipleChoiceFieldValueNotNull_CorrectValue()
        {
            // ARRANGE
            ChoiceRef[] choices = {
                new ChoiceRef {Guids = new List<Guid> {guidChoice1}, Name = "AAA" },
                new ChoiceRef {Guids= new List<Guid> {guidChoice2}, Name= "bbb" }
            };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertValue");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, choices });

            // ASSERT
            Assert.IsTrue(returnValue is ChoiceRef[]);
            ChoiceRef[] returnedChoices = (ChoiceRef[])returnValue;
            Assert.AreEqual(2, returnedChoices.Length);
            Assert.AreEqual(guidChoice1, returnedChoices[0].Guids.First());
            Assert.AreEqual("AAA", returnedChoices[0].Name);
            Assert.AreEqual(guidChoice2, returnedChoices[1].Guids.First());
            Assert.AreEqual("bbb", returnedChoices[1].Name);
        }

        [Test]
        public void ConvertValue_SingleChoiceFieldValueNotNull_CorrectValue()
        {
            // ARRANGE
            ChoiceRef myChoice = new ChoiceRef { Guids = new List<Guid> { guidChoice1 }, Name = "AAA" };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertValue");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.SingleChoice, myChoice });

            // ASSERT
            Assert.IsTrue(returnValue is ChoiceRef);
            ChoiceRef returnedChoice = (ChoiceRef)returnValue;
            Assert.AreEqual(guidChoice1, returnedChoice.Guids.First());
            Assert.AreEqual("AAA", returnedChoice.Name);
        }

        [Test]
        public void ConvertValue_MultipleObjectFieldValueNotNull_CorrectValue()
        {
            // ARRANGE
            int[] multiObjectIDs = {
                111,
                222
            };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertValue");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });

            // ASSERT
            Assert.IsTrue(returnValue is RelativityObject[]);
            RelativityObject[] returnedObjects = (RelativityObject[])returnValue;
            Assert.AreEqual(2, returnedObjects.Length);
            Assert.AreEqual(111, returnedObjects[0].ArtifactID);
            Assert.AreEqual(222, returnedObjects[1].ArtifactID);
        }

        // ConvertForGet

        [Test]
        public void ConvertForGet_MultipleChoiceFieldValueNull_CorrectValue()
        {
            // ARRANGE
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertForGet");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleChoice, null });

            // ASSERT
            Assert.IsNull(returnValue);
        }

        [Test]
        public void ConvertForGet_SingleChoiceFieldValueNotNull_CorrectValue()
        {
            // ARRANGE
            ChoiceRef myChoice = new ChoiceRef(111) { Name = "AAA" };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertForGet");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.SingleChoice, myChoice });

            // ASSERT
            Assert.IsTrue(returnValue is ChoiceRef);
            var returnedChoice = (ChoiceRef)returnValue;
            Assert.AreEqual(111, returnedChoice.ArtifactID);
            Assert.AreEqual("AAA", returnedChoice.Name);
        }

        [Test]
        public void ConvertForGet_MultipleObjectFieldValueIsNull_CorrectValue()
        {
            // ARRANGE
            int[] multiObjectIDs = {
                111,
                222
            };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertForGet");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });

            // ASSERT
            Assert.IsTrue(returnValue is int[]);
        }

        [Test]
        public void ConvertForGet_MultipleObjectFieldValueEmptyArray_CorrectValue()
        {
            // ARRANGE
            int[] multiObjectIDs = {
                111,
                222
            };
            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertForGet");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, multiObjectIDs });

            // ASSERT
            Assert.IsTrue(returnValue is int[]);
        }

        [Test]
        public void ConvertForGet_MultipleObjectFieldValueFieldValueList_CorrectValue()
        {
            // ARRANGE
            var fieldList = new List<RelativityObject>
            {
                new RelativityObject() {ArtifactID = 123 },
                new RelativityObject() {ArtifactID = 456}
            };

            TestBaseRdo baseRdo = new TestBaseRdo();

            // ACT
            MethodInfo dynMethod = GetMethod(baseRdo, "ConvertForGet");
            object returnValue = dynMethod.Invoke(baseRdo, new object[] { FieldTypes.MultipleObject, fieldList });

            // ASSERT
            Assert.IsTrue(returnValue is int[]);
            Assert.AreEqual(123, ((int[])returnValue)[0]);
            Assert.AreEqual(456, ((int[])returnValue)[1]);
        }

        [Test]
        public void GetFieldGuid_ShouldGetFieldGuidSuccessfully()
        {
            // ACT
            Guid fieldGuid = BaseRdo.GetFieldGuid((TestBaseRdo rdo) => rdo.Property);

            // ASSERT
            fieldGuid.Should().Be(Guid.Parse(TestBaseRdo._RELATIVITY_FIELD_GUID));
        }

        [Test]
        public void GetFieldGuid_ShouldFailOnProvidingMethodExpression()
        {
            // ACT
            Action run = () => BaseRdo.GetFieldGuid((TestBaseRdo rdo) => rdo.GetString());

            // ASSERT
            run
                .ShouldThrowExactly<ArgumentException>()
                .And
                .Message.Should().EndWith("refers to a method, not a property.");
        }

        [Test]
        public void GetFieldGuid_ShouldFailOnProvidingFieldExpression()
        {
            // ACT
            Action run = () => BaseRdo.GetFieldGuid((TestBaseRdo rdo) => rdo.Field);

            // ASSERT
            run
                .ShouldThrowExactly<ArgumentException>()
                .And
                .Message.Should().EndWith("refers to a field, not a property.");
        }

        private MethodInfo GetMethod(BaseRdo sut, string methodName)
        {
            MethodInfo method = sut.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return method;
        }
    }

    internal class TestBaseRdo : BaseRdo
    {
        public const string _RELATIVITY_FIELD_GUID = "085CB84B-4DAA-400F-B28F-18DE267BD7EA";

        public string Field = "field";

        public string GetString() => "string";

        [DynamicField("Property", _RELATIVITY_FIELD_GUID, "Fixed Length Text", 255)]
        public string Property
        {
            get
            {
                return GetField<string>(new Guid(_RELATIVITY_FIELD_GUID));
            }
            set
            {
                SetField(new Guid(_RELATIVITY_FIELD_GUID), value);
            }
        }

        public override Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get { throw new NotImplementedException(); }
        }
    }
}

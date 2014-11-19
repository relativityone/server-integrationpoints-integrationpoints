using System.Runtime.InteropServices;
using kCura.ScheduleQueueAgent.CustomAttributes;
using kCura.ScheduleQueueAgent.Helpers;
using NUnit.Framework;

namespace kCura.ScheduleQueueAgent.Tests
{
	[TestFixture]
	public class QueueTableHelperTests
	{
		[Test]
		public void GetQueueTableName_NoAttributes_ReturnsDefaultName()
		{
			//ARRANGE
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[0];

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual("ScheduleAgentQueue", returnValue);
		}

		[Test]
		public void GetQueueTableName_SystemAttributeOnly_ReturnsDefaultName()
		{
			//ARRANGE
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new System.FlagsAttribute() };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual("ScheduleAgentQueue", returnValue);
		}

		[Test]
		public void GetQueueTableName_GuidAttributeOnly_ReturnsCorrectName()
		{
			//ARRANGE
			string guid = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new GuidAttribute(guid) };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual("AgentQueue_" + guid, returnValue);
		}

		[Test]
		public void GetQueueTableName_QueueTableAttributeOnly_ReturnsCorrectName()
		{
			//ARRANGE
			string name = "TestQueueName";
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new QueueTableAttribute() { Name = name } };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual(name, returnValue);
		}

		[Test]
		public void GetQueueTableName_GuidAttributeAndQueueTableAttributeOnly_ReturnsCorrectName()
		{
			//ARRANGE
			string name = "TestQueueName";
			string guid = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new GuidAttribute(guid), new QueueTableAttribute() { Name = name } };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual(name, returnValue);
		}

		[Test]
		public void GetQueueTableName_QueueTableAttributeAndGuidAttributeOnly_ReturnsCorrectName()
		{
			//ARRANGE
			string name = "TestQueueName";
			string guid = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new QueueTableAttribute() { Name = name }, new GuidAttribute(guid) };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual(name, returnValue);
		}

		[Test]
		public void GetQueueTableName_SystemAttributeAndQueueTableAttributeAndGuidAttributeOnly_ReturnsCorrectName()
		{
			//ARRANGE
			string name = "TestQueueName";
			string guid = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
			var helper = new QueueTableHelper();
			object[] attributeObjects = new object[] { new System.FlagsAttribute(), new QueueTableAttribute() { Name = name }, new GuidAttribute(guid) };

			//ACT
			string returnValue = helper.GetQueueTableName(attributeObjects);

			//ASSERT
			Assert.AreEqual(name, returnValue);
		}
	}
}

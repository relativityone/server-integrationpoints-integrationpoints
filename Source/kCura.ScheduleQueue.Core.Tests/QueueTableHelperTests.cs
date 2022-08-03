using System;
using System.Runtime.InteropServices;
using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.Helpers;
using NUnit.Framework;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class QueueTableHelperTests : TestBase
    {
        private readonly string GetQueueTableNameException = "Could not retrieve Queue table name.";

        private readonly string GetAgentGuidException = "Could not retrieve Agent Guid.";

        [SetUp]
        public override void SetUp()
        {
            
        }

        [Test]
        public void GetQueueTableName_NoAttributes_ReturnsDefaultName()
        {
            //ARRANGE
            var helper = new QueueTableHelper();
            object[] attributeObjects = new object[0];

            //ACT
            Exception ex = Assert.Throws<System.Exception>(delegate { helper.GetQueueTableName(attributeObjects); });

            //ASSERT
            Assert.AreEqual(GetQueueTableNameException, ex.Message);
        }

        [Test]
        public void GetQueueTableName_SystemAttributeOnly_ReturnsDefaultName()
        {
            //ARRANGE
            var helper = new QueueTableHelper();
            object[] attributeObjects = new object[] { new System.FlagsAttribute() };

            //ACT
            Exception ex = Assert.Throws<System.Exception>(delegate { helper.GetQueueTableName(attributeObjects); });

            //ASSERT
            Assert.AreEqual(GetQueueTableNameException, ex.Message);
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
            Assert.AreEqual("ScheduleAgentQueue_" + guid, returnValue);
        }

        [Test]
        public void GetAgentGuid_GuidAttributeOnly_ReturnsCorrectName()
        {
            //ARRANGE
            Guid guid = new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D");
            var helper = new QueueTableHelper();
            object[] attributeObjects = new object[] { new GuidAttribute(guid.ToString()) };

            //ACT
            Guid returnValue = helper.GetAgentGuid(attributeObjects);

            //ASSERT
            Assert.AreEqual(guid, returnValue);
        }

        [Test]
        public void GetAgentGuid_SystemAttributeOnly_ReturnsException()
        {
            //ARRANGE
            var helper = new QueueTableHelper();
            object[] attributeObjects = new object[] { new System.FlagsAttribute() };

            //ACT
            Exception ex = Assert.Throws<System.Exception>(delegate { helper.GetAgentGuid(attributeObjects); });

            //ASSERT
            Assert.AreEqual(GetAgentGuidException, ex.Message);
        }

        [Test]
        public void GetAgentGuid_NoGuidAttribute_ReturnsException()
        {
            //ARRANGE
            var helper = new QueueTableHelper();
            object[] attributeObjects = new object[] { };

            //ACT
            Exception ex = Assert.Throws<System.Exception>(delegate { helper.GetAgentGuid(attributeObjects); });

            //ASSERT
            Assert.AreEqual("Could not retrieve Agent Guid.", ex.Message);
        }
    }
}

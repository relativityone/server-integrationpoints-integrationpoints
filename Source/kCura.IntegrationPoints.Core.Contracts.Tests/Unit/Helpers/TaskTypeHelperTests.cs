using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Contracts.Tests.Unit.Helpers
{
    class TaskTypeHelperTests
    {
        [Test]
        [TestCase(TaskType.SyncManager)]
        [TestCase(TaskType.ExportManager)]
        [TestCase(TaskType.ExportService)]
        public void ItShouldReturnMangerTypeTasks(TaskType mgrTaskType)
        {
            var mgrTaskTypes = TaskTypeHelper.GetManagerTypes();

            Assert.That(mgrTaskTypes.Contains(mgrTaskType));
        }
    }
}

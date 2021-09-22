using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    internal abstract class ConfigurationTestBase
    {
        protected IConfiguration _configuration;
        protected SyncConfigurationRdo _configurationRdo;

        [SetUp]
        protected void BaseSetup()
        {
            _configurationRdo = new SyncConfigurationRdo();
            _configurationRdo.JobHistoryType = DefaultGuids.JobHistory.TypeGuid;

            _configuration = new BasicConfigurationStub(_configurationRdo);
        }

        protected void SetupJobName(Mock<IObjectManager> objectManagerMock, string jobName)
        {
            objectManagerMock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                It.Is<QueryRequest>(q =>
                    q.IncludeNameInQueryResult == true &&
                    q.Condition == $"'ArtifactId' == {_configurationRdo.JobHistoryId}"), 0, 1)).ReturnsAsync(
                new QueryResult
                {
                    Objects = new List<RelativityObject>
                        {new RelativityObject {ArtifactID = _configurationRdo.JobHistoryId, Name = jobName}}
                });
        }
    }
}
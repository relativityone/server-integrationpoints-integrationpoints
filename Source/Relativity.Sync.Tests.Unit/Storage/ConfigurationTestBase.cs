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

namespace Relativity.Sync.Tests.Unit.Storage
{
    using RdoExpressionInt = Expression<Func<SyncConfigurationRdo, int>>;
    using RdoExpressionString = Expression<Func<SyncConfigurationRdo, string>>;
    using RdoExpressionGuid = Expression<Func<SyncConfigurationRdo, Guid>>;
    using RdoExpressionGuidNullable = Expression<Func<SyncConfigurationRdo, Guid?>>;

    [TestFixture]
    internal abstract class ConfigurationTestBase
    {
        protected Mock<IConfiguration> _configuration;
        protected SyncConfigurationRdo _configurationRdo;
        
        [SetUp]
        protected void BaseSetup()
        {
            _configurationRdo = new SyncConfigurationRdo();
            _configurationRdo.JobHistoryType = DefaultGuids.JobHistory.TypeGuid;

            _configuration = new Mock<IConfiguration>();

            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()))
                .Returns((Func<SyncConfigurationRdo, string> f) => f(_configurationRdo));

            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, int>>()))
                .Returns((Func<SyncConfigurationRdo, int> f) => f(_configurationRdo));
            
            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, int?>>()))
                .Returns((Func<SyncConfigurationRdo, int?> f) => f(_configurationRdo));

            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, Guid>>()))
                .Returns((Func<SyncConfigurationRdo, Guid> f) => f(_configurationRdo));

            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, Guid?>>()))
                .Returns((Func<SyncConfigurationRdo, Guid?> f) => f(_configurationRdo));

            _configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, bool>>()))
                .Returns((Func<SyncConfigurationRdo, bool> f) => f(_configurationRdo));
        }

        protected bool MatchMemberName(RdoExpressionInt expression, string memberName)
        {
            var memberExpression = ((expression.Body as UnaryExpression)?.Operand as MemberExpression)
                                   ?? (expression.Body as MemberExpression);

            return memberExpression.Member.Name == memberName;
        }
        
        protected bool MatchMemberName(RdoExpressionString expression, string memberName)
        {
            var memberExpression = ((expression.Body as UnaryExpression)?.Operand as MemberExpression)
                                   ?? (expression.Body as MemberExpression);

            return memberExpression.Member.Name == memberName;
        }
        
        protected bool MatchMemberName(RdoExpressionGuid expression, string memberName)
        {
            var memberExpression = ((expression.Body as UnaryExpression)?.Operand as MemberExpression)
                                   ?? (expression.Body as MemberExpression);

            return memberExpression.Member.Name == memberName;
        }
        
        protected bool MatchMemberName(RdoExpressionGuidNullable expression, string memberName)
        {
            var memberExpression = ((expression.Body as UnaryExpression)?.Operand as MemberExpression)
                                   ?? (expression.Body as MemberExpression);

            return memberExpression.Member.Name == memberName;
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
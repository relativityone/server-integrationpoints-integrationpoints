using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.InstanceSetting;
using InstanceSetting = Relativity.Services.InstanceSetting.InstanceSetting;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class KeplerInstanceSettingRepositoryTests : TestBase
    {
        private IInstanceSettingManager _instanceSettingManager;
        private KeplerInstanceSettingRepository _repository;

        public override void SetUp()
        {
            _instanceSettingManager = Substitute.For<IInstanceSettingManager>();
            var serviceMgr = Substitute.For<IServicesMgr>();
            serviceMgr.CreateProxy<IInstanceSettingManager>(ExecutionIdentity.System).Returns(_instanceSettingManager);
            _repository = new KeplerInstanceSettingRepository(serviceMgr);
        }

        [Test]
        public void ItShouldQueryInstanceSetting()
        {
            string section = "section_170";
            string name = "name_691";

            var instanceSetting = "instance_setting_687";

            _instanceSettingManager
                .QueryAsync(Arg.Is<Query>(x => x.Condition == CreateConditionString(section, name)), 1)
                .Returns(Task.FromResult(CreateSuccessWithValue(instanceSetting)));

            var actualInstanceSetting = _repository.GetConfigurationValue(section, name);

            Assert.That(actualInstanceSetting, Is.EqualTo(instanceSetting));

            _instanceSettingManager
                .Received(1)
                .QueryAsync(Arg.Is<Query>(x => x.Condition == CreateConditionString(section, name)), 1);
        }

        [Test]
        public void ItShouldThrowExceptionForFailure()
        {
            _instanceSettingManager
                .QueryAsync(Arg.Any<Query>(), 1)
                .Returns(Task.FromResult(CreateFailure()));

            Assert.That(() => _repository.GetConfigurationValue("section_289", "name_473"), Throws.TypeOf<NotFoundException>());
        }

        [Test]
        public void ItShouldHandleEmptyResult()
        {
            _instanceSettingManager
                .QueryAsync(Arg.Any<Query>(), 1)
                .Returns(Task.FromResult(CreateEmptySuccess()));

            var actualResult = _repository.GetConfigurationValue("section_791", "name_875");

            Assert.That(actualResult, Is.Null);
        }

        private string CreateConditionString(string section, string name)
        {
            var sectionCondition = new TextCondition(InstanceSettingFieldNames.Section, TextConditionEnum.EqualTo, section);
            var nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, name);

            return new CompositeCondition(nameCondition, CompositeConditionEnum.And, sectionCondition).ToQueryString();
        }

        private InstanceSettingQueryResultSet CreateSuccessWithValue(string value)
        {
            return new InstanceSettingQueryResultSet
            {
                Success = true,
                Results = new List<Result<InstanceSetting>>
                {
                    new Result<InstanceSetting>
                    {
                        Success = true,
                        Artifact = new InstanceSetting
                        {
                            Value = value
                        }
                    }
                }
            };
        }

        private InstanceSettingQueryResultSet CreateEmptySuccess()
        {
            return new InstanceSettingQueryResultSet
            {
                Success = true,
                Results = new List<Result<InstanceSetting>>()
            };
        }

        private InstanceSettingQueryResultSet CreateFailure()
        {
            return new InstanceSettingQueryResultSet
            {
                Success = false
            };
        }
    }
}

using System;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.InstanceSetting;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerInstanceSettingRepository : IInstanceSettingRepository
    {
        private readonly IServicesMgr _servicesMgr;
        public KeplerInstanceSettingRepository(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public string GetConfigurationValue(string section, string name)
        {
            var sectionCondition = new TextCondition(InstanceSettingFieldNames.Section, TextConditionEnum.EqualTo, section);
            var nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, name);
            var query = new Query
            {
                Condition = new CompositeCondition(nameCondition, CompositeConditionEnum.And, sectionCondition).ToQueryString()
            };

            using (var proxy = _servicesMgr.CreateProxy<IInstanceSettingManager>(ExecutionIdentity.System))
            {
                var result = proxy.QueryAsync(query, 1).Result;
                if (!result.Success)
                {
                    throw new NotFoundException($"Instance setting not found ({result.Message}).");
                }
                if (result.Results.Count == 0)
                {
                    return null;
                }
                return result.Results[0].Artifact.Value;
            }
        }
    }
}
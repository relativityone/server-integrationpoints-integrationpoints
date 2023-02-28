using System;
using System.Collections;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Helpers;

namespace kCura.ScheduleQueue.Core.TimeMachine
{
    public class DefaultAgentTimeMachineProvider : AgentTimeMachineProvider
    {
        private string agentKey = string.Empty;

        public DefaultAgentTimeMachineProvider()
        {
            Guid agentGuid = Guid.Empty;
            try { agentGuid = new QueueTableHelper().GetAgentGuid(); }
            catch { }
            SetKey(agentGuid);
        }

        public DefaultAgentTimeMachineProvider(Guid agentGuid)
        {
            Manager.Settings.ConfigCacheTimeout = 1;
            SetKey(agentGuid);
        }

        private static IDictionary _instanceSettings;

        protected static IDictionary InstanceSettings
            => _instanceSettings ??
            (_instanceSettings = Manager.Instance.GetConfig(IntegrationPoints.Domain.Constants.SCHEDULE_QUEUE_INSTANCE_SETTING_SECTION));

        private void GetTimeMachineData()
        {
            _enabled = false;
            _workspaceID = -1;
            _utcNow = DateTime.UtcNow;

            string value = ConfigHelper.GetValue<string>(InstanceSettings[agentKey]);
            if (!string.IsNullOrEmpty(value))
            {
                TimeMachineStruct tm = IntegrationPointSerializer.CreateWithoutLogger().Deserialize<TimeMachineStruct>(value);
                if (tm != null && tm.Date.HasValue)
                {
                    _utcNow = tm.Date.Value;
                    if (tm.CaseID > 0)
                    {
                        _workspaceID = tm.CaseID;
                    }
                    _enabled = true;
                }
            }
        }

        private void SetKey(Guid agentGuid)
        {
            agentKey = string.Format("TimeMachineAgent_{0}", agentGuid.ToString().ToUpperInvariant());
        }

        private bool _enabled = false;

        public override bool Enabled
        {
            get { GetTimeMachineData(); return _enabled; }
        }

        private int _workspaceID = -1;

        public override int WorkspaceID
        {
            get { GetTimeMachineData(); return _workspaceID; }
        }

        private DateTime _utcNow = DateTime.UtcNow;

        public override DateTime UtcNow
        {
            get { GetTimeMachineData(); return _utcNow; }
        }
    }

    public class TimeMachineStruct
    {
        public int CaseID { get; set; }

        public DateTime? Date { get; set; }
    }
}

using System;

namespace kCura.ScheduleQueue.Core.TimeMachine
{
    public abstract class AgentTimeMachineProvider
    {
        private static AgentTimeMachineProvider _current;
        private static readonly object _lock = new object();
        static AgentTimeMachineProvider()
        {
            ResetToDefault();
        }

        public static AgentTimeMachineProvider Current
        {
            get
            {
                lock (_lock)
                {
                    return _current;
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                lock (_lock)
                {
                    _current = value;
                }

            }
        }

        public abstract bool Enabled { get; }
        public abstract int WorkspaceID { get; }
        public abstract DateTime UtcNow { get; }

        public static void ResetToDefault()
        {
            Current = new DefaultAgentTimeMachineProvider();
        }
    }
}

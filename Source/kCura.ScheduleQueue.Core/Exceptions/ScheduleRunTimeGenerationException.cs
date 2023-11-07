using System;

namespace kCura.ScheduleQueue.Core.Exceptions
{
    internal class ScheduleRunTimeGenerationException : Exception
    {
        public ScheduleRunTimeGenerationException(string message) : base(message)
        {
        }

        public ScheduleRunTimeGenerationException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

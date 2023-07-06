using System;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using Relativity.Import.V1.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Utils
{
    internal static class EnumExtensions
    {
        public static JobEndStatus ToJobEndStatus(this ImportState state)
        {
            switch (state)
            {
                case ImportState.Completed: return JobEndStatus.Completed;
                case ImportState.Failed: return JobEndStatus.Failed;
                case ImportState.Canceled: return JobEndStatus.Canceled;
                default: throw new NotSupportedException($"ImportState is not supported - {state}.");
            }
        }
    }
}

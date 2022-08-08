using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
    public interface IExternalServiceSimpleInstrumentation
    {
        T Execute<T>(Func<T> functionToExecute);
        Task<T> ExecuteAsync<T>(Func<Task<T>> functionToExecute);
        void Execute(Action actionToExecute);
    }
}

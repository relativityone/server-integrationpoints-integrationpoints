using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
    internal class ExternalServiceSimpleInstrumentation : IExternalServiceSimpleInstrumentation
    {
        private readonly IExternalServiceInstrumentation _instrumentation;

        public ExternalServiceSimpleInstrumentation(IExternalServiceInstrumentation instrumentation)
        {
            _instrumentation = instrumentation;
        }

        public T Execute<T>(Func<T> functionToExecute)
        {
            IExternalServiceInstrumentationStarted startedInstrumentation = _instrumentation.Started();
            try
            {
                T result = functionToExecute();
                startedInstrumentation.Completed();
                return result;
            }
            catch (Exception ex)
            {
                startedInstrumentation.Failed(ex);
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> functionToExecute)
        {
            IExternalServiceInstrumentationStarted startedInstrumentation = _instrumentation.Started();
            try
            {
                T result = await functionToExecute().ConfigureAwait(false);
                startedInstrumentation.Completed();
                return result;
            }
            catch (Exception ex)
            {
                startedInstrumentation.Failed(ex);
                throw;
            }
        }

        public void Execute(Action actionToExecute)
        {
            IExternalServiceInstrumentationStarted startedInstrumentation = _instrumentation.Started();
            try
            {
                actionToExecute();
                startedInstrumentation.Completed();
            }
            catch (Exception ex)
            {
                startedInstrumentation.Failed(ex);
                throw;
            }
        }
    }
}

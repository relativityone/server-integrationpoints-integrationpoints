using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Constants;
using Relativity.API;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace kCura.IntegrationPoints.Common.Logger
{
    public class SerilogLoggerInstrumentationService : ISerilogLoggerInstrumentationService, IDisposable
    {
        private readonly IInstanceSettingsBundle _instanceSettingsBundle;
        private readonly IRipAppVersionProvider _ripAppVersionProvider;
        private readonly IAPILog _fallbackLogger;
        private ILogger _serilogLogger;

        private object _serilogLoggerLocker = new object();

        public SerilogLoggerInstrumentationService(IInstanceSettingsBundle instanceSettingsBundle, IRipAppVersionProvider ripAppVersionProvider, IAPILog fallbackLogger)
        {
            _instanceSettingsBundle = instanceSettingsBundle;
            _ripAppVersionProvider = ripAppVersionProvider;
            _fallbackLogger = fallbackLogger;
        }

        public void Dispose()
        {
            var disposable = _serilogLogger as IDisposable;
            disposable?.Dispose();
            _serilogLogger = null;
        }

        public ILogger GetLogger()
        {
            EnsureSerilogLogger();
            return _serilogLogger;
        }

        private void EnsureSerilogLogger()
        {
            lock (_serilogLoggerLocker)
            {
                if (_serilogLogger == null)
                {
                    try
                    {
                        _serilogLogger = CreateBaseLogger();
                    }
                    catch (Exception ex)
                    {
                        _fallbackLogger.LogWarning("Unable to instrument Serilog Logger.", ex);
                        _serilogLogger = Serilog.Core.Logger.None;
                    }
                }
            }
        }

        private ILogger CreateBaseLogger()
        {
            string apikey = GetRelEyeToken();
            if (string.IsNullOrEmpty(apikey))
            {
                _fallbackLogger.LogWarning("Serilog Logger cannot be initialized because ReleyeToken is not defined.");
                return Serilog.Core.Logger.None;
            }

            string otlpEndpoint = GetRelEyeUriLogs();
            if (string.IsNullOrEmpty(apikey))
            {
                _fallbackLogger.LogWarning("Serilog Logger cannot be initialized because ReleyeUriLogs is not defined.");
                _serilogLogger = Serilog.Core.Logger.None;
                return Serilog.Core.Logger.None;
            }

            string instanceIdentifier = GetInstanceIdentifier();

            ILogger serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.OpenTelemetry(
                    endpoint: otlpEndpoint,
                    protocol: OpenTelemetrySink.OtlpProtocol.HttpProtobuf,
                    headers: new Dictionary<string, string>()
                    {
                        { "apikey", apikey }
                    },
                    disableBatching: true)
                .Enrich.WithProperty(RelEye.Names.SourceId, instanceIdentifier.ToLower())
                .Enrich.WithProperty(RelEye.Names.ServiceName, RelEye.Values.ServiceName)
                .Enrich.WithProperty(RelEye.Names.ServiceVersion, _ripAppVersionProvider.Get())
                .Enrich.WithProperty(RelEye.Names.R1TeamID, RelEye.Values.R1TeamID)
                .Enrich.FromLogContext()
                .CreateLogger();
            _fallbackLogger.LogInformation("SerilogLogger.OpenTelemetry properly initialized.");
            return serilogLogger;
        }

        private string GetRelEyeToken()
        {
            return _instanceSettingsBundle.GetString(
                RelEye.InstanceSettings.REL_EYE_SECTION,
                RelEye.InstanceSettings.REL_EYE_TOKEN);
        }

        private string GetRelEyeUriLogs()
        {
            return _instanceSettingsBundle.GetString(
                RelEye.InstanceSettings.REL_EYE_SECTION,
                RelEye.InstanceSettings.REL_EYE_URI_LOGS);
        }

        private string GetInstanceIdentifier()
        {
            return _instanceSettingsBundle.GetString(
                RelEye.InstanceSettings.CORE_SECTION,
                RelEye.InstanceSettings.INSTANCE_IDENTIFIER);
        }
    }
}

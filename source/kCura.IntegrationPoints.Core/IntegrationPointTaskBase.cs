using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
  public class IntegrationPointTaskBase
  {
    internal ICaseServiceContext _caseServiceContext;
    internal readonly IHelper _helper;
    internal IDataProviderFactory _dataProviderFactory;
    internal kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
    internal JobHistoryService _jobHistoryService;
    internal JobHistoryErrorService _jobHistoryErrorService;
    internal kCura.IntegrationPoints.Contracts.ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
    internal IJobManager _jobManager;


    public IntegrationPointTaskBase(
      ICaseServiceContext caseServiceContext,
      IHelper helper,
      IDataProviderFactory dataProviderFactory,
      kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
      kCura.IntegrationPoints.Contracts.ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
      JobHistoryService jobHistoryService,
      JobHistoryErrorService jobHistoryErrorService,
      IJobManager jobManager)
    {
      _caseServiceContext = caseServiceContext;
      _helper = helper;
      _dataProviderFactory = dataProviderFactory;
      _serializer = serializer;
      _appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
      _jobHistoryService = jobHistoryService;
      _jobHistoryErrorService = jobHistoryErrorService;
      _jobManager = jobManager;
    }

    internal Data.IntegrationPoint IntegrationPoint { get; set; }
    internal Data.JobHistory JobHistory { get; set; }
    internal Guid BatchInstance { get; set; }

    private SourceProvider _sourceProvider;
    protected Data.SourceProvider SourceProvider
    {
      get
      {
        if (this.IntegrationPoint == null)
        {
          throw new ArgumentException("Integration Point Rdo has yet to be retrieved.");
        }
        if (_sourceProvider == null)
        {
          _sourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(this.IntegrationPoint.SourceProvider.Value);
        }
        return _sourceProvider;
      }
    }

    private DestinationProvider _destinationProvider;
    protected Data.DestinationProvider DestinationProvider
    {
      get
      {
        if (this.IntegrationPoint == null)
        {
          throw new ArgumentException("Integration Point Rdo has yet to be retrieved.");
        }
        if (_destinationProvider == null)
        {
          _destinationProvider = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(this.IntegrationPoint.DestinationProvider.Value);
        }
        return _destinationProvider;
      }
    }

    internal void SetIntegrationPoint(Job job)
    {
      if (this.IntegrationPoint != null) return;

      int integrationPointID = job.RelatedObjectArtifactID;
      this.IntegrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
      if (this.IntegrationPoint == null)
      {
        throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
      }
    }

    internal void SetJobHistory()
    {
      if (this.JobHistory != null) return;

      this.JobHistory = _jobHistoryService.CreateRdo(this.IntegrationPoint, this.BatchInstance, DateTime.UtcNow);
      _jobHistoryErrorService.JobHistory = this.JobHistory;
      _jobHistoryErrorService.IntegrationPoint = this.IntegrationPoint;
    }

    internal virtual string GetSourceConfiguration(string originalSourceConfiguration)
    {
      return originalSourceConfiguration;
    }

    internal virtual IEnumerable<FieldMap> GetFieldMap(string serializedFieldMappings)
    {
      IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(serializedFieldMappings);
      fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
      return fieldMap;
    }

    internal virtual List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
    {
      return fieldMap.Select(f => f.SourceField).ToList();
    }

    internal virtual List<FieldEntry> GetDestinationFields(IEnumerable<FieldMap> fieldMap)
    {
      return fieldMap.Select(f => f.DestinationField).ToList();
    }

    internal virtual IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
    {
      var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
      return new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
    }

    internal virtual IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
    {
      Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
      Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
      IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(applicationGuid, providerGuid, _helper);
      return sourceProvider;
    }

    internal virtual IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
    {

      Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
      var factory = _appDomainRdoSynchronizerFactoryFactory as GeneralWithCustodianRdoSynchronizerFactory;
      if (factory != null)
      {
        factory.TaskJobSubmitter = new TaskJobSubmitter(_jobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance);
        factory.SourceProvider = SourceProvider;
      }
      IDataSynchronizer sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
      return sourceProvider;
    }

    internal IEnumerable<string> GetRecipientEmails()
    {
      string emailRecipients = string.Empty;
      try { emailRecipients = IntegrationPoint.EmailNotificationRecipients; }
      catch { }
      IEnumerable<string> emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim());
      return emailRecipientList;
    }
  }
}
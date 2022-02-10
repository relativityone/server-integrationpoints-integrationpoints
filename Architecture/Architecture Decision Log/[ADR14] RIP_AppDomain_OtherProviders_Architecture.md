# Other Providers and AppDomain Architecture in Integration Points

## Status

Proposed

## Context

Custom Providers (Other Providers) in Integration Points enable to ingest data from customer defined input data set into Relativity Dynamic Objects (RDOs). Client builds own RAP using Integration Points SDK (<https://git.kcura.com/projects/IN/repos/integrationpoints-sdk/browse>). The most crucial part of the Custom Provider for Integration Points is to satisfy [IDataSourceProvider](https://git.kcura.com/projects/IN/repos/integrationpoints-sdk/browse/source/Relativity.IntegrationPoints.Contracts/Provider/IDataSourceProvider.cs).

## Custom Provider in Integration Poins Workflow

Overall working flow for Custom Providers in Integration Points is the same as in FTP/LDAP Providers. Only difference is that FTP/LDAP Providers are part of RIP application so it's the same codebase and the `IDataSourceProvider` creation is different which will be explained in next paragraphs.

Custom Providers in RIP are processed in batches, as many smaller jobs. Default batch size is set to **1000**, it's historical past and we weren't brave enough to increase this value which seems to be reasonable (100k docs ends up with 100 batches).

When the job is run the record with job details and type _SyncManager_ is written in to _[EDDS].[ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]_ from where the Agent picks it up. The first job is processed by `SyncManager` task.

### [SyncManager](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Agent/Tasks/SyncManager.cs)

`SyncManager` is responsible for batching the job to smaller batches (by 1000). The code calls `IDataSourceProvider.GetBatchableIds` which retrieve all IDs specified in customer input data set. Then `SyncManager` inserts new job in to the same queue with _SyncWorker_ type and 1000 IDs in details.

### [SyncWorker](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Agent/Tasks/SyncWorker.cs)

`SyncWorker` imports actual data in to Relativity. At first it takes IDs defined in details and then calls `IDataSourceProvider.GetData` for that IDs to retrieve the values based on configure fields mapping. Once the data are retrieved `IDataReader` is created which is passed into ImportAPI. Once the import is finished the job is removed from the queue and status is updated in job-specific resource table where all statuses are gathered to serve end result at the end for the customer.

### Custom Provider in Integration Points Installation

In theory registering Custom Provider in Integration Points is just insert/update object in _SourceProvider_ with all required informations. There is one tiny detail which may lead to problems and it's PreInstallation validation which actually tries to resolve `IDataSourceProvider` object for Custom Provider and install it only when it will be successfully resolved. It helps users with development and follow principle "Fail Fast" which prevent user with installation when dependencies won't be satisfied and container won't be able to resolve `IDataSourceProvider` during Integration Point configuration or actual job run.

Every Custom Provider should implement own Event Handler to register in Integration Points application - [Register Custom Provider Event Handler Documentation](https://platform.relativity.com/RelativityOne/Content/Relativity_Integration_Points/Build_your_first_integration_point.htm#Adding7)

User needs to satisfy [IntegrationPointSourceProviderInstaller](https://git.kcura.com/projects/IN/repos/integrationpoints-sdk/browse/source/Relativity.IntegrationPoints.SourceProviderInstaller/IntegrationPointSourceProviderInstaller.cs). Here is the sample based on Azure AD Provider - [Azure AD Register Provider](https://git.kcura.com/projects/IN/repos/aadprovider/browse/Source/kCura.IntegrationPoints.AADProvider/EventHandlers/RegisterProvider.cs).

Without unnecessary implementation details the code is calling [KeplerSourceProviderInstaller.SendInstallProviderRequestWithRetriesAsync](https://git.kcura.com/projects/IN/repos/integrationpoints-sdk/browse/source/Relativity.IntegrationPoints.SourceProviderInstaller/Internals/KeplerSourceProviderInstaller.cs#48), where under the hood [IProviderManager.InstallProviderAsync](https://git.kcura.com/projects/IN/repos/integrationpoints-keplerservicesinterfaces/browse/source/Relativity.IntegrationPoints.Services.Interfaces.Private/IProviderManager.cs#47) kepler is called. This kepler is registered with Integration Points installation and the interface implementation is carried by Integration Points code and there real magic happens - [ProvideManager.InstallProviderAsync](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/Relativity.IntegrationPoints.Services/ProviderManager.cs#119).

#### **ProviderManager.InstallProviderAsync**

Custom Provider Installation was implemented using [Monad Patter](https://en.wikipedia.org/wiki/Monad_(functional_programming)) and _LanguateExt_ nuget package. It's in general facade for `IRipProviderInstaller` which installs Register Custom Provider in Integration Points.

The validation mentioned above is implemented in [RipInstallerProvider.ValidateProvider](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Core/Provider/RipProviderInstaller.cs#101) and calls `IDataProviderFactory.GetDataProvider` where whole AppDomain creation and dependencies registartion code is executed. All details related to AppDomains and `IDataProviderFactory` interface implementation will be covered in **Integration Points AppDomain Architecture**.'

## Integration Points AppDomain Architecture

The interface used across whole Integration Points project to interact with Custom Providers code is `IDataProviderFactory` with the implementing class [DataProviderBuilder](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Core/Services/Domain/DataProviderBuilder.cs):

```csharp
 internal class DataProviderBuilder : IDataProviderFactory
 {
  private readonly ProviderFactoryVendor _providerFactoryVendor;

  public DataProviderBuilder(ProviderFactoryVendor providerFactoryVendor)
  {
   _providerFactoryVendor = providerFactoryVendor;
  }

  public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
  {
   IDataSourceProvider provider = CreateProvider(applicationGuid, providerGuid);
   return WrapDataProviderInSafeDisposeDecorator(provider);
  }

  private IDataSourceProvider CreateProvider(Guid applicationGuid, Guid providerGuid)
  {
   IProviderFactory providerFactory = _providerFactoryVendor.GetProviderFactory(applicationGuid);
   IDataSourceProvider provider;
   try
   {
    provider = providerFactory.CreateProvider(providerGuid);
   }
   
   ...

   return provider;
  }
 }
```

Here are crucial classes where most important business logic is executed:

* [ProviderFactoryVendor](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Core/Services/Domain/ProviderFactoryVendor.cs) - Prepare and cache factories for executed Custom Providers
* [ProviderFactoryLifecycleStrategy](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Core/Services/Domain/ProviderFactoryCreationStrategy.cs) - Determine if new AppDomain should be spun up and load assemblies into domain (new or existing one)
* [AppDomainHelper](https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Domain/AppDomainHelper.cs) - New AppDomain creation and configuration logic

### ProviderFactoryLifesycleStrategy

In this class we determine if new AppDomain should be created. The base statement is if the Provider is internal (FTP/LDAP) or external (Azure AD etc.) - in general if RAP Guid is Integration Points or not. When it's internal we load client libraries using `IAppDomainHelper.LoadClientLibraries` (details about what DLLs are loaded will be covered in paragraph related to `AppDomainHelper` class). When the provider is external we create brand new AppDomain from scratch load all required assemblies and bootstrap the domain using `Bootstrappers.AppDomainBootstrapper.Bootstrap` method from _Relativity.API_ nuget package

### AppDomainHelper

`IAppDomainHelper` interface:

```csharp
 public interface IAppDomainHelper
 {
   void LoadClientLibraries(AppDomain domain, Guid applicationGuid);
   void ReleaseDomain(AppDomain domain);
   AppDomain CreateNewDomain();
   IAppDomainManager SetupDomainAndCreateManager(AppDomain domain, Guid applicationGuid);
   T CreateInstance<T>(AppDomain domain, params object[] constructorArgs) where T : class;
 }
```

* `CreateNewDomain` :

  Create new AppDomain in _C:\Users\relserviceaccount\AppData\Local\Temp\RelativityIntegrationPoints\RelativityIntegrationPoints{guid}_ and load DLLs there.

  * `DeployLibraryFiles` :

    Copy DLLs to AppDomain folder from from below directories in following order:

    * C:\\Program Files\\kCura Corporation\\Relativity\\Library
    * C:\\Program Files\\kCura Corporation\\Relativity\\WebProcessing (only files start with _kCuraAgent_)
    * C:\\Program Files\\kCura Corporation\\Relativity\\EDDS\\bin (only files start with _FSharp.Core_)

    All copy operations are executed with _overwriteFiles_ set to _true_

    Paths are generated based on Registry Key stored on Agent Machine - _HKEY_LOCAL_MACHINE\\SOFTWARE\\kCura\\Relativity\\FeaturePaths\\BaseInstallDir_

  * `LoadRequiredAssemblies` :

    From Agent specific AppDomain (C:\\Users\\relserviceaccount\\AppData\\Local\\Temp\\Agent\\RelativityTmpAppDomain_{AgentId}_{Guid}) we retrieve and write to created AppDomain (if they don't already exist)

    * kCura.IntegrationPoints.Domain.dll
    * Relativity.IntegrationPoints.Contracts.dll

    After that we load _kCura.IntegrationPoints.Domain_ assembly into domain. We create `AssemblyDomainLoader` which is responsible for resolving assemblies and then we assign `AssemblyDomainLoader.ResolveAssembly` to our AppDomain `AssemblyResolve` event.

* `SetupDomainAndCreateManager` :

  This class prepares `IProviderFactory` which under the hood register `IDataSourceProvider` implemented by Custom Provider.

  First we call `LoadClientLibraries` (described below) and then call `AppDomainManager.Init` which prepare `IProviderFactory`

  At the end method executes `Bootstrappers.AppDomainBootstrapper.Bootstrap` for created AppDomain

* `LoadClientLibraries` :

  This method retrieves assemblies from created domain. During debugging against Test VM following assemblies were returned:

  * mscorelib
  * System
  * System.Core
  * kCura.IntegrationPoints.Domain (which were loaded in `LoadRequiredAssemblies`)

### Agents in Kubernetes

In Kubernetes we don't have access to predefined Key Registry, which were present in in-tenant infrastructure. Due to that we decided to copy assemblies from Working Directory which is the path in which the code is executed.

### Release Domain

Temporarily created AppDomain is released after the job is processed. In classic tenant Agent architecture we have one Agent Domain and many temporarily domains created and released on demand when the job is processed.

## Why separate AppDomain

We don't know intention behind separate AppDomain creation for Custom Providers in Integration Points. We can only guess that there is a risk of dlls missmatch between Integration Points DLLs and Custom Provider (client) DLLs. What also maybe the cause is to separate client's custom DLLs from our own Integration Points DLLs.

## Issues

When ADS Team rolled out Agents in K8s it started to fail Custom Providers installations, because in K8s the registry _HKEY_LOCAL_MACHINE\\SOFTWARE\\kCura\\Relativity\\FeaturePaths\\BaseInstallDir_ doesn't exist anymore. We decided then to copy DLLs from Working Directory, which temporarily resolved the problem. After Relativity release issue related to missing DLLs backed. It was caused by `Bootstrappers.AppDomainBootstrapper.Bootstrap` (<https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Domain/AppDomainHelper.cs#166>).

## Ideas

1. Understand what `Bootstrappers.AppDomainBootstrapper.Bootstrap` method does and rethink if this call is needed for temporarily created AppDomain
  
    * After removing `Bootstrappers.AppDomainBootstrapper.Bootstrap` method execution the code still worked. We can assume that this method is no longer needed for Integration Points

2. Instead of creating new AppDomain we could operate on the one provided by Agent framework - _RelativityTmpAppDomain_{AgentId}_{Guid}_. We could load there client DLLs and operate on them.

    * I tested CurrentDomain usage instead of creating new one, but it failed on releasing the domain, which is reasonable, because we should not release it before execution finish. Anyway following Agent Framework in K8s guidelines it would be best to get rid off AppDomains in k8s and operate on single provided. We could go in this way, but only if it will be guarented that AppDomain is released after job finish.

## Consequences

Get rid off temporarily AppDomain may have unpredictable consequences for Custom Provider jobs, because we don't own client's DLLs and don't have full visibility what DLLs are included in Custom Provider RAP. On the other hand maybe it's good time to make things right and consider using exising domain _RelativityTmpAppDomain_{AgentId}_{Guid}_ and operate on it. Only question is how and when this AppDomain is released. In Integration Points we execute jobs in the loop until there is no more jobs to process. If this domain wouldn't be released after job process there is a risk that in case of two different Custom Providers jobs we would mix DLLs coming from both RAPs which is definitely undesirable behavior. If _RelativityTmpAppDomain_{AgentId}_{Guid}_ is released after every `AgentBase.Execute` method call we could consider processing only one job per `AgentBase.Execute`.

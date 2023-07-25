# Relativity Sync

A library for syncing documents and Relativity Dynamic Objects (RDOs) between Relativity workspaces.

## Overview

Relativity Sync utilizes the Relativity Import API, Export API, and various Kepler-based APIs to transfer documents (and eventually objects) between workspaces.

The workspace-to-workspace workflow was originally designed as a part of [Relativity Integration Points (RIP)](https://git.kcura.com/projects/IN/repos/integrationpoints/browse). Relativity Sync is currently packaged with RIP and is still only differentiated as a separate flow within it.

Relativity Sync will add a few features to the old RIP workflow:

- Batching and parallel imports
- Interfaces to receive detailed job progress reporting

### Structure

Relativity Sync uses the [Banzai](https://github.com/eswann/Banzai) framework to structure its workflow.

- Sync builds a pipeline which executes a series of steps. See `Relativity.Sync.PipelineBuilder` for how it is structured.
- Each of these steps inherits from `Banzai.INode<SyncExecutionContext>`. Every node except for the root node is either a `Relativity.Sync.Nodes.SyncMultiNode` or inherits from `Relativity.Sync.Nodes.SyncNode<T>`, which are Sync's implementations of that interface.
  - `SyncNode<T>` requires `T` be a subtype of `Relativity.Sync.IConfiguration`. `SyncNode<T>` takes `Relativity.Sync.ICommand<T>` as a dependency.
  - `SyncMultiNode` does not require its own configuration interface. It defines several child nodes to execute in parallel. See `PipelineBuilder.cs`.
  - The major implementation of `ICommand<T>` is `Relativity.Sync.Command<T>`, which takes `Relativity.Sync.IExecutor<T>` and `Relativity.Sync.IExecutionConstrains<T>` as dependencies.
  - Each `IExecutor<T>` and `IExecutionConstrains<T>` is implemented separately, and are unique to each step.
    - `IExecutionConstrains<T>` implementations determine whether or not a step should be run; they are invoked by `Command<T>.CanExecuteAsync`.
    - `IExecutor<T>` implementations contains the actual logic for each step; they are invoked by `Command<T>.ExecuteAsync`.

### Usage

1. Reference the `Relativity.Sync` NuGet package or `Relativity.Sync.dll` (if building locally).
1. Create an instance of `Relativity.Sync.SyncJobFactory`.
1. The rest of integration with Relativity Sync involves constructing the appropriate arguments to `SyncJobFactory.Create`:
    1. `Autofac.IContainer`: This is an Autofac container with appropriate registrations. Since Relativity Sync is still in development, you will need to register implementations for the following dependencies:
        - `Relativity.Sync.I*Configuration`
          - These are the configuration interfaces for each step. The underlying object is expected to act as a POCO, so you can implement all of these as part of one configuration object if needed.
    1. `Relativity.Sync.SyncJobParameters`: This defines the RIP job, namely the job's ArtifactID + the ArtifactID of the source workspace.
    1. `Relativity.Sync.RelativityServices`: This defines a set of Relativity API dependencies required by Sync. These should be available from inside any Relativity API integration point (e.g. agent, event handler, etc.).
    1. (optional) `Relativity.Sync.SyncJobExecutionConfiguration`: These are configurations for tweaking the performance of Relativity Sync, mainly around batch sizes and parallelization. In almost all cases you should use the defaults.
    1. (optional) `Relativity.Sync.ISyncLog`: The root logger for the Sync job. By default this will be an empty logger, but you must provide an implementation to get any logging from the Sync framework.
1. Once you have the appropriate arguments, create an `ISyncJob` using the `SyncJobFactory.Create` method that's appropriate for your use case, and then invoke the job using `ISyncJob.ExecuteAsync`.
    - **NB**: There are also `RetryAsync` methods, but job retry is not yet supported.
    - You may provide an `IProgress<>` implementation to `ExecuteAsync` to get progress reporting; otherwise, Sync will not report any progress.
    - You may also provide a `CancellationToken` to `ExecuteAsync`, but cancellation is only partially supported. Namely, **individual Import API jobs are not yet cancellable**. Since Relativity Sync batches out several Import API jobs you may be able to partially cancel document transfer, but jobs that are in progress when cancellation occurs will complete before the job is fully cancelled. Import API job cancellation is expected to be supported in a future release.

## Repository

The main solution is `Source\Relativity.Sync.sln`. It references all projects in this repository.

- **Projects**
  - `Source\Relativity.Sync\`
    - Project containing the main Sync logic. The only project whose output is published.
  - `Source\Relativity.Sync.Tests.Common\`
    - Project containing logic common to other test projects.
  - `Source\Relativity.Sync.Tests.Integration\`
    - Project containing integration tests for Relativity Sync. Integration tests should be able to be run without outside dependencies (e.g. a Relativity instance).
  - `Source\Relativity.Sync.Tests.Performance\`
    - Project containing performance tests for Relativity Sync.
  - `Source\Relativity.Sync.Tests.System\`
    - Project containing tests for Relativity Sync that run against an instance of Relativity. System tests may rely on a certain version of Relativity, but shouldn't rely on a version of e.g. the Integration Points application.
  - `Source\Relativity.Sync.Tests.Unit\`
    - Project containing unit tests for Relativity Sync.
- **Scripts**
  - `build_jenkins.ps1`
    - Build script for Relativity Sync. Wrapper around `default.ps1`.
  - `default.ps1`
    - PSake task file for running build-related tasks in the repository. Should be called through `build_jenkins.ps1`.
  - `Jenkinsfile`
    - Defines the Jenkins build pipeline for Relativity Sync. See the [Build Pipeline](#build-pipeline) section for more details.
  - `Jenkinsfile.*`
    - Defines non-standard Jenkins pipelines, e.g. for performance tests or nightly system tests. See the [Build Pipeline](#build-pipeline) section for more details.
  - `scripts\*.ps1`
    - Build scripts invoked by `default.ps1`.
  - `scripts\installRaid.groovy`
    - Helper library for Jenkinsfile.
- **Paket**
  - `paket.dependencies`
    - Defines the [Paket](https://fsprojects.github.io/Paket/) dependencies for the Relativity Sync project.
  - `paket.lock`
    - Defines the exact Paket dependency versions.
  - `Source\*\paket.references`
    - Defines the Paket references for a given project.
  - `Source\*\paket.template`
    - Defines the NuGet package that is generated by Paket during the build. See [Artifacts](#artifacts).

## Development
### Build 

## Build Tasks

This repository builds with Powershell through the `.\build_jenkins.ps1` script. 
It supports standard tasks like `.\build_jenkins.ps1 compile`, `.\build_jenkins.ps1 test`, and `.\build_jenkins.ps1 package`.

## Online Documentation

For more information on Library CD, [view the documentation in Einstein](https://einstein.kcura.com/x/Kj-bC)

### Test

Sync defines three kinds of tests - unit, integration, and system.
- **Unit** tests cover the behavior of individual methods and properties of single classes. All dependencies should be mocked.
- **Integration** tests cover the behavior of one or more classes acting in concert. System dependencies (e.g. Relativity APIs, file system) should be mocked, but generally classes defined in Sync should be used.
- **System** tests cover the behavior of several classes acting in concert against a running instance of Relativity. Some components may still be mocked, e.g. logging, metrics, and nodes outside of the test scope.

Unit tests are run as part of the default build; otherwise, you can run them in Visual Studio or using the `runUnitTests` build task:

    > .\build_jenkins.ps1 runUnitTests

You can run integration tests in Visual Studio or using the `runIntegrationTests` build task:

    > .\build_jenkins.ps1 runIntegrationTests

To run system tests, you can update the SUT hostname in `app.config` in Relativity.Sync.Tests.System to run them in Visual Studio, or run the `runSystemTests` build tasks with the `-sutAddress` parameter:

    > .\build_jenkins.ps1 runSystemTests -sutAddress p-dv-vm-dog2bog

### Artifacts

The main output of Relativity Sync is the `Relativity.Sync` NuGet package, which is created by Paket and is defined in the appropriate [paket.template file](.\Source\Relativity.Sync\paket.template).

The NuGet package is created as part of the default build.

Versions of the package with an `beta` infix are published from short-lived feature branches, and ones with a `dev` infix are published from `develop`. To see the complete package naming logic, see the [get-version.ps1 script](.\scripts\get-version.ps1).

### Build Pipeline

Relativity Sync is built in a [multibranch Jenkins pipeline](https://jenkins.kcura.corp/job/DataTransfer/job/RelativitySync/job/RelativitySync/). `alpha` builds are published from short-lived feature branches, and `beta` builds from the `develop` branch.

Relativity Sync also has other pipelines, in particular a nightly one for executing system tests. See the [RelativitySync Jenkins job folder](https://jenkins.kcura.corp/job/DataTransfer/job/RelativitySync/) for more details.

## Contributing

1. Create a short-lived feature branch off of the latest `develop` with the appropriate ticket number somewhere at the start of the name (e.g. `REL-12345-create-foobar`, or just `REL-12345`).
1. Make and test your changes locally.
    1. You should have comprehensive **unit test** coverage over any new classes.
    1. There should also be basic **integration test** coverage over golden flows and negative scenarios.
    1. If you are updating or creating a new workflow or new widely-used class, then you may need to create **system tests** to cover that as well.
1. Push up your branch, and create a PR into `develop`. Add members of the Codigo o Plomo team to the PR. You will be able to merge once the pipeline is green.

## Miscellaneous

You can find more information about this project including troubleshooting steps and limitations on [Einstein](https://einstein.kcura.com/x/TSO3C).

## History  

The server-main branch was forked for Relativity Server by the Server Vertical.
Tag: 0.9.182                                        
Branch: server-main
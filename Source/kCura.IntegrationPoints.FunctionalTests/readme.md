# Integration Points - Tests

## Unit

TBD

## Integration

Integration Tests should be testing real production code with mocked dependencies if needed. We shouldn't mock any of production code classes except the cases when it's neccessary (e.g direct sqls)

### Folder Structure

```()
.
+-- Helpers
|   +-- HelperManager.cs
+-- Mocks
|   +-- Kepler
|       +-- WorkspaceManagerStub.cs
|       +-- ObjectManagerStub.*.cs
|   +-- ProxyMock.cs
|   +-- QueryManagerMock.cs
|   +-- ScheduleTestAgent.cs
|   +-- TestHelper
+-- Models
+-- Tests
+-- InMemoryDatabase.cs
+-- TestsBase.cs
```

### Kepler Concept

Kepler calls should be mocked directly in testing framework and as much as possible rely on real data stored in `InMemoryDatabase`. Mock setups should be done in corresponding mock classess.

**Note:** Object Manager due wide usage range has been implemented as partial class where each of sub-classes is responsible for different **Type**

Method used in production code should be directly mapped to mock implementation. E.g:

**Production code:**

```{csharp}
...
using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
{
    var request = new ReadRequest()
    {
        Object = new RelativityObjectRef() { ArtifactID = integrationPointId }
    };

    ReadResult result = await proxy.ReadAsync(workspaceId, request).ConfigureAwait(false);
}
...
```

**Mock setup:**

```{csharp}
Mock.Setup(x => x.ReadAsync(workspace.ArtifactId, It.Is<ReadRequest>(r =>
        r.Object.ArtifactID == integrationPoint.ArtifactId)))
    .Returns(() =>
        {
            var result = Database.IntegrationPoints.Exists(x => x.ArtifactId == request.Object.ArtifactID)
                ? new ReadResult {Object = new RelativityObject()}
                : new ReadResult {Object = null};

            return Task.FromResult(result);
        }
    );
```

Every new Kepler Mock instance should be registered in `ProxyMock` and `TestHelper` constructor.

Keplers setup should be registered within every new Instance in database if needed. E.g.:

```()
When adding new Integration Point instance to database, we should also register specific mock implementations for this instance (ReadAsync, etc.).
```

### Test Models

In Testing Framework has Test Models has been introduced. They are corresponding with specific record in Database. E.g. `JobTest` is 1:1 with _ScheduleAgentQueue_ in Relativity Instance.

**Note:** Models shouldn't reffer to any production code concepts except enums

Models should be created with _Test_ suffix to not create production code object by accident and for easier navigation through the test code. They can have much more properties if needed to simulate real relations beetween object. E.g. SavedSearch beside standard Relativity Instance properties can have reference to List of documents which belongs to this saved search for easier Kepler Mocking.

### InMemoryDatabase & TestBase

It's mock implementation for Relativity Instance Database. It's point of true for current Test state. Every integration test should derive from TestBase class which initialize Testing Framework and register production code classes for further usage.

## Functional

TBD

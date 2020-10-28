# Parallel Workspace Factory

## Status

Proposed

## Context

Currently in Integration and UI Tests we waste much time for creating Source and Destination Workspaces. Approximately time for one workspace creation is ~45 sec. One of the possible solution could be creating the workspaces in the background and get when needed - _Workspace Pool_

## Decision

Approach proposed in this study is based on - **Object Pool pattern**.

### Definiton

    The object pool pattern is a software creational design pattern that uses a set of initialized objects kept ready to use – a "pool" – rather than allocating and destroying them on demand. A client of the pool will request an object from the pool and perform operations on the returned object. When the client has finished, it returns the object to the pool rather than destroying it; this can be done manually or automatically.

    Object pools are primarily used for performance: in some circumstances, object pools significantly improve performance. Object pools complicate object lifetime, as objects obtained from and returned to a pool are not actually created or destroyed at this time, and thus require care in implementation.

One big difference is that we would destroy the object after using it and back to the pool the new one creation request.

Below has been proposed implementation of such concept:

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        ObjectPool<MyClass> objPool = new ObjectPool<MyClass>(() => Task.Run(() => new MyClass()));
        while (true)
        {
            MyClass obj = await objPool.Get();
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}

public class MyClass
{
    public int Id;

    public MyClass()
    {
        Thread.Sleep(TimeSpan.FromSeconds(5));

        Random random = new Random();
        Id = random.Next();
    }
}

public class ObjectPool<T> where T : new()
{
    private readonly Func<Task<T>> _createNext;
    private readonly ConcurrentQueue<Task<T>> _items = new ConcurrentQueue<Task<T>>();

    private int MAX = 7;

    public ObjectPool(Func<Task<T>> createNext)
    {
        _createNext = createNext;

        Init();
    }

    public void Init()
    {
        for (int i = 0; i < MAX; i++)
        {
            _items.Enqueue(_createNext());
        }
    }

    public Task<T> Get()
    {
        if (_items.TryDequeue(out var item))
        {
            _items.Enqueue(_createNext());

            return item;
        }
        else
        {
            throw new Exception();
        }
    }
}
```

During initialization Pool would create predefined number of tasks for workspace creation requests. They would be created in background and retrieved on demand when client would need this. After getting the workspace from the pool the next creation request would be pushed at the end of the queue.

This solution is thread safe by using ``ConcurrentQueue<T>`` type. If exception occurs during workspace creation it affect only one particular test instead of whole suite.

Workspace Pool object would be created as singleton, it enables to share the workspaces between various test suites.

### RIP Code

Possible approaches:

1. Modify ``Workspace.cs`` to retrieve workspaces from _Workspace Pool_ object instead of RSAPI

2. Implement _Workspace Pools_ usage directly in TestsBase classes (``SourcePorivderTemplate``, ``RelativityProviderTemplate``)

Perhaps first approach would be faster to treat _Workspace Pool_ as replacement for RSAPI, but it would leave code base bad as it was. Following Ascent initiative it would be nice prolog to make RIP Tests better place.

## Consequences

(+) Significantly decrease test duration (faster PR merges and nightlys)

(+) Remove RSAPI code for workspace creation

# SignalR Proxy

Strongly typed SignalR Core client generator for SignalR based on [Castle Core](https://github.com/castleproject/Core).

## 60 sec intro

Imagine you have the following hub definition.

```csharp
public interface IHubMethods
{
    Task Method1(int x);
    Task<int> Method2();
}

public interface IHubEvents
{
    Task Event(int x, string y);
}

public class MyHub : Hub<IHubEvents>, IHubMethods
{
    // skipped for brevity
}
```

The `MyHub` class exposed methods of `IHubMethods` to clients via SignalR. Additionally, hub may only send events as defined
in the `IHubEvents`. Both interfaces can be shared to clients. The client will then use them in the following way.

```csharp
var connection = new HubConnectionBuilder()
    /* setup the connection the SignalR way */
    .Build();

await connection.Start();

HubProxy<IHubMethods> proxy = SignalRProxyGenerator.CreateProxy<IHubMethods>(connection);
IHubMethods client = proxy.Client;

await client.Method1(5);
// Method without result are translated as `Send` hub calls

var res = await client.Method2();
// Methods with result are translated as `Invoke` hub calls

IHubEvents events; // = ...
var subscription = proxy.Subscribe(events);
// IHubEvents.Event method will be called when server sends the appropriate message

subscription.Dispose();
// Registered handler can be disconnected
```

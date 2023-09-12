# More read:
[.NET In-Process Azure Durable Functions: Preserve Stack Order When Passing Between Orchestrators, Activities etc](https://jaliyaudagedara.blogspot.com/2023/09/net-in-process-azure-durable-functions.html)

Reproducing the issue,
```csharp
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // TODO: To reproduce the issue, comment this line out
        builder.Services.AddSingleton<IMessageSerializerSettingsFactory, CustomMessageSerializerSettingsFactory>();
    }
}
```
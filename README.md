## .NET Azure Durable Functions: Preserve Stack Order When Passing Between Orchestrators, Activities etc

### In-Process:
[.NET In-Process Azure Durable Functions: Preserve Stack Order When Passing Between Orchestrators, Activities etc](https://jaliyaudagedara.blogspot.com/2023/09/net-in-process-azure-durable-functions.html)

Reproducing the issue,
```csharp
public class Startup : FunctionsStartup
{
	public override void Configure(IFunctionsHostBuilder builder)
	{
		// TODO: To reproduce the issue, comment out the following
		builder.Services.AddSingleton<IMessageSerializerSettingsFactory, CustomMessageSerializerSettingsFactory>();
	}
}
```

### Isolated:
[.NET Isolated Azure Durable Functions: Preserve Stack Order When Passing Between Orchestrators, Activities etc](https://jaliyaudagedara.blogspot.com/2024/02/net-isolated-azure-durable-functions.html)

Reproducing the issue,
```csharp
var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(services =>
	{
		services.Configure<JsonSerializerOptions>(options =>
		{
			// TODO: To reproduce the issue, comment out the following
			options.Converters.Add(new JsonConverterFactoryForStackOfT());
		});
	})
	.Build();

host.Run();
```
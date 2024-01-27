using DurableFunctions.Isolated.StackSerialization.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

IHost host = new HostBuilder()
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
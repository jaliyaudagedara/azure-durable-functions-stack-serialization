using DurableFunctions.InProcess.StackSerialization;
using DurableFunctions.InProcess.StackSerialization.JsonConverters;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DurableFunctions.InProcess.StackSerialization;

public class Startup : FunctionsStartup
{
	public override void Configure(IFunctionsHostBuilder builder)
	{
		// TODO: To reproduce the issue comment this line out
		builder.Services.AddSingleton<IMessageSerializerSettingsFactory, CustomMessageSerializerSettingsFactory>();
	}

	/// <summary>
	/// A factory that provides the serialization for all inputs and outputs for activities and
	/// orchestrations, as well as entity state.
	/// </summary>
	internal class CustomMessageSerializerSettingsFactory : IMessageSerializerSettingsFactory
	{
		public JsonSerializerSettings CreateJsonSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.None,
				DateParseHandling = DateParseHandling.None,
				Converters = new List<JsonConverter>
				{
					new StackConverter()
				}
			};
		}
	}
}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace DurableFunctions.Isolated.StackSerialization;

public class Function1
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public Function1(IOptions<JsonSerializerOptions> jsonSerializerOptions)
	{
		_jsonSerializerOptions = jsonSerializerOptions.Value;
	}

	[Function("HttpStart")]
	public async Task<HttpResponseData> HttpStart(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
		[DurableClient] DurableTaskClient client)
	{
		var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(Function1));

		OrchestrationMetadata orchestrationMetadata = await client.WaitForInstanceCompletionAsync(instanceId, true);

		Stack<string> transitions = JsonSerializer.Deserialize<Stack<string>>(orchestrationMetadata.SerializedOutput!, _jsonSerializerOptions)!;

		HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
		await response.WriteAsJsonAsync(transitions);
		return response;
	}

	[Function("Function1")]
	public async Task<Stack<string>> RunOrchestrator1(
		[OrchestrationTrigger] TaskOrchestrationContext context)
	{
		Stack<string> transitions = new();

		transitions.Push("T0");
		transitions.Push("T1");
		transitions.Push("T2");

		return await context.CallSubOrchestratorAsync<Stack<string>>("Function2", transitions);
	}

	[Function("Function2")]
	public async Task<Stack<string>> RunOrchestrator2(
		[OrchestrationTrigger] TaskOrchestrationContext context)
	{
		Stack<string> transitions = context.GetInput<Stack<string>>()!;

		transitions.Push("T3");
		transitions.Push("T4");
		transitions.Push("T5");

		return await Task.FromResult(transitions);
	}
}
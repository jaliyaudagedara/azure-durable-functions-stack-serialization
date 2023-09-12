using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableFunctions.StackSerialization;

public static class Function1
{
    [FunctionName("HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        string instanceId = await starter.StartNewAsync("Function1");

        log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, TimeSpan.FromSeconds(30));
    }

    [FunctionName("Function1")]
    public static async Task<Stack<string>> RunOrchestrator1([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        Stack<string> transitions = new();

        transitions.Push("T0");
        transitions.Push("T1");
        transitions.Push("T2");

        return await context.CallSubOrchestratorAsync<Stack<string>>("Function2", transitions);
    }

    [FunctionName("Function2")]
    public static async Task<Stack<string>> RunOrchestrator2([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        Stack<string> transitions = context.GetInput<Stack<string>>();

        transitions.Push("T3");
        transitions.Push("T4");
        transitions.Push("T5");

        return await Task.FromResult(transitions);
    }
}
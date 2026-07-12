using System.Text.Json;
using System.Text.Json.Nodes;
using Rrp.Core;

var server = new McpServer(Console.In, Console.Out);
await server.RunAsync(CancellationToken.None);

internal sealed class McpServer(TextReader input, TextWriter output)
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private readonly RuntimeCapabilities _capabilities = new("rrp/1.0", "0.1.0-alpha", new HashSet<string>
    { "workflow.validate", "replay.plan", "desktop.session-probe" });

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await input.ReadLineAsync(cancellationToken);
            if (line is null) return;
            JsonNode? request;
            try { request = JsonNode.Parse(line); }
            catch (JsonException) { continue; }
            if (request is null || request["id"] is null) continue;
            var response = Dispatch(request);
            await output.WriteLineAsync(response.ToJsonString(_json));
            await output.FlushAsync(cancellationToken);
        }
    }

    private JsonObject Dispatch(JsonNode request)
    {
        var id = request["id"]!.DeepClone();
        try
        {
            var method = request["method"]?.GetValue<string>();
            object result = method switch
            {
                "initialize" => new { protocolVersion = "2025-06-18", capabilities = new { tools = new { } }, serverInfo = new { name = "open-record-replay", version = "0.1.0-alpha" } },
                "tools/list" => new { tools = ToolDefinitions() },
                "tools/call" => CallTool(request["params"] as JsonObject),
                "ping" => new { },
                _ => throw new InvalidOperationException($"Unknown method '{method}'.")
            };
            return new JsonObject { ["jsonrpc"] = "2.0", ["id"] = id, ["result"] = JsonSerializer.SerializeToNode(result, _json) };
        }
        catch (Exception ex)
        {
            return new JsonObject { ["jsonrpc"] = "2.0", ["id"] = id, ["error"] = new JsonObject { ["code"] = -32603, ["message"] = ex.Message } };
        }
    }

    private object CallTool(JsonObject? parameters)
    {
        var name = parameters?["name"]?.GetValue<string>() ?? throw new InvalidOperationException("Tool name is required.");
        var args = parameters?["arguments"];
        object payload = name switch
        {
            "rrp_capabilities_get" => _capabilities,
            "rrp_workflow_validate" => Validate(ParseWorkflow(args)),
            "rrp_replay_plan" => Plan(ParseWorkflow(args)),
            _ => throw new InvalidOperationException($"Unknown tool '{name}'.")
        };
        return new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(payload, _json) } }, isError = false };
    }

    private RrpWorkflow ParseWorkflow(JsonNode? args)
    {
        var node = args?["workflow"] ?? throw new InvalidOperationException("arguments.workflow is required.");
        return node.Deserialize<RrpWorkflow>(_json) ?? throw new InvalidOperationException("Workflow is invalid.");
    }
    private ValidationResult Validate(RrpWorkflow workflow) => new WorkflowValidator().Validate(workflow);
    private ReplayPlan Plan(RrpWorkflow workflow) => new ReplayPlanner(new WorkflowValidator()).Plan(workflow, _capabilities);

    private static object[] ToolDefinitions() =>
    [
        Tool("rrp_capabilities_get", "Return RRP runtime capabilities.", new { type = "object", properties = new { } }),
        Tool("rrp_workflow_validate", "Validate a portable RRP workflow.", WorkflowInput()),
        Tool("rrp_replay_plan", "Create a deterministic replay plan without executing actions.", WorkflowInput())
    ];
    private static object Tool(string name, string description, object schema) => new { name, description, inputSchema = schema };
    private static object WorkflowInput() => new { type = "object", properties = new { workflow = new { type = "object" } }, required = new[] { "workflow" } };
}

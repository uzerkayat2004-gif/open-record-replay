using System.Text.Json;
using System.Text.Json.Nodes;
using Rrp.Core;
using Rrp.Windows;

var home = Environment.GetEnvironmentVariable("RRP_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenRecordReplay");
Directory.CreateDirectory(home);
var store = new JsonFileRrpStore(home);
await using var recording = new RecordingService(store);
var replay = new ReplayService(store);
var server = new RrpMcpServer(Console.In, Console.Out, store, recording, replay);
await server.RunAsync(CancellationToken.None);

internal sealed class RrpMcpServer(TextReader input, TextWriter output, IRrpStore store, RecordingService recording, ReplayService replay)
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private readonly RuntimeCapabilities _capabilities = new("rrp/1.0", "0.5.0-beta", new HashSet<string> { "record.desktop", "workflow.compile", "workflow.validate", "replay.desktop", "replay.cancel", "storage.local" });

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var line = await input.ReadLineAsync(ct); if (line is null) return;
            JsonNode? request; try { request = JsonNode.Parse(line); } catch { continue; }
            if (request is null || request["id"] is null) continue;
            var response = await DispatchAsync(request, ct);
            await output.WriteLineAsync(response.ToJsonString(_json)); await output.FlushAsync(ct);
        }
    }

    private async Task<JsonObject> DispatchAsync(JsonNode request, CancellationToken ct)
    {
        var id=request["id"]!.DeepClone();
        try
        {
            var method=request["method"]?.GetValue<string>();
            object result=method switch
            {
                "initialize" => new { protocolVersion="2025-06-18", capabilities=new { tools=new{}, resources=new{} }, serverInfo=new { name="open-record-replay", version="0.5.0-beta" } },
                "notifications/initialized" => new { },
                "ping" => new { },
                "tools/list" => new { tools=Tools() },
                "tools/call" => await CallAsync(request["params"] as JsonObject,ct),
                "resources/list" => new { resources=Array.Empty<object>() },
                _ => throw new McpError(-32601,$"Unknown method '{method}'.")
            };
            return new(){["jsonrpc"]="2.0",["id"]=id,["result"]=JsonSerializer.SerializeToNode(result,_json)};
        }
        catch(Exception ex)
        {
            var code=ex is McpError me?me.Code:-32603;
            return new(){["jsonrpc"]="2.0",["id"]=id,["error"]=new JsonObject{{"code",code},{"message",ex.Message}}};
        }
    }

    private async Task<object> CallAsync(JsonObject? p,CancellationToken ct)
    {
        var name=p?["name"]?.GetValue<string>()??throw new McpError(-32602,"Tool name is required.");
        var a=p?["arguments"] as JsonObject??new JsonObject();
        object value=name switch
        {
            "rrp_capabilities_get" => _capabilities,
            "rrp_recording_start" => await recording.StartAsync(String(a,"name","Untitled recording"),ct),
            "rrp_recording_status" => recording.Current is { } current ? current : (object)new { state="idle" },
            "rrp_recording_stop" => await recording.StopAsync(Bool(a,"compileWorkflow",true),ct),
            "rrp_recording_list" => await store.ListRecordingsAsync(ct),
            "rrp_workflow_list" => await store.ListWorkflowsAsync(ct),
            "rrp_workflow_get" => await store.GetWorkflowAsync(Required(a,"workflowId"),ct)??throw new KeyNotFoundException("Workflow not found."),
            "rrp_workflow_validate" => new WorkflowValidator().Validate(Workflow(a)),
            "rrp_workflow_compile" => await CompileAsync(a,ct),
            "rrp_replay_plan" => await PlanAsync(a,ct),
            "rrp_replay_run" => await replay.StartAsync(Required(a,"workflowId"),ct),
            "rrp_replay_status" => replay.Get(Required(a,"replayId"))??throw new KeyNotFoundException("Replay not found."),
            "rrp_replay_cancel" => new { cancelled=replay.Cancel(Required(a,"replayId")) },
            "rrp_diagnostics" => Diagnostics(),
            _ => throw new McpError(-32602,$"Unknown tool '{name}'.")
        };
        return new { content=new[]{new{type="text",text=JsonSerializer.Serialize(value,_json)}}, structuredContent=value, isError=false };
    }

    private async Task<RrpWorkflow> CompileAsync(JsonObject a,CancellationToken ct)
    { var recordingId=Required(a,"recordingId");var d=await store.GetRecordingAsync(recordingId,ct)??throw new KeyNotFoundException("Recording not found.");var w=new WorkflowCompiler().Compile(d);await store.SaveWorkflowAsync(w,ct);return w; }
    private async Task<ReplayPlan> PlanAsync(JsonObject a,CancellationToken ct)
    { RrpWorkflow w=a["workflow"] is null?await store.GetWorkflowAsync(Required(a,"workflowId"),ct)??throw new KeyNotFoundException("Workflow not found."):Workflow(a);return new ReplayPlanner(new WorkflowValidator()).Plan(w,_capabilities); }
    private object Diagnostics()=>new{healthy=OperatingSystem.IsWindows(),platform=Environment.OSVersion.ToString(),home=Environment.GetEnvironmentVariable("RRP_HOME")??"default",capabilities=_capabilities.Features};
    private RrpWorkflow Workflow(JsonObject a)=>a["workflow"]?.Deserialize<RrpWorkflow>(_json)??throw new McpError(-32602,"workflow is required.");
    private static string Required(JsonObject a,string n)=>a[n]?.GetValue<string>()??throw new McpError(-32602,$"{n} is required.");
    private static string String(JsonObject a,string n,string d)=>a[n]?.GetValue<string>()??d;
    private static bool Bool(JsonObject a,string n,bool d)=>a[n]?.GetValue<bool>()??d;

    private static object[] Tools()=>
    [
        Tool("rrp_capabilities_get","Return runtime capabilities.",false),
        Tool("rrp_recording_start","Begin a visible local Windows recording.",false,("name","string",false)),
        Tool("rrp_recording_status","Return the active recording status.",false),
        Tool("rrp_recording_stop","Stop recording and optionally compile a workflow.",false,("compileWorkflow","boolean",false)),
        Tool("rrp_recording_list","List saved recordings.",false),
        Tool("rrp_workflow_list","List saved workflows.",false),
        Tool("rrp_workflow_get","Get a saved workflow.",false,("workflowId","string",true)),
        Tool("rrp_workflow_validate","Validate a workflow.",false,("workflow","object",true)),
        Tool("rrp_workflow_compile","Compile a completed recording.",false,("recordingId","string",true)),
        Tool("rrp_replay_plan","Plan without executing. Supply workflowId or workflow.",false,("workflowId","string",false),("workflow","object",false)),
        Tool("rrp_replay_run","Execute a saved workflow.",false,("workflowId","string",true)),
        Tool("rrp_replay_status","Get replay progress.",false,("replayId","string",true)),
        Tool("rrp_replay_cancel","Cancel a replay.",false,("replayId","string",true)),
        Tool("rrp_diagnostics","Run local runtime diagnostics.",false)
    ];

    private static object Tool(string name,string description,bool destructive,params (string Name,string Type,bool Required)[] fields)
    { var props=fields.ToDictionary(x=>x.Name,x=>(object)new{type=x.Type});var required=fields.Where(x=>x.Required).Select(x=>x.Name).ToArray();return new{name,description,inputSchema=new{type="object",properties=props,required},annotations=new{readOnlyHint=name.Contains("get")||name.Contains("list")||name.Contains("status")||name.Contains("plan")||name.Contains("diagnostics"),destructiveHint=destructive}}; }
}

internal sealed class McpError(int code,string message):Exception(message){public int Code{get;}=code;}

using System.Text.Json;

namespace Rrp.Core;

public interface IRrpStore
{
    Task SaveRecordingAsync(RecordingDocument document, CancellationToken ct);
    Task<RecordingDocument?> GetRecordingAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<RecordingSummary>> ListRecordingsAsync(CancellationToken ct);
    Task SaveWorkflowAsync(RrpWorkflow workflow, CancellationToken ct);
    Task<RrpWorkflow?> GetWorkflowAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<WorkflowMetadata>> ListWorkflowsAsync(CancellationToken ct);
}

public sealed class JsonFileRrpStore : IRrpStore
{
    private readonly string _root;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonFileRrpStore(string root)
    {
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(Path.Combine(_root, "recordings"));
        Directory.CreateDirectory(Path.Combine(_root, "workflows"));
    }

    public Task SaveRecordingAsync(RecordingDocument d, CancellationToken ct) => SaveAsync(Path.Combine(_root, "recordings", Safe(d.RecordingId)+".json"), d, ct);
    public Task<RecordingDocument?> GetRecordingAsync(string id, CancellationToken ct) => LoadAsync<RecordingDocument>(Path.Combine(_root, "recordings", Safe(id)+".json"), ct);
    public Task SaveWorkflowAsync(RrpWorkflow w, CancellationToken ct) => SaveAsync(Path.Combine(_root, "workflows", Safe(w.Metadata.Id)+".json"), w, ct);
    public Task<RrpWorkflow?> GetWorkflowAsync(string id, CancellationToken ct) => LoadAsync<RrpWorkflow>(Path.Combine(_root, "workflows", Safe(id)+".json"), ct);

    public async Task<IReadOnlyList<RecordingSummary>> ListRecordingsAsync(CancellationToken ct)
    {
        var list = new List<RecordingSummary>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(_root,"recordings"), "*.json"))
        { var d=await LoadAsync<RecordingDocument>(file,ct); if(d is not null) list.Add(new(d.RecordingId,d.Name,d.State,d.CreatedAt,d.Actions.Count)); }
        return list.OrderByDescending(x=>x.CreatedAt).ToArray();
    }

    public async Task<IReadOnlyList<WorkflowMetadata>> ListWorkflowsAsync(CancellationToken ct)
    {
        var list=new List<WorkflowMetadata>();
        foreach(var file in Directory.EnumerateFiles(Path.Combine(_root,"workflows"),"*.json"))
        { var w=await LoadAsync<RrpWorkflow>(file,ct); if(w is not null) list.Add(w.Metadata); }
        return list.OrderBy(x=>x.Name).ToArray();
    }

    private async Task SaveAsync<T>(string path,T value,CancellationToken ct)
    {
        await _gate.WaitAsync(ct); try { var tmp=path+".tmp"; await File.WriteAllTextAsync(tmp,JsonSerializer.Serialize(value,_json),ct); File.Move(tmp,path,true); } finally { _gate.Release(); }
    }
    private async Task<T?> LoadAsync<T>(string path,CancellationToken ct)
    { if(!File.Exists(path)) return default; await _gate.WaitAsync(ct); try { return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path,ct),_json); } finally { _gate.Release(); } }
    private static string Safe(string id) => id.Length is >0 and <=128 && id.All(c=>char.IsLetterOrDigit(c)||c is '-' or '_') ? id : throw new ArgumentException("Invalid identifier.");
}

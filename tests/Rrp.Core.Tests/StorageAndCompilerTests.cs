using Rrp.Core;
using Xunit;

namespace Rrp.Core.Tests;

public sealed class StorageAndCompilerTests
{
    [Fact]
    public async Task File_store_round_trips_recording_and_workflow()
    {
        var root = Path.Combine(Path.GetTempPath(), "rrp-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonFileRrpStore(root);
            var recording = new RecordingDocument("1.0", "recording1", "Demo", DateTimeOffset.Now, DateTimeOffset.Now,
                RecordingState.Completed, [new("step-1", "desktop.click", 10, Element(), new Dictionary<string, object?>())]);
            await store.SaveRecordingAsync(recording, CancellationToken.None);
            var loaded = await store.GetRecordingAsync("recording1", CancellationToken.None);
            Assert.NotNull(loaded);
            Assert.Single(loaded.Actions);

            var workflow = new WorkflowCompiler().Compile(recording);
            await store.SaveWorkflowAsync(workflow, CancellationToken.None);
            var loadedWorkflow = await store.GetWorkflowAsync("recording1", CancellationToken.None);
            Assert.NotNull(loadedWorkflow);
            Assert.Single(loadedWorkflow.Steps);
            Assert.NotEmpty(loadedWorkflow.Steps[0].Target!.Candidates);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Compiler_refuses_incomplete_recording()
    {
        var recording = new RecordingDocument("1.0", "x", "x", DateTimeOffset.Now, null, RecordingState.Recording, []);
        Assert.Throws<InvalidOperationException>(() => new WorkflowCompiler().Compile(recording));
    }

    private static ElementSnapshot Element() => new(100, "notepad", "Untitled - Notepad", "15", "Save", "Button", "Button", "Win32",
        new(10, 10, 80, 30), true, false, false, []);
}

using Rrp.Core;
namespace Rrp.Core.Tests;
public sealed class WorkflowValidatorTests{
 [Fact] public void Valid_read_workflow_passes(){var w=Make(new("inspect","desktop.inspect",null,null,null,[new("window.visible")],null));Assert.True(new WorkflowValidator().Validate(w).Valid);}
 [Fact] public void Mutation_without_postcondition_fails(){var result=new WorkflowValidator().Validate(Make(new("click","desktop.click",null,null,null,null)));Assert.Contains(result.Issues,x=>x.Code=="missing_postcondition");}
 [Fact] public void Missing_capability_blocks_plan(){var w=Make(new("inspect","desktop.inspect",null,null,null,[new("window.visible")],null),["replay.desktop"]);var plan=new ReplayPlanner(new WorkflowValidator()).Plan(w,new("rrp/1.0","test",new HashSet<string>()));Assert.False(plan.Runnable);Assert.Contains(plan.Issues,x=>x.Code=="capability_mismatch");}
 static RrpWorkflow Make(WorkflowStep s,IReadOnlyList<string>? requirements=null)=>new("rrp.dev/v1","Workflow",new("test","Test",DateTimeOffset.UtcNow),new("rrp/1.0",requirements??[]),new Dictionary<string,WorkflowParameter>(),[s]);
}

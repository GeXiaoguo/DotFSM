namespace IssueTracker;

using DotFSM;

public enum ProjectTrigger { Plan, Start, Close, Park, ReOpen }
public enum ProjectState { Created = 0, Parked, InPlanning, InExecution, Closed }
public enum IssueState { Null = 0, Created, Assigned, Resolved, Terminated }
public enum IssueTrigger { Create = 0, Assign, Resolve, Terminate }
public record Issue()
{
    public Guid ID { get; } = Guid.NewGuid();
    public IssueState CurrentWorkflowState { get; init; } = IssueState.Created;
    public static readonly DotFSM<(ProjectState, IssueState), IssueTrigger> WorkflowDefinition = WorkflowDefinitions.ProjectIssueWorkflow.Combine(WorkflowDefinitions.IssueWorkflow);
}
public record Project(DotFSM<ProjectState, ProjectTrigger> WorkflowDefinition, ProjectState CurrentWorkflowState = ProjectState.Created)
{
    public Guid ID { get; } = Guid.NewGuid();
    public Issue? Issue {get; init;}
}
public static class WorkflowDefinitions
{
    public static readonly DotFSM<IssueState, IssueTrigger> IssueWorkflow = new DotFSM<IssueState, IssueTrigger>(new Transition<IssueState, IssueTrigger>[]
    {
            new (SourceState: IssueState.Null       ,Trigger: IssueTrigger.Create    , DestinationState: IssueState.Created     ),
            new (SourceState: IssueState.Created    ,Trigger: IssueTrigger.Assign    , DestinationState: IssueState.Assigned    ),
            new (SourceState: IssueState.Created    ,Trigger: IssueTrigger.Terminate , DestinationState: IssueState.Terminated  ),
            new (SourceState: IssueState.Assigned   ,Trigger: IssueTrigger.Resolve   , DestinationState: IssueState.Resolved    ),
            new (SourceState: IssueState.Assigned   ,Trigger: IssueTrigger.Terminate , DestinationState: IssueState.Terminated  ),
            new (SourceState: IssueState.Terminated ,Trigger: IssueTrigger.Assign    , DestinationState: IssueState.Assigned    ),
    });
    public static DotFSM<ProjectState, ProjectTrigger> ProjectWorkflow
        => Builder<ProjectState, ProjectTrigger>
          .Start(ProjectState.Created)
              .Allow(ProjectTrigger.Plan, ProjectState.InPlanning)
          .ForState(ProjectState.InPlanning)
              .Allow(ProjectTrigger.Start, ProjectState.InExecution)
              .Allow(ProjectTrigger.Park, ProjectState.Parked)
          .ForState(ProjectState.Parked)
              .Allow(ProjectTrigger.ReOpen, ProjectState.InPlanning)
          .ForState(ProjectState.InExecution)
              .Allow(ProjectTrigger.Close, ProjectState.Closed)
          .ForState(ProjectState.Closed)
              .Allow(ProjectTrigger.ReOpen, ProjectState.InPlanning)
          .Build();
    public static DotFSM<ProjectState, IssueTrigger> ProjectIssueWorkflow
        => Builder<ProjectState, IssueTrigger>
          .Start(ProjectState.InPlanning)
              .Allow(IssueTrigger.Create, ProjectState.InPlanning)
              .Allow(IssueTrigger.Terminate, ProjectState.InPlanning)
          .ForState(ProjectState.InExecution)
              .Allow(IssueTrigger.Assign, ProjectState.InExecution)
              .Allow(IssueTrigger.Terminate, ProjectState.InExecution)
              .Allow(IssueTrigger.Resolve, ProjectState.InExecution)
          .Build();
}

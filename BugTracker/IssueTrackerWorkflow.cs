namespace IssueTracker;

using DotFSM;

public enum State { Null = 0, Created, Assigned, Resolved, Terminated }
public enum Trigger { Create = 0, Assign, Resolve, Terminate }
public static class EnumExtensions
{
    public static T? ToEnum<T>(this string enumName) where T : struct => Enum.TryParse<T>(enumName, out T result) ? result : null;
}
public static class WorkflowDefinitions
{
    public static readonly DotFSM<State, Trigger> ComplexWorkflow = new DotFSM<State, Trigger>(new Transition<State, Trigger>[]
    {
            new () {SourceState = State.Null       ,Trigger = Trigger.Create    , DestinationState = State.Created     },
            new () {SourceState = State.Created    ,Trigger = Trigger.Assign    , DestinationState = State.Assigned    },
            new () {SourceState = State.Created    ,Trigger = Trigger.Terminate , DestinationState = State.Terminated  },
            new () {SourceState = State.Assigned   ,Trigger = Trigger.Resolve   , DestinationState = State.Resolved    },
            new () {SourceState = State.Assigned   ,Trigger = Trigger.Terminate , DestinationState = State.Terminated  },
            new () {SourceState = State.Terminated ,Trigger = Trigger.Assign    , DestinationState = State.Assigned    },
    });
    public static DotFSM<State, Trigger> SimpleWorkflow() =>
        Builder<State, Trigger>
          .Start(State.Created)
              .Allow(Trigger.Assign, State.Assigned)
              .Allow(Trigger.Terminate, State.Terminated)
          .ForState(State.Assigned)
              .Allow(Trigger.Resolve, State.Resolved)
              .Allow(Trigger.Terminate, State.Terminated)
          .Build();
}
public record Issue
{
    public Guid ID { get; } = Guid.NewGuid();
    public DotFSM<State, Trigger> Workflow { get; init; } = WorkflowDefinitions.ComplexWorkflow;
    public string? Title { get; init; }
    public State CurrentWorkflowState { get; init; } = State.Created;
}
public static class IssueWorkflowService
{
    public static Issue Assign(Issue issue) => FireTrigger(issue, Trigger.Assign);
    public static Issue Resolve(Issue issue) => FireTrigger(issue, Trigger.Resolve);
    public static Issue Terminate(Issue issue) => FireTrigger(issue, Trigger.Terminate);
    public static Issue FireTrigger(Issue issue, Trigger trigger)
    {
        var transition = issue.Workflow.GetTransition(issue.CurrentWorkflowState, trigger);
        if (transition == null)
        {
            throw new IssueWorkflowException($"{trigger} is not allowed for {issue.CurrentWorkflowState}");
        }
        return issue with { CurrentWorkflowState = transition.DestinationState };
    }
}

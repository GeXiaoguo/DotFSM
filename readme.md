# A light weight Finite State Machine in C#
## `DotFSM` is a light weight C# library that helps with implementing workflows

Workflow is a common business logic problem in software development. For example the diagram below is a workflow for an issue tracking application.
```mermaid
stateDiagram-v2
direction LR
Null --> Created : Create
Created --> Assigned : Assign
Created --> Terminated : Terminate
Assigned --> Resolved : Resolve
Assigned --> Terminated : Terminate
Terminated --> Assigned : Assign
```

Workflows are essentially graphs with each edge representing one allowed state transition. This is apparent when we look at the mermaid code for the diagram above. It is simply a collection of the state transitions specified as `source state` --> `destination state` : `trigger`. 
```
Null --> Created : Create
Created --> Assigned : Assign
Created --> Terminated : Terminate
Assigned --> Resolved : Resolve
Assigned --> Terminated : Terminate
Terminated --> Assigned : Assign
```

`DotFSM` allows you to specify a `Finite State Machine` as a collection of state transitions. For example the above state machine can be specified as this
```
new DotFSM<State, Trigger>(new Transition<State, Trigger>[]
{
    new () {SourceState = State.Null       ,Trigger = Trigger.Create    , DestinationState = State.Created     },
    new () {SourceState = State.Created    ,Trigger = Trigger.Assign    , DestinationState = State.Assigned    },
    new () {SourceState = State.Created    ,Trigger = Trigger.Terminate , DestinationState = State.Terminated  },
    new () {SourceState = State.Assigned   ,Trigger = Trigger.Resolve   , DestinationState = State.Resolved    },
    new () {SourceState = State.Assigned   ,Trigger = Trigger.Terminate , DestinationState = State.Terminated  },
    new () {SourceState = State.Terminated ,Trigger = Trigger.Assign    , DestinationState = State.Assigned    },
})
```

`DotFSM` also comes with a builder which allows specifying the above state machine as below, which subjectively is easier to read.
```
Builder<State, Trigger>
    .Start(State.Created)
        .Allow(Trigger.Assign, State.Assigned)
        .Allow(Trigger.Terminate, State.Terminated)
    .ForState(State.Assigned)
        .Allow(Trigger.Resolve, State.Resolved)
        .Allow(Trigger.Terminate, State.Terminated)
    .ForState(State.Terminated)
        .Allow(Trigger.Assign, State.Assigned)
    .Build();
```

Once a state machine is defined, excercising it is as simle as firing triggers into the state machine and getting either a `DotFSM.Transition` back if the trigger is allowed or `null` if it is not allowed.

Then an issue tracking service can be as simple as
```
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
            throw new IssueWorkflowException($"{trigger} is not allowed for {issue}");
        }
        return issue with { CurrentWorkflowState = transition.DestinationState };
    }
}
```

A complete console application that moves issues between states according to the workflow diagram above can be as simple as below
```
using IssueTracker;

var issue = new Issue()
{
    Workflow = WorkflowDefinitions.ComplexWorkflow
};

while (true) 
{
    Console.WriteLine(" --- Current workflow definition ---");
    Console.WriteLine(issue.Workflow.ToMermaidDiagram());
    Console.WriteLine("------------------------------------");
    Console.WriteLine($"Current state: {issue.CurrentWorkflowState}");
    Console.WriteLine($"Commands allowed:  Exit, {string.Join(",", issue.Workflow.AllowedTriggers(issue.CurrentWorkflowState))}");
    var line = Console.ReadLine();
    if ("exit".Equals(line, StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var trigger = line.ToEnum<Trigger>();
    if (trigger == null) 
    {
        Console.WriteLine($"unknown command {line}");
        continue;
    }

    try
    {
        var updatedIssue = IssueWorkflowService.FireTrigger(issue, trigger.Value);
        issue = updatedIssue;
    }
    catch (IssueWorkflowException e)
    {
        Console.WriteLine(e.Message);
    }
}
```

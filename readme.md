# A lightweight Finite State Machine in C#

**Workflow is a common business logic problem** in software development. For example, the diagram below is a workflow for an issue tracking application.
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

**Workflows are essentially graphs** with each permitted state transition represented by an edge. This is apparent when we look at the mermaid code for the diagram above. It is simply a flat collection of the state transitions specified as `src-state` --> `dst-state` : `trigger`. 
```
Null --> Created : Create
Created --> Assigned : Assign
Created --> Terminated : Terminate
Assigned --> Resolved : Resolve
Assigned --> Terminated : Terminate
Terminated --> Assigned : Assign
```

**DotFSM allows defining `Finite State Machines` as a collection of state transitions.** For example, the above state machine can be defined like below
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
**DotFSM also comes with a builder** which allows specifying the above state machine as below, which is subjectively easier to read.
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

**Exercising the state machine is simply a search of the matching edge in the graph**. If a transition is permitted, the search function returns a `DotFSM.Transition`, or otherwise a `null` value.
```
    var transition = EvaluateWorkflow(workflowDefinition, issue.CurrentWorkflowState, trigger);
    if (transition == null)
    {
        throw new IssueWorkflowException($"{trigger} is not allowed for {issue}");
    }
    return issue with { CurrentWorkflowState = transition.DestinationState };
```

## Workflow composition

**One benefit of representing workflow as data is that multiple workflows can be combined into one**. This allows complex workflows to be broken into smaller and simpler pieces and implemented independently. For example, suppose the issue tracking application also need to deal with projects. And the project has the following statuses: `InPlanning`, `InExecution`, `Closed`, `Parked`. The business rules are listed below: 
 - `InPlanning` projects allow `Create` and `Terminate` issues.
 - `InExecution` projects allow `Assign`, `Terminate`, and `Resolve` issues
 - `Parked` and `Closed` projects do not allow any action to be done on issues. 

The rules above can be illusated as a simple workflow below. Let's name it the `ProjectIssueWorkflow`.
```mermaid
stateDiagram-v2
direction LR
InPlanning --> InPlanning : IssueTrigger.Create
InPlanning --> InPlanning : IssueTrigger.Terminate
InExecution --> InExecution : IssueTrigger.Assign
InExecution --> InExecution : IssueTrigger.Terminate
InExecution --> InExecution : IssueTrigger.Resolve
```
Just as a refresh, the `IssueWorkflow` is defined as 
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
The two workflows can be combined and exercised at run time like below.
```
  var workflow = ProjectIssueWorkflow.Combine(IssueWorkflow);
```
Visualizing the combined workflow makes it apparent how complex the real business rules are and how simple it is when the rules are broken into two smaller, independent, and easy to understand workflows `IssueWorkflow` and `ProjectIssueWorkflow`.
```mermaid
stateDiagram-v2
InPlanning*Null --> InPlanning*Created : Create
InPlanning*Created --> InPlanning*Terminated : Terminate
InPlanning*Assigned --> InPlanning*Terminated : Terminate
InExecution*Created --> InExecution*Assigned : Assign
InExecution*Terminated --> InExecution*Assigned : Assign
InExecution*Created --> InExecution*Terminated : Terminate
InExecution*Assigned --> InExecution*Terminated : Terminate
InExecution*Assigned --> InExecution*Resolved : Resolve

```

The new workflow can then be exercised like any other `DotFSM` instances, with `currentState` being of type `(ProejctState, IssueState)`.
```
var transition = workflow.GetTransition(currentState, trigger)
```

**A complete console application** that moves both the project and issue between states according to the workflow definitions above can be as simple as the code below
```
var (context, command) = ParseCommand(line);
switch (context)
{
    case "project":
        {
            var trigger = command.ToEnum<ProjectTrigger>() ?? throw new CommandLineParsingException($"unknown command {command}");
            var updatedProject = ProjectWorkflowService.FireTrigger(_project, trigger);
            _project = updatedProject;
            break;
        }
    case "issue":
        {
            var trigger = command.ToEnum<IssueTrigger>() ?? throw new CommandLineParsingException($"unknown command {command}");
            var updatedProject = IssueWorkflowService.FireTrigger(_project, trigger);
            _project = updatedProject;
            break;
        }
    default:
        {
            throw new CommandLineParsingException($"Invalid context {context}. Only 'Proejct' and 'Issue' are valid");
        }
} 
```

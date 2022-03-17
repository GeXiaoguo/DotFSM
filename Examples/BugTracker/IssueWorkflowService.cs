namespace IssueTracker;

using DotFSM;

public static class IssueWorkflowService
{
    public static Project FireTrigger(Project project, IssueTrigger trigger)
    {
        var currentState = (project.CurrentWorkflowState, project.Issue?.CurrentWorkflowState ?? IssueState.Null);
        var transition = Issue.WorkflowDefinition.GetTransition(currentState, trigger);
        if (transition == null)
        {
            throw new IssueWorkflowException($"{trigger} is not allowed for {currentState}");
        }
        var issue = project.Issue ?? new Issue();
        return project with
        {
            CurrentWorkflowState = transition.DestinationState.Item1,
            Issue = issue with { CurrentWorkflowState = transition.DestinationState.Item2}
        };
    }
}
public static class ProjectWorkflowService
{
    public static Project FireTrigger(Project project, ProjectTrigger trigger)
    {
        var transition = project.WorkflowDefinition.GetTransition(project.CurrentWorkflowState, trigger);
        if (transition == null)
        {
            throw new IssueWorkflowException($"{trigger} is not allowed for {project.CurrentWorkflowState}");
        }
        return project with { CurrentWorkflowState = transition.DestinationState };
    }
}

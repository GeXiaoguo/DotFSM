// See https://aka.ms/new-console-template for more information
using DotFSM;
using IssueTracker;

var _project = new Project(WorkflowDefinition: WorkflowDefinitions.ProjectWorkflow);

(ProjectState, IssueState) CurrentState(Project project) => (project.CurrentWorkflowState, project.Issue?.CurrentWorkflowState?? IssueState.Null);
(string context, string command) ParseCommand(string? cmdLine)
{
    var array = cmdLine?.Split(" ").ToArray();
    if (array?.Length != 2)
    {
        throw new CommandLineParsingException($"Invalid command: {cmdLine}");
    }
    return (array[1], array[0]);
}
while (true) 
{
    var currentState  = CurrentState(_project);

    var allowedIssueCommands = Issue.WorkflowDefinition
        .AllowedTriggers(currentState)
        .Select(x => $"{x} Issue");

    var allowedProjectCommands = _project
        .WorkflowDefinition
        .AllowedTriggers(_project.CurrentWorkflowState)
        .Select(x => $"{x} Project");

    Console.WriteLine($"-----------------------------------------------------------------");
    Console.WriteLine($"Current state: Project: {currentState.Item1}, Issue: {currentState.Item2}");
    Console.WriteLine($"Issue Commands allowed: \r\n{string.Join("\r\n", allowedIssueCommands)}");
    Console.WriteLine($"Project Commands allowed: \r\n{string.Join("\r\n", allowedProjectCommands)}");

    var line = Console.ReadLine()?.ToLowerInvariant();
    if ("exit".Equals(line))
    {
        return;
    }

    try
    {
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
    }
    catch (Exception e) when (e is IssueWorkflowException or CommandLineParsingException)
    {
        Console.WriteLine(e.Message);
    }
}
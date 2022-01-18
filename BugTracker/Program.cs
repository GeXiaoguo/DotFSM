// See https://aka.ms/new-console-template for more information
using IssueTracker;

var issue = new Issue()
{
    Workflow = WorkflowDefinitions.ComplexWorkflow
};

Console.WriteLine(" --- Current workflow definition ---");
Console.WriteLine(issue.Workflow.ToMermaidDiagram());
Console.WriteLine("------------------------------------");

while (true) 
{
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
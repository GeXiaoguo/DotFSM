namespace DotFSM;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
public class DotFSM<StateT, TriggerT>
    where StateT : IComparable
    where TriggerT : IComparable
{
    public IEnumerable<Transition<StateT, TriggerT>> Transitions { get; } = new List<Transition<StateT, TriggerT>>();
    public DotFSM(IEnumerable<Transition<StateT, TriggerT>> allowedTransitions)
    {
        var ambiguousTransitions = allowedTransitions
            .GroupBy(x => (x.SourceState, x.Trigger))
            .Where(x => x.Count() > 1)
            .ToList();

        if (ambiguousTransitions.Count > 1)
        {
            throw new FSMDefinitionException($"The following transitions are ambiguous {string.Join("\r\n", ambiguousTransitions)}");
        }
        Transitions = allowedTransitions.ToList();
    }
    public IEnumerable<TriggerT> AllowedTriggers(StateT givenState)
        => Transitions
           .Where(x => x.SourceState.Equals(givenState))
           .Select(x => x.Trigger);
    public Transition<StateT, TriggerT>? GetTransition(StateT currentState, TriggerT trigger) =>
        Transitions
        .SingleOrDefault(x => x.SourceState.CompareTo(currentState) == 0 && x.Trigger.CompareTo(trigger) == 0);
    public string ToMermaidDiagram() => string.Join("\r\n", new[] { $"stateDiagram-v2" }.Union(Transitions.Select(ToMermaid)));
    private static string ToMermaid(Transition<StateT, TriggerT> transition) => $"{transition.SourceState} --> {transition.DestinationState} : {transition.Trigger}";

}
public record Transition<StateT, TriggerT>
{
    public StateT SourceState { get; init; }
    public StateT DestinationState { get; init; }
    public TriggerT Trigger { get; init; }
    public Func<bool>? IsAllowed { get; init; }
}

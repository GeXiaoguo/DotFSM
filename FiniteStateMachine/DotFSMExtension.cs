namespace DotFSM;

using System;
using System.Collections.Generic;
using System.Linq;

public static class DotFSMExtension
{
    public static IEnumerable<TriggerT> AllowedTriggers<StateT, TriggerT>(this DotFSM<StateT, TriggerT> dotFSM, StateT givenState)
        where StateT : IComparable
        where TriggerT : IComparable
        => dotFSM.Transitions
           .Where(x => x.SourceState.Equals(givenState))
           .Select(x => x.Trigger);
    public static Transition<StateT, TriggerT>? GetTransition<StateT, TriggerT>(this DotFSM<StateT, TriggerT> dotFSM, StateT currentState, TriggerT trigger)
        where StateT : IComparable
        where TriggerT : IComparable
        => dotFSM.Transitions
           .SingleOrDefault(x => x.SourceState.CompareTo(currentState) == 0 && x.Trigger.CompareTo(trigger) == 0);
    public static string ToMermaidDiagram<StateT, TriggerT>(this DotFSM<StateT, TriggerT> dotFSM)
        where StateT : IComparable
        where TriggerT : IComparable
        => string.Join("\r\n", new[] { $"stateDiagram-v2" }.Union(dotFSM.Transitions.Select(ToMermaid)));
    private static string ToMermaid<StateT, TriggerT>(Transition<StateT, TriggerT> transition)
        => $"{transition.SourceState} --> {transition.DestinationState} : {transition.Trigger}";
}
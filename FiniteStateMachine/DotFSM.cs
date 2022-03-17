namespace DotFSM;

using System;
using System.Collections.Generic;
using System.Linq;
public record class Transition<StateT, TriggerT>(StateT SourceState, StateT DestinationState, TriggerT Trigger);
public class DotFSM<StateT, TriggerT>
    where StateT : IComparable
    where TriggerT : IComparable
{
    public IEnumerable<Transition<StateT, TriggerT>> Transitions { get; }
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
}

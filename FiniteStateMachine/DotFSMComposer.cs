namespace DotFSM;

using System;
using System.Linq;

public static class DotFSMComposer
{
    public static DotFSM<(StateT1, StateT2), TriggerT> Combine<StateT1, StateT2, TriggerT>(this DotFSM<StateT1, TriggerT> dotFSM1, DotFSM<StateT2, TriggerT> dotFSM2)
        where StateT1 : IComparable
        where StateT2 : IComparable
        where TriggerT : IComparable
    {
        var transitions = dotFSM1
            .Transitions
            .Join(
                dotFSM2.Transitions,
                tran => tran.Trigger,
                tran => tran.Trigger,
                (tran1, tran2) => new Transition<(StateT1, StateT2), TriggerT>
                (
                    SourceState: (tran1.SourceState, tran2.SourceState),
                    Trigger: tran1.Trigger,
                    DestinationState: (tran1.DestinationState, tran2.DestinationState)
                )
            );

        return new DotFSM<(StateT1, StateT2), TriggerT>(transitions);
    }
}

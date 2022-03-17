using System;
using System.Collections.Generic;

namespace DotFSM;
public class Builder<StateT, TriggerT>
        where StateT : IComparable
        where TriggerT : IComparable
{
    private List<Transition<StateT, TriggerT>> _transitions { get; } = new List<Transition<StateT, TriggerT>>();
    public StateT state;
    private Builder(StateT srcState)
    {
        state = srcState;
    }
    public Builder<StateT, TriggerT> ForState(StateT srcState) 
    {
        state = srcState;
        return this;
    }
    public static Builder<StateT, TriggerT> Start(StateT srcState) => new Builder<StateT, TriggerT>(srcState);
    public Builder<StateT, TriggerT> Allow(TriggerT trigger, StateT destState)
    {
        _transitions.Add(new Transition<StateT, TriggerT>
        (
            SourceState: state,
            Trigger: trigger,
            DestinationState: destState
        ));
        return this;
    }
    public DotFSM<StateT, TriggerT> Build() => new DotFSM<StateT, TriggerT>(_transitions);
}
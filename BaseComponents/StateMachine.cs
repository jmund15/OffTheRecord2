 using Godot;
using System;
using TimeRobbers.BaseComponents;
using Godot.Collections;

[GlobalClass]
public partial class StateMachine : Node
{
	[Export]
	protected State InitialState;

	protected State PrimaryState;
	//protected List<State> ActiveParallelStates = new List<State>();

	protected Dictionary<State, bool> FiniteStates = new Dictionary<State, bool>();
    protected Dictionary<State, bool> ParallelStates = new Dictionary<State, bool>();
    //protected Dictionary<SecondaryRobberState, bool> SecondaryStates = new Dictionary<SecondaryRobberState, bool>();

    [Signal]
    public delegate void TransitionedStateEventHandler(State oldState, State newState);
    public virtual void Init(CharacterBody2D body, AnimationPlayer animPlayer)
	{
        if (body is not IDirectionComponent dirComponent)
		{
			GD.PrintErr("STATE MACHINE INIT ERROR || Body MUST Implement 'IDirectionComponenet'!");
			throw new Exception("STATE MACHINE INIT ERROR || Body MUST Implement 'IDirectionComponenet'!");
			//return;
		}
        foreach (var child in GetChildren())
		{
			if (child is not State state) { continue; }
			state.Body = body;
			state.DirectionComponent = dirComponent;
			state.AnimPlayer = animPlayer;
			state.TransitionState += TransitionState;
			state.AddParallelState += AddParallelState;
			if (state is IParallelState stateParallel)
			{
				ParallelStates.Add(state, false);
			}
			else
			{
				FiniteStates.Add(state, false);
			}
			state.Init(body, animPlayer);
		}
		InitialState.Enter(ParallelStates);
		FiniteStates[InitialState] = true;
        //foreach (var secondaryState in Global.GetEnumValues<SecondaryRobberState>())
        //{
        //    SecondaryStates.Add(secondaryState, false);
        //}
    }
	public virtual void TransitionState(State oldState, State newState)
	{
		if (ParallelStates[oldState])
		{
			oldState.Exit();
			ParallelStates[oldState] = false;
			newState.Enter(ParallelStates);
			ParallelStates[newState] = true;
		}
		if (PrimaryState != null) {
			if (PrimaryState != oldState) { return; }
            if (PrimaryState == newState) { return; }
            PrimaryState.Exit();
            FiniteStates[PrimaryState] = false;
        }
		PrimaryState = newState;
		PrimaryState.Enter(ParallelStates); 
        FiniteStates[PrimaryState] = true;
		EmitSignal(SignalName.TransitionedState, oldState, newState);
    }
	public virtual void AddParallelState(State state)
	{
		if (state is not IParallelState parallelState) {
			GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
			return;
		}
		if (ParallelStates[state])
		{
			GD.PrintErr("WARNING || Can't add parallel state that is already active!");
			return;
		}
		state.Enter(ParallelStates);
		ParallelStates[state] = true;
	}
	public virtual void RemoveParallelState(State state)
	{
        if (state is not IParallelState parallelState)
        {
            GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
            return;
        }
        if (!ParallelStates[state])
        {
            GD.PrintErr("WARNING || Can't exit parallel state that is already disactive!");
            return;
        }
        state.Exit();
        ParallelStates[state] = false;
    }
    public virtual void ProcessFrame(float delta)
	{
		PrimaryState.ProcessFrame(delta);
		foreach (var parallelState in ParallelStates)
		{
			if (parallelState.Value)
			{
				parallelState.Key.ProcessFrame(delta);
			}
		}
	}
	public virtual void ProcessPhysics(float delta)
	{
		PrimaryState.ProcessPhysics(delta);
        foreach (var parallelState in ParallelStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.ProcessFrame(delta);
            }
        }
    }
}

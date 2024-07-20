using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

[GlobalClass]
public abstract partial class CompoundState : State
{
    #region STATE_VARIABLES
    [Export]
    public State InitialSubState { get; protected set; }
    protected State PrimarySubState;

    public Dictionary<State, bool> FiniteSubStates { get; protected set; } = new Dictionary<State, bool>();
    public Dictionary<State, bool> ParallelSubStates { get; protected set; } = new Dictionary<State, bool>();

    [Signal]
    public delegate void TransitionedStateEventHandler(State oldState, State newState);
    [Signal]
    public delegate void AddedParallelStateEventHandler(State parallelState);
    [Signal]
    public delegate void RemovedParallelStateEventHandler(State parallelState);
    #endregion

    #region STATE_UPDATES
    public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
    {
        if (body is not IDirectionComponent dirComponent)
        {
            GD.PrintErr("COMPOUND STATE INIT ERROR || Body MUST Implement 'IDirectionComponenet'!");
            throw new Exception("COMPOUND STATE INIT ERROR || Body MUST Implement 'IDirectionComponenet'!");
            //return;
        }
        base.Init(body, animPlayer);
        foreach (var child in GetChildren())
        {
            //GD.Print("parent state: ", Name, ", child state: ", child.Name);
            if (child is not State state) { continue; }
            state.Init(body, animPlayer);
            state.TransitionState += TransitionFiniteSubState;
            state.AddParallelState += AddParallelSubState;
            state.RemoveParallelState += RemoveParallelSubState;
            if (state is IParallelState stateParallel)
            {
                ParallelSubStates.Add(state, false);
            }
            else
            {
                FiniteSubStates.Add(state, false);
            }
        }
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        //GD.Print("just entered compound state: ", Name, "\nParallel Sub States: ", ParallelSubStates);
        InitialSubState.Enter(ParallelSubStates);
        FiniteSubStates[InitialSubState] = true;
        PrimarySubState = InitialSubState;
    }
    public override void Exit()
    {
        base.Exit(); 
        PrimarySubState.Exit();
        FiniteSubStates[PrimarySubState] = false;
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.Exit();
            }
        }
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        PrimarySubState.ProcessFrame(delta);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.ProcessFrame(delta);
            }
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        PrimarySubState.ProcessPhysics(delta);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.ProcessPhysics(delta);
            }
        }
    }
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
        PrimarySubState.HandleInput(@event);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.HandleInput(@event);
            }
        }
    }
    #endregion
    #region STATE_HELPER
    public virtual void TransitionFiniteSubState(State oldSubState, State newSubState)
    {
        if (newSubState == null)
        {
            throw new Exception($"COMPOUND STATE ERROR || NEW STATE TRANSITIONED FROM \"{oldSubState.Name}\" HAS NOT BEEN SET IN THE EDITOR AND IS NULL");
        }
        //if (ParallelSubStates[oldSubState])
        //{
        //    oldSubState.Exit();
        //    ParallelSubStates[oldSubState] = false;
        //    newSubState.Enter(ParallelStates);
        //    ParallelSubStates[newSubState] = true;
        //}
        if (PrimarySubState != null)
        {
            if (PrimarySubState != oldSubState) { return; }
            if (PrimarySubState == newSubState) { return; }
            PrimarySubState.Exit();
            FiniteSubStates[PrimarySubState] = false;
        }

        PrimarySubState = newSubState;
        PrimarySubState.Enter(ParallelSubStates);
        FiniteSubStates[PrimarySubState] = true;
        EmitSignal(SignalName.TransitionedState, oldSubState, newSubState);
    }
    public virtual void AddParallelSubState(State state)
    {
        if (state is not IParallelState parallelState)
        {
            GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
            return;
        }
        if (ParallelSubStates[state])
        {
            GD.PrintErr("WARNING || Can't add parallel state that is already active!");
            return;
        }
        state.Enter(ParallelSubStates);
        ParallelSubStates[state] = true;
        EmitSignal(SignalName.AddedParallelState, state);
    }
    public virtual void RemoveParallelSubState(State state)
    {
        if (state is not IParallelState parallelState)
        {
            GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
            return;
        }
        if (!ParallelSubStates[state])
        {
            GD.PrintErr("WARNING || Can't exit parallel state that is already disactive!");
            return;
        }
        state.Exit();
        ParallelSubStates[state] = false;
        EmitSignal(SignalName.RemovedParallelState, state);
    }
    #endregion
}

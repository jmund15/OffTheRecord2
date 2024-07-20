using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

[GlobalClass]
public abstract partial class State : Node2D
{
    #region STATE_VARIABLES
    [Export]
    public string AnimName { get; private set; }
    public CharacterBody2D Body { get; set; }
    public IDirectionComponent DirectionComponent { get; set; }
    public AnimatedSprite2D AnimSprite { get; set; }

    protected Dictionary<State, bool> ParallelStates = new Dictionary<State, bool>();

    [Signal]
    public delegate void TransitionStateEventHandler(State oldState, State newState);
    [Signal]
    public delegate void AddParallelStateEventHandler(State parallelState);
    [Signal]
    public delegate void RemoveParallelStateEventHandler(State parallelState);
    #endregion
    #region STATE_UPDATES
    public virtual void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
    {
        Body = body;
        AnimSprite = animPlayer;
        DirectionComponent = (Body as IDirectionComponent);
    }

    public virtual void Enter(Dictionary<State, bool> parallelStates)
    {
        GD.Print($"{Body.Name} Just entered state: ", Name);
        ParallelStates = parallelStates;
    }
    public virtual void Exit()
    {
    }
    public virtual void ProcessFrame(float delta)
    {
    }
    public virtual void ProcessPhysics(float delta)
    {
    }
    public virtual void HandleInput(InputEvent @event)
    {
    }
    #endregion
    #region STATE_HELPER
    #endregion
}

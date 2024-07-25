using Godot;
using Godot.Collections;
using System;

public partial class MonsterStateMachine : CompoundState
{
    private Monster _monster;
    private HurtboxComponent _hurtboxComponent;
    private HitboxComponent _hitboxComponent;

    [Export(PropertyHint.NodeType, "State")]
    private DevourLimbState _devourLimbState;
    [Export(PropertyHint.NodeType, "State")]
    private LungeState _lungeState;
    public override void Init(CharacterBody2D body, AnimatedSprite2D animSprite)
    {
        if (body is not Monster monster)
        {
            GD.PrintErr("MONSTER STATE MACHINE INIT ERROR || Body MUST be of type Monster!");
            throw new Exception("MONSTER STATE MACHINE INIT ERROR || Body MUST be of type Monster!");
        }
        _monster = body as Monster;
        _hurtboxComponent = monster.HurtboxComponent;
        _hitboxComponent = monster.HitboxComponent;
        base.Init(body, animSprite);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _monster.DevourLimb += OnDevourLimb;
        _monster.Lunge += OnLunge;
        _hurtboxComponent.HitboxEntered += OnHitboxEntered;
    }
    public override void Exit()
    {
        base.Exit();
        _monster.DevourLimb -= OnDevourLimb;
        _hurtboxComponent.HitboxEntered -= OnHitboxEntered;
    }
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
    }

    private void OnLunge()
    {
        if (PrimarySubState is DevourLimbState) { return; }
        TransitionFiniteSubState(PrimarySubState, _lungeState);
    }
    private void OnDevourLimb(int newLimbCount)
    {
        TransitionFiniteSubState(PrimarySubState, _devourLimbState);
    }
    public override void TransitionFiniteSubState(State oldState, State newState)
    {
        base.TransitionFiniteSubState(oldState, newState);
    }
    public override void AddParallelSubState(State state)
    {
        base.AddParallelSubState(state);
    }
    public override void RemoveParallelSubState(State state)
    {
        base.RemoveParallelSubState(state);
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
    }
    private void OnHitboxEntered(HitboxComponent hitbox)
    {
        //if (hitbox == _hitboxComponent)
        //{
        //    return;
        //}
        //if (_hurtboxComponent.Invulnerable || _hurtboxComponent.Defending)
        //{
        //    return;
        //}
        //TransitionFiniteSubState(PrimarySubState, _devourLimbState);
    }
}

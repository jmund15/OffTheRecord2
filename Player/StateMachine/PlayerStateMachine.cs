using Godot;
using Godot.Collections;
using System;

public partial class PlayerStateMachine : CompoundState
{
    private Player _player;
    private HealthComponent _healthComponent;
    private HurtboxComponent _hurtboxComponent;
    private HitboxComponent _hitboxComponent;

    [Export(PropertyHint.NodeType, "State")]
    private LoseLimbState _loseLimbState;
    public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
    {
        base._Ready();
        if (body is not Player player) 
        {
            GD.PrintErr("ROBBER STATE MACHINE INIT ERROR || Body MUST be of type Player!");
            throw new Exception("ROBBER STATE MACHINE INIT ERROR || Body MUST be of type Player!");
            //return;
        }
        _player = player;
        _healthComponent = player.HealthComponent;
        _hurtboxComponent = player.HurtboxComponent;
        _hitboxComponent = player.HitboxComponent;
        base.Init(body, animPlayer);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _player.LoseLimb += OnLoseLimb;
        _player.LimbHealthStateChange += OnLimbHealthStateChange;
        _healthComponent.HealthChanged += OnHealthChanged;
        _hurtboxComponent.HitboxEntered += OnHitboxEntered;
    }
    public override void Exit()
    {
        base.Exit();
        _player.LoseLimb -= OnLoseLimb;
        _player.LimbHealthStateChange -= OnLimbHealthStateChange;
        _healthComponent.HealthChanged -= OnHealthChanged;
        _hurtboxComponent.HitboxEntered -= OnHitboxEntered;
    }
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
    }
    private void OnLoseLimb(int newLimbCount)
    {
        GD.Print("transitioning to lose limb");
        TransitionFiniteSubState(PrimarySubState, _loseLimbState);
    }
    private void OnLimbHealthStateChange(Player.LimbHealthState newLimbHealthState)
    {
        var animPlaying = AnimSprite.IsPlaying();
        int currAnimFrame = 0;
        float currAnimFrameProg = 0.0f;
        if (animPlaying)
        {
            currAnimFrame = AnimSprite.Frame;
            currAnimFrameProg = AnimSprite.FrameProgress;
        }

        if (!animPlaying) { return; }
        AnimSprite.Play(PrimarySubState.AnimName + _player.LimbCount + _player.LimbHealthAnimString[newLimbHealthState]);
        AnimSprite.SetFrameAndProgress(currAnimFrame, currAnimFrameProg);
    }
    private void OnHealthChanged(HealthUpdate healthUpdate)
    {

    }

    private void UpdateStatesAnimPlayers(CompoundState state)
    {
        foreach (var finiteState in state.FiniteSubStates.Keys) // for each state change anim player
        {
            if (finiteState is CompoundState compoundState)
            {
                UpdateStatesAnimPlayers(compoundState);
            }
            finiteState.AnimSprite = _player.AnimSprite;
        }
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
        if (hitbox == _hitboxComponent)
        {
            return;
        }
        if (_hurtboxComponent.Invulnerable || _hurtboxComponent.Defending)
        {
            return;
        }
        TransitionFiniteSubState(PrimarySubState, _loseLimbState);
    }
}

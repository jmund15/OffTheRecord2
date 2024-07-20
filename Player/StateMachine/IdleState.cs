using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

public partial class IdleState : State
{
    #region STATE_VARIABLES
    [Export]
    private string _animName;

    private Player _player;
    [Export]
    private State _walkState;
    [Export]
    private State _injectState;
    [Export]
    private State _interactState;

    private Vector2 _inputDirection = new Vector2();

    private bool _bufferingMovementTransition = false;
    #endregion
    #region STATE_UPDATES
    public override void Init(CharacterBody2D body, AnimationPlayer animPlayer)
    {
        base.Init(body, animPlayer);
        _player = Body as Player;
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        //AnimPlayer = _player.AnimPlayer;
        AnimPlayer.Play(_animName + IDirectionComponent.GetFaceDirectionString(_player.FaceDirection));
        _bufferingMovementTransition = false;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = DirectionComponent.GetDesiredDirection();

        if (!_inputDirection.IsZeroApprox() &&
             !_bufferingMovementTransition)
        { //BUFFER AFTER MOVEMENT CHANGES
            GetTree().CreateTimer(Player.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            _bufferingMovementTransition = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);
        //var currRunPos = AnimPlayer.CurrentAnimation == string.Empty ? 0.0 : AnimPlayer.CurrentAnimationPosition;
        //AnimPlayer.Seek(currRunPos, true);
    }
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed(_player.InjectInput))
        {
            EmitSignal(SignalName.TransitionState, this, _attackState);
        }
    }
    #endregion
    #region STATE_HELPER
    private void ChangeMovementState()
    {
        //if (_inputDirection.Length() > RobberHelper.WalkMaxInput)
        //{
        //    EmitSignal(SignalName.TransitionState, this, _runState);
        //}
        //else if (_inputDirection.Length() > RobberHelper.SneakMaxInput)
        //{
        //    EmitSignal(SignalName.TransitionState, this, _walkState);
        //}
        //else
        if (!_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _walkState);
        }
        _bufferingMovementTransition = false;
    }
    //private void OnHitboxEntered(HitboxComponent hitbox)
    //{
    //    if (_hurtboxComponent.Invulnerable)
    //    {
    //        return;
    //    }
    //    var attack = hitbox.CurrentAttack;
    //    var force = attack.Force - (_player.BagForce / 2);
    //    var minForce = -0.3f;
    //    var maxForce = 0.9f;
    //    var normForce = (force - minForce) / (maxForce - minForce);
    //    _player.EffectDirection = attack.Direction.Normalized();
    //    MovementDirection nextFaceDir = IDirectionComponent.GetOppositeDirection(attack.FaceDirection);

    //    if (hitbox.GetParent() is Player)
    //    {
    //        _popComponent.DefendingRobberHit(nextFaceDir);
    //        //ATTACKER IS EQUAL OR HEAVIER THAN DEFENDER
    //        if (_hurtboxComponent.LatestAttack.Force >= _player.BagForce)
    //        {
    //            _popState.PostPopState = _fallState;
    //        }
    //        else //ATTACKER IS LIGHTER THAN DEFENDER
    //        {
    //            _popState.PostPopState = _hitState;
    //        }
    //        _player.RobbersInteractDropCoins(normForce, attack.Force);

    //        //SetDeferred(PropertyName.FaceDirection, Variant.From(newFaceDir)); // MAYBE DON'T USE TEST FIRST

    //        AnimPlayer.CallDeferred(AnimationPlayer.MethodName.Play, "hit" + IDirectionComponent.GetFaceDirectionString(nextFaceDir));
    //        EmitSignal(SignalName.TransitionState, this, _popState);
    //    }
    //    else //non robber attacker
    //    {
    //        if (_hurtboxComponent.LatestAttack.Force >= _player.BagForce)
    //        {
    //            EmitSignal(SignalName.TransitionState, this, _fallState);
    //        }
    //        else //ATTACKER IS LIGHTER THAN DEFENDER
    //        {
    //            EmitSignal(SignalName.TransitionState, this, _hitState);
    //        }
    //        _player.RobberDamaged((int)attack.Damage, _player.EffectDirection, normForce);
    //    }
    //}

    #endregion
}

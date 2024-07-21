using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

public partial class IdleState : State
{
    #region STATE_VARIABLES
    //[Export]
    //private string _animName;

    private Player _player;
    [Export]
    private WalkState _walkState;
    [Export]
    private InjectState _injectState;
    [Export]
    private InteractState _interactState;

    private Vector2 _inputDirection = new Vector2();

    private bool _bufferingMovementTransition = false;
    #endregion
    #region STATE_UPDATES
    public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
    {
        base.Init(body, animPlayer);
        _player = Body as Player;
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _player.CanMove = true;
        //AnimSprite = _player.AnimSprite;
        AnimSprite.Play(AnimName + _player.LimbCount + _player.LimbHealthAnimString[_player.CurrLimbHealthState]);
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
        if (!_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _walkState);
            //GetTree().CreateTimer(Player.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            //_bufferingMovementTransition = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);
        //var currRunPos = AnimSprite.CurrentAnimation == string.Empty ? 0.0 : AnimSprite.CurrentAnimationPosition;
        //AnimSprite.Seek(currRunPos, true);
    }
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed(_player.InjectInput) && _player.CuresHeld > 0)
        {
            EmitSignal(SignalName.TransitionState, this, _injectState);
        }
        //else if (@event.IsActionPressed(_player.InteractInput))
        //{
        //    EmitSignal(SignalName.TransitionState, this, _interactState);
        //}
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
        GD.Print("changing movement?");
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

    //        AnimSprite.CallDeferred(AnimatedSprite2D.MethodName.Play, "hit" + IDirectionComponent.GetFaceDirectionString(nextFaceDir));
    //        EmitSignal(SignalName.TransitionState, this, _popState);
    //    }
    //    else //non player attacker
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

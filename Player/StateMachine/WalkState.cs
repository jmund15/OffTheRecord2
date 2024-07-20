using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

public partial class WalkState : CompoundState
{
    #region STATE_VARIABLES
    [Export]
    private string _animName;

    private Player _player;

    [Export]
    private State _idleState;

    private Vector2 _inputDirection = new Vector2();
    private MovementDirection _faceDirection;
    private bool _walled = false;

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
        AnimPlayer = _player.AnimPlayer;

        _inputDirection = DirectionComponent.GetDesiredDirectionNormalized();
        _faceDirection = IDirectionComponent.GetDirectionFromVector(_inputDirection);
      
        AnimPlayer.Play(_animName + IDirectionComponent.GetFaceDirectionString(_faceDirection));
        AnimPlayer.Advance(0);

        _walled = true; // precaution
    }
    public override void Exit()
    {
        base.Exit();
        AnimPlayer.SpeedScale = 1.0f;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = DirectionComponent.GetDesiredDirection();

        if (_inputDirection.IsZeroApprox())
        { //BUFFER AFTER MOVEMENT CHANGES
            GetTree().CreateTimer(Player.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            _bufferingMovementTransition = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        Body.Velocity = _inputDirection.Normalized() * Player.WalkSpeed * delta;
        Body.MoveAndSlide();

        var newDir = IDirectionComponent.GetDirectionFromVector(_inputDirection);
        if (_faceDirection != newDir)
        {
            var animPos = AnimPlayer.CurrentAnimationPosition;
            _faceDirection = IDirectionComponent.GetDirectionFromVector(_inputDirection);
            var animString = IDirectionComponent.GetFaceDirectionString(_faceDirection);
            AnimPlayer.Play(_animName + animString);
            AnimPlayer.Seek(animPos, true);
        }

        //var currRunPos = AnimPlayer.CurrentAnimation == string.Empty ? 0.0 : AnimPlayer.CurrentAnimationPosition;
        if (Body.IsOnWall())
        {
            //EmitSignal(SignalName.TransitionState, this, _wallState);
        }
        if (Body.GetSlideCollisionCount() > 0 && !_inputDirection.IsZeroApprox())
        {
            //var coll = Body.GetLastSlideCollision();
            //var collAngle = coll.GetAngle(_inputDirection.Normalized());
            //GD.Print("robber jsut collided @ normal: ", coll.GetNormal(), " and angle: ", collAngle,
            //    ", wall min slide angle: ", Body.WallMinSlideAngle);
            //if (collAngle > (Mathf.Pi / 2) + RobberHelper.HugWallAngle)
            //{
            //    EmitSignal(SignalName.TransitionState, this, _wallState);
            //}
            //else if (collAngle > (Mathf.Pi / 2) + RobberHelper.SneakWallAngle)
            //{
            //    EmitSignal(SignalName.TransitionState, this, _sneakState);
            //}
            //else if (collAngle > (Mathf.Pi / 2) + RobberHelper.WalkWallAngle)
            //{
            //    _walled = true;
            //}
            //else
            //{
            //    _walled = false;
            //}
        }
        else if (_walled) { _walled = false; }
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
        if (_inputDirection.Length() == 0)
        {
            EmitSignal(SignalName.TransitionState, this, _idleState);
        }
        _bufferingMovementTransition = false;
    }
    #endregion
}

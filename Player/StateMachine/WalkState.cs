using Godot;
using Godot.Collections;
using System;
using System.Text;
using TimeRobbers.BaseComponents;

public partial class WalkState : State
{
    #region STATE_VARIABLES
    //[Export]
    //private string _animName;

    private Player _player;

    [Export]
    private IdleState _idleState;
    [Export]
    private InjectState _injectState;
    [Export]
    private InteractState _interactState;

    private Vector2 _inputDirection = new Vector2();
    private MovementDirection _faceDirection;
    private bool _walled = false;

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

        _inputDirection = DirectionComponent.GetDesiredDirectionNormalized();
        _faceDirection = IDirectionComponent.GetDirectionFromVector(_inputDirection);
      
        AnimSprite.Play(AnimName + _player.LimbCount + _player.LimbHealthAnimString[_player.CurrLimbHealthState]);
        _walled = true; // precaution
    }
    public override void Exit()
    {
        base.Exit();
        AnimSprite.SpeedScale = 1.0f;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = DirectionComponent.GetDesiredDirection();

        if (_inputDirection.IsZeroApprox())
        { //BUFFER AFTER MOVEMENT CHANGES
            EmitSignal(SignalName.TransitionState, this, _idleState);
            //GetTree().CreateTimer(Player.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            //_bufferingMovementTransition = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        Body.Velocity = _inputDirection.Normalized() * _player.CalcMovementSpeed() * _player.CurrentSpeed * delta;
        Body.MoveAndSlide();

        //var currRunPos = AnimSprite.CurrentAnimation == string.Empty ? 0.0 : AnimSprite.CurrentAnimationPosition;
        if (Body.IsOnWall())
        {
            //EmitSignal(SignalName.TransitionState, this, _wallState);
        }
        if (Body.GetSlideCollisionCount() > 0 && !_inputDirection.IsZeroApprox())
        {
            //var coll = Body.GetLastSlideCollision();
            //var collAngle = coll.GetAngle(_inputDirection.Normalized());
            //GD.Print("player jsut collided @ normal: ", coll.GetNormal(), " and angle: ", collAngle,
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
        if (_inputDirection.Length() == 0)
        {
            EmitSignal(SignalName.TransitionState, this, _idleState);
        }

        _bufferingMovementTransition = false;
    }
    #endregion
}

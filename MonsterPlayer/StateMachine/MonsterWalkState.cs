using Godot;
using Godot.Collections;
using TimeRobbers.BaseComponents;

public partial class MonsterWalkState : State
{
    #region STATE_VARIABLES
    //[Export]
    //private string _animName;

    private Monster _monster;

    [Export]
    private MonsterIdleState _idleState;
    [Export]
    private LungeState _lungeState;

    private Vector2 _inputDirection = new Vector2();
    private MovementDirection _faceDirection;
    #endregion
    #region STATE_UPDATES
    public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
    {
        base.Init(body, animPlayer);
        _monster = Body as Monster;
    }

    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _monster.CanMove = true;

        _inputDirection = DirectionComponent.GetDesiredDirectionNormalized();
        _faceDirection = IDirectionComponent.GetDirectionFromVector(_inputDirection);

        AnimSprite.Play(AnimName + _monster.LimbCount);
    }
    public override void Exit()
    {
        base.Exit();
        AnimSprite.SpeedScale = 1.0f;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = DirectionComponent.GetDesiredDirectionNormalized();

        if (_inputDirection.IsZeroApprox())
        { //BUFFER AFTER MOVEMENT CHANGES
            EmitSignal(SignalName.TransitionState, this, _idleState);
            //GetTree().CreateTimer(Monster.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            //_bufferingMovementTransition = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        Body.Velocity = _inputDirection.Normalized() * _monster.CalcMovementSpeed() * _monster.CurrentSpeed * delta;
        Body.MoveAndSlide();
    }
    #endregion
    #region STATE_HELPER

    #endregion
}

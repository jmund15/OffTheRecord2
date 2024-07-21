using Godot;
using Godot.Collections;

public partial class MonsterIdleState : State
{
    #region STATE_VARIABLES

    private Monster _monster;
    [Export]
    private MonsterWalkState _walkState;
    [Export]
    private LungeState _lungeState;

    private Vector2 _inputDirection = new Vector2();
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
        //AnimSprite = _monster.AnimSprite;
        AnimSprite.Play(AnimName + _monster.LimbCount);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = DirectionComponent.GetDesiredDirection();
        GD.Print("monster input dir: ", _inputDirection);
        if (!_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _walkState);
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        //var currRunPos = AnimSprite.CurrentAnimation == string.Empty ? 0.0 : AnimSprite.CurrentAnimationPosition;
        //AnimSprite.Seek(currRunPos, true);
    }
    #endregion
    #region STATE_HELPER
    #endregion
}

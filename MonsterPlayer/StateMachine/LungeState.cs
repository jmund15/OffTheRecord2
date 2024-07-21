using Godot;
using Godot.Collections;
using System;

public partial class LungeState : State
{
	#region STATE_VARIABLES
	private Monster _monster;

    [Export(PropertyHint.NodeType, "State")]
    private State _idleState;

    private Vector2 _lungeDir;
    private float _lungeFriction = 10;
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
        _lungeDir = DirectionComponent.GetDesiredDirectionNormalized();

        AnimSprite.Play(AnimName + _monster.LimbCount);
		AnimSprite.AnimationFinished += OnAnimationFinished;
    }
    public override void Exit()
	{
		base.Exit();
        Body.Velocity = Vector2.Zero;
        AnimSprite.AnimationFinished -= OnAnimationFinished;
    }
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        //if (AnimSprite.Frame <= 4)
        //{
            Body.Velocity = _lungeDir * _monster.CalcMovementSpeed() * Monster.LungeSpeed * delta;
        //}
        //else
        //{
        //    Body.Velocity -= Body.Velocity * _monster.CalcMovementSpeed() * _lungeFriction * delta;
        //}
        Body.MoveAndSlide();
    }
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
    #endregion
    #region STATE_HELPER
    private void OnAnimationFinished()
    {
        EmitSignal(SignalName.TransitionState, this, _idleState);
    }
    #endregion
}

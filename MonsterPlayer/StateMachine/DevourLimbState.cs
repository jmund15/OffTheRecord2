using Godot;
using Godot.Collections;

public partial class DevourLimbState : State
{
	//TEMPLATE FOR STATES
	#region STATE_VARIABLES
	private Monster _monster;

	[Export(PropertyHint.NodeType, "State")]
	private State _idleState;
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
        AnimSprite.Play(AnimName + _monster.LimbCount);
        AnimSprite.AnimationFinished += OnAnimationFinished;
    }
	public override void Exit()
	{
		base.Exit();
        _monster.LimbCount++;
        AnimSprite.AnimationFinished -= OnAnimationFinished;
	}
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
    #endregion
    #region STATE_HELPER
    private void OnAnimationFinished()
    {
		_monster.LimbDevouring.QueueFree();
		if (_monster.LimbsToDevour.Contains(_monster.LimbDevouring))
		{
			_monster.LimbsToDevour.Remove(_monster.LimbDevouring);
		}
        EmitSignal(SignalName.TransitionState, this, _idleState);
    }
    #endregion
}

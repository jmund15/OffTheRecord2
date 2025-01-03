using Godot;
using Godot.Collections;

public partial class InteractState : State
{
	//TEMPLATE FOR STATES
	#region STATE_VARIABLES
	private Player _player;
	[Export(PropertyHint.NodeType, "State")]
	private IdleState _idleState;
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
        _player.CanMove = false;
    }
	public override void Exit()
	{
		base.Exit();
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
	#endregion
}

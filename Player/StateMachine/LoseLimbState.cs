using Godot;
using Godot.Collections;

public partial class LoseLimbState : State
{
	//TEMPLATE FOR STATES
	#region STATE_VARIABLES
	[Export(PropertyHint.NodeType, "State")]
	private IdleState _idleState;
	#endregion
	#region STATE_UPDATES
	public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
	{
		base.Init(body, animPlayer);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		EmitSignal(SignalName.TransitionState, this, _idleState);
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

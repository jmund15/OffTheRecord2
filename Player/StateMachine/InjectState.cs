using Godot;
using Godot.Collections;

public partial class InjectState : State
{
	//TEMPLATE FOR STATES
	#region STATE_VARIABLES
	[Export(PropertyHint.NodeType, "State")]
	private State _transitionState;
	#endregion
	#region STATE_UPDATES
	public override void Init(CharacterBody2D body, AnimationPlayer animPlayer)
	{
		base.Init(body, animPlayer);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
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

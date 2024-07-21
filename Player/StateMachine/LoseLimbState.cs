using Godot;
using Godot.Collections;
using System;

public partial class LoseLimbState : State
{
	//TEMPLATE FOR STATES
	#region STATE_VARIABLES
	private Global _global;
	private Player _player;

	[Export(PropertyHint.NodeType, "State")]
	private IdleState _idleState;

	private SeveredLimb _severedLimb;
	#endregion
	#region STATE_UPDATES
	public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
	{
		base.Init(body, animPlayer);
		_player = Body as Player;
		_global = GetNode<Global>("/root/Global");

    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
        _player.CanMove = false;


		_player.AnimSprite.Play(AnimName + _player.LimbCount);
		_player.AnimSprite.AnimationFinished += OnAnimationFinished;

        _severedLimb = _player.SeveredLimbScene.Instantiate<SeveredLimb>();
		
		var limbPos = (Global.Monster.GlobalPosition - _player.GlobalPosition);
		if (!Global.Monster.AttackedPlayer)
		{
			limbPos = _global.GetRandomDirection().Normalized() * 25f;
        }
        _severedLimb.GlobalPosition = _player.GlobalPosition + (limbPos * 0.25f);
		_global.MainScene.CallDeferred(MainScene.MethodName.AddChild, _severedLimb);
        //_global.MainScene.AddChild(_severedLimb);
		
	}
    public override void Exit()
	{
		base.Exit();
        _player.AnimSprite.AnimationFinished -= OnAnimationFinished;
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
		_player.EmitSignal(Player.SignalName.LimbDetached, _severedLimb);
        EmitSignal(SignalName.TransitionState, this, _idleState);
    }
    #endregion
}

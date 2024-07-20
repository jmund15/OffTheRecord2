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

        var limb = _player.SeveredLimbScene.Instantiate<SeveredLimb>();
		var rndLimbPos = _global.GetRandomDirection();
		limb.GlobalPosition = _player.GlobalPosition + (rndLimbPos * 20);
        _global.MainScene.AddChild(limb);
		switch(_player.LimbCount)
		{
			case 3:
				limb.LimbSprite.Play("arm"); break;
            case 2:
                limb.LimbSprite.Play("leg1"); break;
            case 1:
                limb.LimbSprite.Play("leg2"); break;
			default: break;
        }
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
		_player.EmitSignal(Player.SignalName.LimbDetached);
        EmitSignal(SignalName.TransitionState, this, _idleState);
    }
    #endregion
}

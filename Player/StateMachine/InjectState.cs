using Godot;
using Godot.Collections;
using System;

public partial class InjectState : State
{
	#region STATE_VARIABLES
	private Player _player;

	[Export(PropertyHint.NodeType, "State")]
	private IdleState _idleState;

    private bool _isHealing = false;
	#endregion
	#region STATE_UPDATES
	public override void Init(CharacterBody2D body, AnimatedSprite2D animPlayer)
	{
		base.Init(body, animPlayer);
		_player = Body as Player;
        _player.CureFlare.AnimationFinished += OnFlareAnimationFinished;
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		_player.CanMove = false;

        AnimSprite.Play(AnimName + _player.LimbCount + _player.LimbHealthAnimString[_player.CurrLimbHealthState]);
		AnimSprite.AnimationFinished += OnAnimationFinished;
    }
    public override void Exit()
	{
		base.Exit();
        _player.Healing = false;
        _player.CureFlare.Play("flareEnd");
	}
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
        if (_player.Healing)
        {
            _player.HealthComponent.Heal(_player.CureSpeed * delta);
            if (_player.HealthComponent.Health == _player.HealthComponent.MaxHealth)
            {
                EmitSignal(SignalName.TransitionState, this, _idleState);
            }
        }
    }
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
        if (@event.IsActionReleased(_player.InjectInput))
        {
            EmitSignal(SignalName.TransitionState, this, _idleState);
        }
    }
    #endregion
    #region STATE_HELPER
    private void OnAnimationFinished()
    {
        _player.CureFlare.Play("flareIntro");
        _player.CureFlareMask.Play("mask");
        _player.CureFlare.Show();
        _player.CureFlareMask.Show();

        _player.Healing = true;
        _player.CuresHeld--;

        AnimSprite.AnimationFinished -= OnAnimationFinished;
    }
    private void OnFlareAnimationFinished()
    {
        if (_player.CureFlare.Animation == "flareIntro")
        {
            _player.CureFlare.Play("flareLoop");
        }
        else if (_player.CureFlare.Animation == "flareEnd")
        {
            _player.CureFlare.Hide();
            _player.CureFlareMask.Hide();
			_player.CureFlare.Hide();
            _player.CureFlareMask.Stop();
        }
    }
    #endregion
}

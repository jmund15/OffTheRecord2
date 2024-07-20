using Godot;
using System;

public partial class Monster : Player
{
    public Player Player { get; private set; }

    [Signal]
    public delegate void LungeEventHandler();
    [Signal]
    public delegate void DevourLimbEventHandler(int newLimbCount);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
        CurrLimbHealthState = LimbHealthState.Monster;
        HitboxComponent.HurtboxEntered += OnHurtboxEntered;
        LimbCount = 0;
        Player.LimbDetached += OnLimbDetached;

        InitStateMachine();
	}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);
	}
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
    private void OnLimbDetached()
    {
        EmitSignal(SignalName.DevourLimb, LimbCount);
    }
    private void OnHurtboxEntered(HurtboxComponent hurtbox)
    {
        if (hurtbox == HurtboxComponent)
        {
            return;
        }
        
        
    }
}

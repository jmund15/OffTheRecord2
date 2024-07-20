using Godot;
using System;

public partial class Syringe : Pickup
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void OnPickupBodyEntered(Node2D body)
    {
		if (body is not Player player ) { return; }
		player.CuresHeld++;
		QueueFree();
	}
    public override void OnPickupAreaEntered(Area2D area)
    {
		
        base.OnPickupAreaEntered(area);
    }
}

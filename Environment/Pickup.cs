using Godot;
using System;

public partial class Pickup : StaticBody2D
{
    protected Global Global { get; private set; }
    public Area2D PickupArea { get; private set; }

    public AnimatedSprite2D PickupSprite { get; private set; }
    public override void _Ready()
    {
        Global = GetNode<Global>("/root/Global");

        PickupArea = GetNode<Area2D>("PickupArea");
        PickupArea.BodyEntered += OnPickupBodyEntered;
        PickupArea.AreaEntered += OnPickupAreaEntered;

        PickupSprite = GetNode<AnimatedSprite2D>("PickupSprite");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public virtual void OnPickupBodyEntered(Node2D body)
    {
    }
    public virtual void OnPickupAreaEntered(Area2D area)
    {
    }
}

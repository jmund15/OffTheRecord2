using Godot;
using System;
using System.Collections.Generic;

public enum MovementDirection
{
	//NULL = -1,
	DOWN = 0,
	UP,
	LEFT,
	RIGHT,
	DOWNLEFT,
	DOWNRIGHT,
	UPLEFT,
	UPRIGHT
}

[GlobalClass]
public partial class TopDownCharacter : CharacterBody2D
{
	public MovementDirection FaceDirection { get; protected set; }
    public Vector2 CharacterSize { get; protected set; }

    protected readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());
	protected Global Global { get; private set; }
	protected Events SignalBus { get; private set; }
	public Sprite2D Sprite { get; protected set; }
	public AnimatedSprite2D AnimSprite { get; protected set; }
    protected Sprite2D Shadow { get; set; }
	protected Vector2 BaseShadowPos { get; private set; }
	//protected float WalkSpeed { get; set; }
	//protected float RunSpeed { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("TopDownCharacter");
		SignalBus = GetNode<Events>("/root/Events");
		Global = GetNode<Global>("/root/Global");
        Shadow = GetNode<Sprite2D>("Shadow");
		BaseShadowPos = Shadow.Position;
    }
}

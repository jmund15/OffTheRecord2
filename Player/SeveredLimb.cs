using Godot;
using System;

public partial class SeveredLimb : Node2D
{

	public AnimatedSprite2D LimbSprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LimbSprite = GetNode<AnimatedSprite2D>("Limb");
        switch (Global.Player.LimbCount)
        {
            case 3:
                LimbSprite.Play("arm"); break;
            case 2:
                LimbSprite.Play("leg1"); break;
            case 1:
                LimbSprite.Play("leg2"); break;
            default: break;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

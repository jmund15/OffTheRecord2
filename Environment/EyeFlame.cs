using Godot;
using System;

public partial class EyeFlame : AnimatedSprite2D
{
	private AnimatedSprite2D _mask;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimationFinished += OnAnimationFinished;
        _mask = GetNode<AnimatedSprite2D>("Mask");
		Hide();
	}

    private void OnAnimationFinished()
    {
		Play("loop");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public void StartEyeFlame()
	{
		Show();
		Play("startUp");
		_mask.Show();
		_mask.Play("mask");
	}
}

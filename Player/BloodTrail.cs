using Godot;
using System;

public partial class BloodTrail : AnimatedSprite2D
{
	private Global _global;

	public Color BloodColor = Colors.White;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");

		Modulate = BloodColor;
		int animNum = _global.Rnd.Next(1, 6);
		Play("blood" + animNum);
		GetTree().CreateTimer(15f).Timeout += StartFade;
	}

    private void StartFade()
    {
		var fadeTween = CreateTween();
		fadeTween.TweenProperty(this, "modulate:a", 0.0f, 15f).SetEase(Tween.EaseType.In);
		fadeTween.TweenCallback(Callable.From(QueueFree));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}

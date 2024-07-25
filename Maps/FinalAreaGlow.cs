using Godot;
using System;

public partial class FinalAreaGlow : ColorRect
{
	private Global _global;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
		Glow();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Glow()
	{
		var glowTween = CreateTween();
		glowTween.TweenInterval(0.5f);
        glowTween.TweenProperty(this, "modulate:a", 0.75f, 1.5f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
        glowTween.TweenInterval(0.25f);
        glowTween.TweenProperty(this, "modulate:a", 1.0f, 1.0f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
		glowTween.TweenCallback(Callable.From(Glow));
    }
}

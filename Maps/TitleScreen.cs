using Godot;
using System;

public partial class TitleScreen : Sprite2D
{
	private bool _onWiggle = false;
    private Vector2 _basePos;
    private Vector2 _wigglePosOffset = new Vector2(0, -5);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _basePos = Position;
        GetTree().CreateTimer(0.75f).Timeout += Wiggle;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
	private void Wiggle()
	{
        if (!_onWiggle)
        {
            _onWiggle = true;
            Position = _basePos + _wigglePosOffset;
            GetTree().CreateTimer(0.15f).Timeout += Wiggle;
        }
		else
		{
            _onWiggle = false;
            Position = _basePos;
            GetTree().CreateTimer(0.65f).Timeout += Wiggle;
        }
    }
}

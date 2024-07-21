using Godot;
using System;

public partial class PlayButton : AnimatedSprite2D
{
	private Area2D _area;
    public bool MouseIn = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_area = GetNode<Area2D>("Area2D");
		_area.MouseEntered += OnMouseEntered;
		_area.MouseExited += OnMouseExited;
	}

    private void OnMouseExited()
    {
        Play("unhover");
        MouseIn = false;
    }

    private void OnMouseEntered()
    {
        Play("hover");
        MouseIn = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (MouseIn && Input.IsMouseButtonPressed(MouseButton.Left))
        { 

        }
	}
}

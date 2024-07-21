using Godot;
using System;

public partial class MainScene : Node2D
{
	private Global _global;
	private Player _player;
	private Monster _monster;

	private Area2D _finalArea;
	private AnimatedSprite2D _eyeFlame;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
		_player = Global.Player;
		_monster = Global.Monster;

		_finalArea = GetNode<Area2D>("FinalArea");
		_finalArea.Monitoring = false;
		_finalArea.AreaEntered += OnAreaEntered;

		_monster.Position = new Vector2(546, 243);
		_monster.LimbCount = 1;
		_monster.CanMove = true;


		_player.Position = new Vector2(546, 300);
        _player.LimbCount = 3;
        _player.CanMove = false;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() is Monster monster)
		{

		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (_global.CandleGroupsCompleted == 4)
		{
			_finalArea.Monitoring = true;
		}
	}
	private void StartCutscene()
	{

	}
	private void FinalCutscene()
	{

	}
}

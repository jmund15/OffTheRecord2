using Godot;
using System;

public partial class MainScene : Node2D
{
	public bool OnTitle = true;
	public bool OnInitCutscene = false;
	public bool OnEndCutscene = false;

	private Global _global;
	private Player _player;
	private Monster _monster;

	private UI _ui;
	private Camera _camera;

	private TitleScreen _titleScreen;
	private PlayButton _playButton;
	private AnimatedSprite2D _titleMonster;

	private AnimatedSprite2D _initCutscene;

	private Vector2 _initCamPos = new Vector2(550, 245);

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


		_ui = GetNode<CanvasLayer>("CanvasLayer").GetNode<UI>("UI");
		_camera = GetNode<Camera>("Camera2D"); _camera.InCutscene = true;
		_camera.Position = new Vector2(350, -500);
		_titleScreen = _ui.GetNode<TitleScreen>("TitleScreen");
		_playButton = _ui.GetNode<PlayButton>("PlayButton");
		_titleMonster = _ui.GetNode<AnimatedSprite2D>("TitleMonster");

		_initCutscene = GetNode<AnimatedSprite2D>("InitCutscene");

        //_monster.Position = new Vector2(546, 243);
        _monster.LimbCount = 0;
        _monster.CanMove = false;
        //_player.Position = new Vector2(546, 300);
        _player.LimbCount = 4;
        _player.CanMove = false;
        _player.InCutscene = true;
		_monster.Hide();
		_player.Hide();

        AudioStreamPlayer player = GetNode("Sound").GetNode("Title").GetNode<AudioStreamPlayer>("TitleMusic");
		player.Play();
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
		if (OnTitle && _playButton.MouseIn && Input.IsMouseButtonPressed(MouseButton.Left))
		{
			_playButton.Play("play");
			TitlePlay();
		}

		if (_global.CandleGroupsCompleted == 4)
		{
			_finalArea.Monitoring = true;
		}
	}
	private void TitlePlay()
	{
        AudioStreamPlayer playerTitle = GetNode("Sound").GetNode("Title").GetNode<AudioStreamPlayer>("TitleMusic");
        playerTitle.Stop();

        AudioStreamPlayer player = GetNode("Sound").GetNode("Music").GetNode<AudioStreamPlayer>("Area3");
        player.Play();

        var startTween = CreateTween();
		startTween.TweenProperty(_playButton, "modulate:a", 0.0f, 2.0f);
		startTween.Parallel().TweenProperty(_titleScreen, "modulate:a", 0.0f, 2.0f);
		startTween.Parallel().TweenProperty(_titleMonster, "modulate:a", 0.0f, 2.0f);
		startTween.Parallel().TweenProperty(_camera, "position", _initCamPos, 4.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        startTween.Parallel().TweenProperty(_camera, "zoom", new Vector2(5.0f, 5.0f), 4.0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
		startTween.TweenCallback(Callable.From(StartCutscene));
    }
	private void StartCutscene()
	{
		GD.Print("intro cutscene now");
		OnTitle = false;
		OnInitCutscene = true;
        _initCutscene.Play("intro");
		_initCutscene.AnimationFinished += OnInitFinished;
    }
    private void OnInitFinished()
    {
		_initCutscene.Stop();
        _monster.InitCutscene = true;
		_player.DetachFirstLimb();
        _monster.Show();
        _player.Show();
        _initCutscene.Hide();
        _camera.InCutscene = false;

        _player.LoseLimb += (limbCount) =>
		{
            _player.CanMove = true;
            _player.InCutscene = false;
			OnInitCutscene = false;
        };
    }

    private void FinalCutscene()
	{

	}
}

using Godot;
using System;

public partial class Candle : StaticBody2D
{
    private Global _global;

	protected AnimatedSprite2D _animatedSprite;
	private Area2D _area;
    public PointLight2D PointLight;
    public AnimatedSprite2D FlameMask { get; private set; }

	private bool _playerInArea = false;
    private bool _candleOn = false;
    public bool CandleOn
    {
        get => _candleOn;
        set
        {
            if (_candleOn == value) return;
            _candleOn = value;
            EmitSignal(SignalName.CandleChange, _candleOn);
        }
    }
    public bool CanSwitch = true;

    [Signal]
    public delegate void CandleChangeEventHandler(bool candleOn);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _global = GetNode<Global>("/root/Global");

        _animatedSprite = GetNode<AnimatedSprite2D>("CandleFlame");
        FlameMask = _animatedSprite.GetNode<AnimatedSprite2D>("FlameMask");
        FlameMask.Hide();
        _area = GetNode<Area2D>("Area2D");
        PointLight = GetNode<PointLight2D>("PointLight2D");
        PointLight.Enabled = false;
        _animatedSprite.AnimationFinished += OnAnimationFinshed;
		_area.BodyEntered += OnBodyEntered;
        _area.AreaEntered += OnAreaEntered;
        _area.AreaExited += OnAreaExited;
    }

    private void OnAreaExited(Area2D area)
    {
        _playerInArea = false;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!CanSwitch) { return; }
        if (area.GetParent() is Monster)
        {
            CandleOff();
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (_playerInArea && Input.IsActionJustPressed("Interact") && CanSwitch)
        {
            CandleSwitch();
        }
	}

    private void OnAnimationFinshed()
    {
        if (_animatedSprite.Animation == "flameOn")
        {
            _animatedSprite.Play("flameLoop");
        }
        else
        {
        }
    }
    private void OnBodyEntered(Node2D body)
    {
        if (!CanSwitch) { return; }
        if (body is Player)
        {
            _playerInArea = true;
        }
        if (body is Monster)
        {
            CandleOff();
        }
    }
    private void CandleSwitch()
    {

        if (!CandleOn)
        {
            _animatedSprite.Play("flameOn");
            AudioStreamPlayer player = GetTree().Root.GetNode("MainScene").GetNode("Sound").GetNode("SFXDude").GetNode<AudioStreamPlayer>("sfx6");
            player.Play();
            CandleOn = true;
            PointLight.Enabled = true;

        }
        //else
        //{
        //    _animatedSprite.Play("flameOff");
        //    AudioStreamPlayer player = GetTree().Root.GetNode("MainScene").GetNode("Sound").GetNode("SFXDude").GetNode<AudioStreamPlayer>("sfx7");
        //    player.Play();
        //    CandleOn = false;
        //    PointLight.Enabled = false;

        //}
    }
    private void CandleOff()
    {
        if (!CandleOn) { return; }
        _animatedSprite.Play("flameOff");
        AudioStreamPlayer player = GetTree().Root.GetNode("MainScene").GetNode("Sound").GetNode("SFXDude").GetNode<AudioStreamPlayer>("sfx7");
        player.Play();
        CandleOn = false;
        PointLight.Enabled = false;
    }
}

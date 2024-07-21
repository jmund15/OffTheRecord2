using Godot;
using System;
using System.Collections.Generic;
using static Player;

public partial class UI : Control
{
	private Global _global;
	private Player _player;

    public ColorRect BlackOverlay;

    private TextureRect _eyeCounter;
    public static Rect2 zeroDone = new Rect2(new Vector2(0, 0), new Vector2(64, 32));
    public static Rect2 oneDone = new Rect2(new Vector2(0, 32), new Vector2(64, 32));
    public static Rect2 twoDone = new Rect2(new Vector2(0, 64), new Vector2(64, 32));
    public static Rect2 threeDone = new Rect2(new Vector2(0, 96), new Vector2(64, 32));
    public static Rect2 fourDone = new Rect2(new Vector2(0, 128), new Vector2(64, 32));

    public Dictionary<int, Rect2> CandleGroupMap = new Dictionary<int, Rect2>()
    {
        { 0, zeroDone },
        { 1, oneDone },
        { 2, twoDone },
        { 3, threeDone },
        { 4, fourDone },
    };

    private Control _bodyTop;
    private TextureRect _finalArm;
    private TextureRect _firstArm;
    private TextureRect _firstLeg;
    private TextureRect _finalLeg;

    private Dictionary<int, TextureRect> _bodyMap = new Dictionary<int, TextureRect>();


    private Control _syringeTop;
	private List<AtlasTexture> _syringeUIs = new List<AtlasTexture>();

    
    public static readonly Color fullHex = new Color("7D5D47");
    public static readonly Color OofHex = new Color("928670");
    public static readonly Color UhohHex = new Color("456755");
    public static readonly Color JoeverHex = new Color("697681");
    public static readonly Color GoneHex = new Color("5C151D");

    private Dictionary<Player.LimbHealthState, Color> _bodyColorMap = new Dictionary<Player.LimbHealthState, Color>()
    {
        { LimbHealthState.Full, fullHex },
        { LimbHealthState.Oof, OofHex },
        { LimbHealthState.Uhoh, UhohHex },
        { LimbHealthState.Joever, JoeverHex }
    };

    public static Rect2 FullSyringeRegion = new Rect2(new Vector2(32, 0), new Vector2(32, 32));
    public static Rect2 EmptySyringeRegion = new Rect2(new Vector2(0, 0), new Vector2(32, 32));
    // Called when the node enters the scene tree for the first time.

    private ColorRect _gameOver;
    public ColorRect WinGame;

    private TextureRect _damageOverlay;
    private AtlasTexture _damageAnim;
    private float _timePer = 0.1f;
    private float _timeStay = 0.5f;
    private float _timeFade = 0.5f;

    private List<Rect2> _damAnimRegions = new List<Rect2>()
    {
        new Rect2(new Vector2(0, 0), new Vector2(1152, 648)),
        new Rect2(new Vector2(1152, 0), new Vector2(1152, 648)),
        new Rect2(new Vector2(1152 * 2, 0), new Vector2(1152, 648)),
        new Rect2(new Vector2(1152 * 3, 0), new Vector2(1152, 648))
    };
    private List<Rect2> _damHalfAnimRegions = new List<Rect2>()
    {
        new Rect2(new Vector2(0, 0), new Vector2(1152, 648)),
        new Rect2(new Vector2(1152, 0), new Vector2(1152, 648))
    };
    private Rect2 _blankReg = new Rect2();
    public void PlayDamageOverlay()
    {
        _damageAnim.Region = _blankReg;
        _damageOverlay.Modulate = Colors.White;
        _damageOverlay.Show();
        var damTween = CreateTween();
        foreach (var region in _damAnimRegions)
        {
            damTween.TweenProperty(_damageAnim, "region", region, 0.0f).SetDelay(_timePer);
        }
        damTween.TweenProperty(_damageOverlay, "modulate:a", 0.0f, _timeFade).SetDelay(_timeStay);
    }
    public void PlayHalfDamageOverlay()
    {
        _damageAnim.Region = _blankReg;
        _damageOverlay.Modulate = Colors.White;
        _damageOverlay.Show();
        var damTween = CreateTween();
        foreach (var region in _damHalfAnimRegions)
        {
            damTween.TweenProperty(_damageAnim, "region", region, 0.0f).SetDelay(_timePer);
        }
        damTween.TweenProperty(_damageOverlay, "modulate:a", 0.0f, _timeFade).SetDelay(_timeStay / 2f);
    }
    public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
		_player = Global.Player;

        BlackOverlay = GetNode<ColorRect>("BlackOverlay");
        BlackOverlay.Hide();

        _eyeCounter = GetNode<TextureRect>("EyeCounter");
        _global.CandleGroupComplete += OnCandleGroupComplete;

        _bodyTop = GetNode<Control>("BodyUI");
        _firstArm = _bodyTop.GetNode<TextureRect>("FirstArm");
        _finalArm = _bodyTop.GetNode<TextureRect>("FinalArm");
        _firstLeg = _bodyTop.GetNode<TextureRect>("FirstLeg");
        _finalLeg = _bodyTop.GetNode<TextureRect>("FinalLeg");

        _bodyMap.Add(4, _firstArm);
        _bodyMap.Add(3, _firstLeg);
        _bodyMap.Add(2, _finalLeg);
        _bodyMap.Add(1, _finalArm);
        foreach (var text in _bodyMap.Values) 
        {
            text.Modulate = fullHex;
        }

        _player.LimbHealthStateChange += OnLimbHealthStateChange;
        _player.LimbDetached += OnLimbDetached;
        _player.LoseLimb += OnLoseLimb;

        _syringeTop = GetNode<Control>("SyringeUI");
		for (int i = 1; i <= 5; i++)
		{
			var syringeText = _syringeTop.GetNode<TextureRect>("Syringe" + i).Texture;
            _syringeUIs.Add(syringeText as AtlasTexture);
        }
        SetCureUIs(_player.CuresHeld);
        _player.NumCuresChanged += SetCureUIs;

        _gameOver = GetNode<ColorRect>("GameOver");
        _gameOver.Hide();

        WinGame = GetNode<ColorRect>("WinGame");
        WinGame.Hide();

        _damageOverlay = GetNode<TextureRect>("DamageOverlay");
        _damageAnim = _damageOverlay.Texture as AtlasTexture;
        _damageOverlay.Hide();
    }

    private void OnLoseLimb(int newLimbCount)
    {
        if (newLimbCount == 0)
        {
            _gameOver.Show();
            GetTree().CreateTimer(1.5f).Timeout += () =>
            {
                GetTree().ReloadCurrentScene();
            };
        }
    }

    private void OnCandleGroupComplete()
    {
        (_eyeCounter.Texture as AtlasTexture).Region = CandleGroupMap[_global.CandleGroupsCompleted];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    private void OnLimbHealthStateChange(Player.LimbHealthState newLimbHealthState, bool damaged)
    {
        _bodyMap[_player.LimbCount].Modulate = _bodyColorMap[newLimbHealthState];
        if (damaged) { PlayHalfDamageOverlay(); GD.Print("damage overlay"); }
    }
    private void OnLimbDetached(SeveredLimb limb)
    {
        PlayDamageOverlay();
        _bodyMap[_player.LimbCount + 1].Modulate = GoneHex;
    }

    private void SetCureUIs(int curesHeld)
    {
        for (int i = 0; i < 5;i++) 
		{
			if (i < curesHeld)
			{
                _syringeUIs[i].Region = FullSyringeRegion;
            }
			else
			{
                _syringeUIs[i].Region = EmptySyringeRegion;
            }
        }
    }



}

using Godot;
using System;
using System.Collections.Generic;
using static Player;

public partial class UI : Control
{
	private Global _global;
	private Player _player;

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
    public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
		_player = Global.Player;

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

        _syringeTop = GetNode<Control>("SyringeUI");
		for (int i = 1; i <= 5; i++)
		{
			var syringeText = _syringeTop.GetNode<TextureRect>("Syringe" + i).Texture;
            _syringeUIs.Add(syringeText as AtlasTexture);
        }
        SetCureUIs(_player.CuresHeld);
        _player.NumCuresChanged += SetCureUIs;
	}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    private void OnLimbHealthStateChange(Player.LimbHealthState newLimbHealthState)
    {
        _bodyMap[_player.LimbCount].Modulate = _bodyColorMap[newLimbHealthState];
    }
    private void OnLimbDetached(SeveredLimb limb)
    {
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

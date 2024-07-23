 using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * 
 */
public partial class Global : Node
{
    public MainScene MainScene { get; private set; }
    public static Player Player { get; private set; }
    public static Monster Monster { get; private set; }


    public Events SignalBus { get; private set; }
    public readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());

    public const float LowestPixelSize = 0.01f;



    public int CandleGroupsCompleted;
    private Node2D _candleGroup1;
    public bool _candle1Fin;
    private Sprite2D _group1Path;
    private Node2D _candleGroup2;
    public bool _candle2Fin;
    private Sprite2D _group2Path;
    private Node2D _candleGroup3;
    public bool _candle3Fin;
    private Sprite2D _group3Path;
    private Node2D _candleGroup4;
    public bool _candle4Fin;
    private Sprite2D _group4Path;

    private Node2D _eyeFlames1;
    private Node2D _eyeFlames2;
    private Node2D _eyeFlames3;
    private List<EyeFlame> _group1EyeFlames;
    private List<EyeFlame> _group2EyeFlames;
    private List<EyeFlame> _group3EyeFlames;

    public List<Candle> Group1Candles { get; private set; }
    public List<Candle> Group2Candles { get; private set; }
    public List<Candle> Group3Candles { get; private set; } 
    public List<Candle> Group4Candles { get; private set; }

    public bool Reset = false;

    [Signal]
    public delegate void CandleGroupCompleteEventHandler();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //GD.Print("roll force range: ", RollForceRange);
        SignalBus = GetNode<Events>("/root/Events");
        MainScene = GetNode<MainScene>("/root/MainScene");
        Player = MainScene.GetNode<Player>("Player");
        Monster = MainScene.GetNode<Monster>("Monster");
        Monster.ProtagRef = Player;


       CandleGroupsCompleted = 0;
        _candle1Fin = false;
        _candle2Fin = false;
        _candle3Fin = false;
        _candle4Fin = false;
    
        _group1EyeFlames = new List<EyeFlame>();
        _group2EyeFlames = new List<EyeFlame>();
        _group3EyeFlames = new List<EyeFlame>();

        Group1Candles = new List<Candle>();
        Group2Candles = new List<Candle>();
        Group3Candles = new List<Candle>();
        Group4Candles = new List<Candle>();


        _candleGroup1 = MainScene.GetNode<Node2D>("CandleGroup1");
        _group1Path = MainScene.GetNode<Sprite2D>("Map1");
        foreach (var child in _candleGroup1.GetChildren())
        {
            Group1Candles.Add(child as Candle);
            (child as Candle).CandleChange += OnCandleChange;
        }

        _candleGroup2 = MainScene.GetNode<Node2D>("CandleGroup2");
        _group2Path = MainScene.GetNode<Sprite2D>("Map2");
        foreach (var child in _candleGroup2.GetChildren())
        {
            Group2Candles.Add(child as Candle);
            (child as Candle).CandleChange += OnCandleChange;

        }

        _candleGroup3 = MainScene.GetNode<Node2D>("CandleGroup3");
        _group3Path = MainScene.GetNode<Sprite2D>("Map3");
        foreach (var child in _candleGroup3.GetChildren())
        {
            Group3Candles.Add(child as Candle);
            (child as Candle).CandleChange += OnCandleChange;

        }

        _candleGroup4 = MainScene.GetNode<Node2D>("CandleGroup4");
        _group4Path = MainScene.GetNode<Sprite2D>("Map4");
        foreach (var child in _candleGroup4.GetChildren())
        {
            Group4Candles.Add(child as Candle);
            (child as Candle).CandleChange += OnCandleChange;
        }

        _eyeFlames1 = MainScene.GetNode<Node2D>("Group1Eyes");
        _eyeFlames2 = MainScene.GetNode<Node2D>("Group2Eyes");
        _eyeFlames3 = MainScene.GetNode<Node2D>("Group3Eyes");
        foreach (var child in _eyeFlames1.GetChildren())
        {
            _group1EyeFlames.Add(child as EyeFlame);
        }
        foreach (var child in _eyeFlames2.GetChildren())
        {
            _group2EyeFlames.Add(child as EyeFlame);
        }
        foreach (var child in _eyeFlames3.GetChildren())
        {
            _group3EyeFlames.Add(child as EyeFlame);
        }
    }

    private void OnCandleChange(bool candleOn)
    {
        if (candleOn)
        {
            if (!_candle1Fin)
            {
                bool groupFull = true;
                foreach (var candle in Group1Candles)
                {
                    if (!candle.CandleOn)
                    {
                        groupFull = false;
                        break;
                    }
                }
                if (groupFull)
                {
                    _candle1Fin = true;
                    CandleGroupsCompleted++;
                    EmitSignal(SignalName.CandleGroupComplete);
                    _group1Path.Modulate = new Color("570627");
                    foreach (var candle in Group1Candles)
                    {
                        candle.CanSwitch = false;
                        candle.FlameMask.Show();
                    }
                    foreach (var eye in _group1EyeFlames)
                    {
                        eye.StartEyeFlame();
                    }
                }
            }
            if (!_candle2Fin)
            {
                bool groupFull = true;
                foreach (var candle in Group2Candles)
                {
                    if (!candle.CandleOn)
                    {
                        groupFull = false;
                        break;
                    }
                }
                if (groupFull)
                {
                    _candle2Fin = true;
                    CandleGroupsCompleted++;
                    EmitSignal(SignalName.CandleGroupComplete);
                    _group2Path.Modulate = new Color("570627");
                    foreach (var candle in Group2Candles)
                    {
                        candle.CanSwitch = false;
                        candle.FlameMask.Show();
                    }
                    foreach (var eye in _group2EyeFlames)
                    {
                        eye.StartEyeFlame();
                    }
                }
            }
            if (!_candle3Fin)
            {
                bool groupFull = true;
                foreach (var candle in Group3Candles)
                {
                    if (!candle.CandleOn)
                    {
                        groupFull = false;
                        break;
                    }
                }
                if (groupFull)
                {
                    _candle3Fin = true;
                    CandleGroupsCompleted++;
                    _group3Path.Modulate = new Color("570627");
                    EmitSignal(SignalName.CandleGroupComplete);
                    foreach (var candle in Group3Candles)
                    {
                        candle.CanSwitch = false;
                        candle.FlameMask.Show();
                    }
                    foreach (var eye in _group3EyeFlames)
                    {
                        eye.StartEyeFlame();
                    }
                }
            }
            if (!_candle4Fin)
            {
                bool groupFull = true;
                foreach (var candle in Group4Candles)
                {
                    if (!candle.CandleOn)
                    {
                        groupFull = false;
                        break;
                    }
                }
                if (groupFull)
                {
                    CandleGroupsCompleted++;
                    _candle4Fin = true;
                    _group4Path.Modulate = new Color("570627");
                    EmitSignal(SignalName.CandleGroupComplete);
                    foreach (var candle in Group4Candles)
                    {
                        candle.CanSwitch = false;
                        candle.FlameMask.Show();
                        candle.PointLight.Color = new Color("e8abb0");
                    }
                }
            }
        }
    }

    public void FreezeEffect(float freezeTime)
    {
        MainScene.GetTree().Paused = true;
        GetTree().CreateTimer(freezeTime).Timeout += () =>
        {
            MainScene.GetTree().Paused = false;
        };
    }
    public void AddCameraTrauma(float trauma)
    {
        //_leadCrown.AddCamTrauma(trauma);
    }
    public Vector2 GetRandomDirection()
    {
        var direction = new Vector2();
        var randNeg = Rnd.Next(0, 4);
        switch (randNeg)
        {
            case 0:
                direction = new Vector2(Rnd.NextSingle(), Rnd.NextSingle()); //(MovementDirection)Rnd.Next(0, 4);
                break;
            case 1:
                direction = new Vector2(-Rnd.NextSingle(), Rnd.NextSingle());
                break;
            case 2:
                direction = new Vector2(Rnd.NextSingle(), -Rnd.NextSingle());
                break;
            case 3:
                direction = new Vector2(-Rnd.NextSingle(), -Rnd.NextSingle());
                break;
            default:
                break;
        }
        return direction;
    }
    public static IEnumerable<T> GetEnumValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
    public static float NormalizeNumber(float num, float minNum, float maxNum)
    {
        return (num - minNum) / (maxNum - minNum);
    }
    public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        Vector2 q0 = p0.Lerp(p1, t);
        Vector2 q1 = p1.Lerp(p2, t);
        Vector2 r = q0.Lerp(q1, t);
        return r;
    }
}

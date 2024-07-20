using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Map : Node2D
{
	public List<string> ConnectedMaps { get; protected set; }

    protected Global Global { get; private set; }
    protected Events SignalBus { get; private set; }
    protected MainScene MainScene { get; private set; }
    protected string LoadMapSignalName { get; private set; }
    protected string UnloadMapSignalName { get; private set; }
    [Export]
    protected Godot.Collections.Dictionary<int, Vector2> RobberStartingPoses { get; set; } = new Godot.Collections.Dictionary<int, Vector2>()
    {
        { 1, new Vector2(-320, -160) },
        { 2, new Vector2(320, -160) },
        { 3, new Vector2(-320, 160) },
        { 4, new Vector2(320, 160) }
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddToGroup("Map");
        SignalBus = GetNode<Events>("/root/Events");
        Global = GetNode<Global>("/root/Global");
        MainScene = GetNode<MainScene>("//root/MainScene");

        LoadMapSignalName = Events.SignalName.LoadMap;
        UnloadMapSignalName = Events.SignalName.UnloadMap;

        //RobberStartingPoses.Add(1, new Vector2(-320, -160));
        //RobberStartingPoses.Add(2, new Vector2(320, -160));
        //RobberStartingPoses.Add(3, new Vector2(-320, 160));
        //RobberStartingPoses.Add(4, new Vector2(320, 160));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    protected virtual void StartMap()
    {
        //foreach (var robberPair in Global.Robbers)
        //{
        //    //if (robberPair.Value != null) // isn't possible right????
        //    //{
        //        robberPair.Value.Position = RobberStartingPoses[robberPair.Key];
        //    //}
        //}
    }
}

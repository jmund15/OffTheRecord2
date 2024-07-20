using Godot;
using System;
public partial class Events : Node
{
	//[Signal]
	//public delegate void LoadMapEventHandler(string mapPath);

	[Signal]
	public delegate void PlayerAddEventHandler(int playerNum);
	[Signal] 
	public delegate void PlayerRemoveEventHandler(int playerNum);
	[Signal]
	public delegate void LoadMapEventHandler(PackedScene mapScene);
    [Signal]
    public delegate void UnloadMapEventHandler(Map map);
    [Signal]
    public delegate void ShowMapEventHandler(Map map);
    [Signal]
    public delegate void HideMapEventHandler(Map map);

    public override void _Ready()
	{
	}
	public override void _Process(double delta)
	{
	}
}

using Godot;
using System;
using System.Collections.Generic;

public partial class Camera : Camera2D
{
    private Global _global;
    private Events _signalBus;
    private MainScene _mainScene;

    public bool InCutscene = false;
    public static Rect2 CameraBounds { get; private set; }

    [Export]
    private float _normSmoothingSpeed = 6.0f;
    [Export]
    private float _transitionSmoothingSpeed = 2.5f;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _global = GetNode<Global>("/root/Global");
        _signalBus = GetNode<Events>("/root/Events");
        _mainScene = GetNode<MainScene>("/root/MainScene");
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (InCutscene) { return; }
        Position = Global.Player.Position;
        
    }
    
}

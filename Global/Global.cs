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

    public const string LeftAction = "leftPlayer";
    public const string RightAction = "rightPlayer";
    public const string UpAction = "upPlayer";
    public const string DownAction = "downPlayer";
    public const string AttackAction = "attackPlayer";
    public const string DefendAction = "defendPlayer";
    public const string LeapAction = "leapPlayer";
    public const string ThrowAction = "throwPlayer";
    public const string ItemAction = "itemPlayer";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //GD.Print("roll force range: ", RollForceRange);
        SignalBus = GetNode<Events>("/root/Events");
        MainScene = GetNode<MainScene>("/root/MainScene");
        Player = MainScene.GetNode<Player>("Player");
        Monster = MainScene.GetNode<Monster>("Monster");
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

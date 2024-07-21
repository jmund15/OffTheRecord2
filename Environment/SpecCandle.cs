using Godot;
using System;

public partial class SpecCandle : Candle
{
	private Global _global;
	[Export]
	private int _pathNum;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_global = GetNode<Global>("/root/Global");
		base._Ready();
		CanSwitch = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch(_pathNum)
		{
			case 1:
				if (_global._candle1Fin)
				{
					SpecCandleOn();
				}
				break;
            case 2:
                if (_global._candle2Fin)
                {
                    SpecCandleOn();
                }
                break;
            case 3:
                if (_global._candle3Fin)
                {
                    SpecCandleOn();
                }
                break;
        }
	}
    private void SpecCandleOn()
    {
        _animatedSprite.Play("flameOn");
        CandleOn = true;
        PointLight.Enabled = true;
    }
}

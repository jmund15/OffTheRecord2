using Godot;
using System;
using System.Collections.Generic;
using TimeRobbers.BaseComponents;

public partial class Player : BasePlayer, IDirectionComponent
{
    #region CLASS_VARIALBLES
    public enum LimbHealthState
    {
        Full,
        Oof,
        Uhoh,
        Joever,
        Monster
    }
    public HealthComponent HealthComponent { get; protected set; }
    public float LimbHealthAmt { get; protected set; }
    public float CurrLimbHealth { get; protected set; }
    public float LimbHealthStateAmt { get; protected set; }
    public LimbHealthState CurrLimbHealthState { get; protected set; } = LimbHealthState.Full;
    public Dictionary<LimbHealthState, string> LimbHealthAnimString { get; protected set; } = new Dictionary<LimbHealthState, string>()
    {
        { LimbHealthState.Full, "Full" },
        { LimbHealthState.Oof, "Oof" },
        { LimbHealthState.Uhoh, "Uhoh" },
        { LimbHealthState.Joever, "Joever" },
        { LimbHealthState.Monster, "" }
    };

    [Export]
    public float AfflictionRate { get; protected set; } = 25f;

    private int _curesHeld = 1;
    public int CuresHeld {
        get { return _curesHeld; }
        set
        {
            if (value == _curesHeld) return;
            else
            {
                _curesHeld = value;
                EmitSignal(SignalName.NumCuresChanged, _curesHeld);
            }
        }
    }
    private bool _healing = false;
    public bool Healing
    {
        get => _healing;
        set
        {
            if (_healing == value) return;
            else
            {
                _healing = value;
                EmitSignal(SignalName.HealingStateChange, _healing);
            }
        }
    }
    [Export]
    public float CureSpeed { get; protected set; } = 50f;
    public AnimatedSprite2D CureFlare { get; protected set; }
    public AnimatedSprite2D CureFlareMask { get; protected set; }
    //public const float WalkMinInput = 0.1f;
    public string LeftInput { get; protected set; } = "Left";
    public string RightInput { get; protected set; } = "Right";
    public string UpInput { get; protected set; } = "Up";
    public string DownInput { get; protected set; } = "Down";
    public string InjectInput { get; protected set; } = "Inject";
    public string InteractInput { get; protected set; } = "Interact";
    [Export]
    public PackedScene SeveredLimbScene { get; protected set; }
    [Signal]
    public delegate void LoseLimbEventHandler(int newLimbCount);
    [Signal]
    public delegate void LimbDetachedEventHandler(SeveredLimb limb);
    [Signal]
    public delegate void LimbHealthStateChangeEventHandler(LimbHealthState newLimbHealthState);
    [Signal]
    public delegate void NumCuresChangedEventHandler(int curesHeld);
    [Signal]
    public delegate void HealingStateChangeEventHandler(bool isHealing);
    #endregion

    #region BASE_OVERRIDDEN_GODOT_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        LimbCount = 3;
        HealthComponent = GetNode<HealthComponent>("HealthComponent");

        HurtboxComponent.HitboxEntered += OnHitboxEntered;

        CureFlare = GetNode<AnimatedSprite2D>("CureFlare");
        CureFlareMask = CureFlare.GetNode<AnimatedSprite2D>("CureFlareMask");
        CureFlare.Hide();
        CureFlareMask.Hide();

        HealthComponent.HealthChanged += OnHealthChanged;
        LimbHealthAmt = HealthComponent.MaxHealth / LimbCount;
        CurrLimbHealth = LimbHealthAmt;
        LimbHealthStateAmt = LimbHealthAmt / 4;
        CurrLimbHealthState = LimbHealthState.Full;

        InitStateMachine();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (CanMove)
        {
            HealthComponent.Damage(AfflictionRate * (float)delta);
        }
    }
    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        _stateMachine.HandleInput(@event);
    }
    #endregion
    #region SIGNAL_LISTENERS

    private void OnHealthChanged(HealthUpdate healthUpdate)
    {
        CurrLimbHealth = LimbHealthAmt - (healthUpdate.MaxHealth - healthUpdate.NewHealth);
        if (CurrLimbHealth <= 0)
        {
            GD.Print("total health: ", healthUpdate.NewHealth, "\nlimb health: ", CurrLimbHealth);
            //PlayerLoseLimb();
            LimbCount--;
            if (LimbCount == 0)
            {
                GameOver();
            }
            //GD.Print("before set max health");
            //HealthComponent.SetHea(healthUpdate.MaxHealth - LimbHealthAmt);
            HealthComponent.SetMaxHealth(healthUpdate.MaxHealth - LimbHealthAmt);
            //GD.Print("set max health");
            EmitSignal(SignalName.LoseLimb, LimbCount);
        }
        else
        {
            switch (CurrLimbHealth)
            {
                case float n when n >= LimbHealthStateAmt * 3:
                    if (CurrLimbHealthState == LimbHealthState.Full) { return; }
                    CurrLimbHealthState = LimbHealthState.Full;
                    break;
                case float n when n >= LimbHealthStateAmt * 2:
                    if (CurrLimbHealthState == LimbHealthState.Oof) { return; }
                    CurrLimbHealthState = LimbHealthState.Oof;

                    break;
                case float n when n >= LimbHealthStateAmt:
                    if (CurrLimbHealthState == LimbHealthState.Uhoh) { return; }
                    CurrLimbHealthState = LimbHealthState.Uhoh;
                    break;
                default: // lowest limb health state
                    if (CurrLimbHealthState == LimbHealthState.Joever) { return; }
                    CurrLimbHealthState = LimbHealthState.Joever;
                    break;
            }
            FlashRed();
            EmitSignal(SignalName.LimbHealthStateChange, Variant.From(CurrLimbHealthState));
        }
    }
    private void OnHitboxEntered(HitboxComponent hitbox)
    {
        if (HurtboxComponent.Invulnerable) { return; }
        LimbCount--;
        if (LimbCount == 0)
        {
            GameOver();
        }
        //GD.Print("before set max health");
        //HealthComponent.SetHea(healthUpdate.MaxHealth - LimbHealthAmt);
        HealthComponent.SetMaxHealth(HealthComponent.MaxHealth - LimbHealthAmt);
        //GD.Print("set max health");
        EmitSignal(SignalName.LoseLimb, LimbCount);
        HurtboxComponent.Invulnerable = true;
        GetTree().CreateTimer(2.5f).Timeout += () =>
        {
            HurtboxComponent.Invulnerable = false;
        };
    }
    public override void OnTransitionedState(State oldState, State newState)
    {
        //throw new NotImplementedException();
    }
    #endregion
    #region HELPER_FUNCTIONS
    private void PlayerLoseLimb()
    {
    }
    private void FlashRed()
    {

    }
    public override float CalcMovementSpeed()
    {
        return base.CalcMovementSpeed();
        //switch (LimbCount)
        //{
        //    case 4:
        //        return 1;
        //    case 3:
        //        return 0.8f;
        //    case 2:
        //        return 0.6f;
        //    case 1:
        //        return 0.4f;
        //    default:
        //        throw new Exception("LIMB COUNT ERROR");
        //}
    }
    public void GameOver()
    {
        QueueFree();
    }
    public void UpdateShadowPos()
    {
        //switch (FaceDirection)
        //{
        //    case MovementDirection.RIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(7.5f, 0); break;
        //    case MovementDirection.DOWNRIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(5f, 0); break;
        //    case MovementDirection.DOWN:
        //        Shadow.Position = BaseShadowPos + new Vector2(0, 0); break;
        //    case MovementDirection.DOWNLEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-5f, 0); break;
        //    case MovementDirection.LEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-7.5f, 0); break;
        //    case MovementDirection.UPLEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-5f, -2.5f); break;
        //    case MovementDirection.UP:
        //        Shadow.Position = BaseShadowPos + new Vector2(0f, -5f); break;
        //    case MovementDirection.UPRIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(5f, -2.55f); break;
        //    default:
        //        GD.PrintErr("ERROR || non-standard current y framecoord: " + Sprite.FrameCoords.Y); break;
        //}
    }
    //TODO: CHANGE TO RETURN VARIABLE INSTEAD OF CALCULATING EACH TIME FOR OPTIMIZING????
    public override Vector2 GetDesiredDirection()
    {
        //GD.Print(Input.GetVector(LeftInput, RightInput, UpInput, DownInput));
        return Input.GetVector(LeftInput, RightInput, UpInput, DownInput);
    }
    public override Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }
    #endregion
}
